using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Text.Json;
using WebHook.DTOs;
using WebHook.Enums;
using WebHook.Error;
using WebHook.interfaces;

namespace WebHook
{
    public class Services : IServiceInterface
    {
        public async Task SaveToDb(JsonElement json)
        {
            var eventName = GetEventName(json);

            var convertedBody = System.Text.RegularExpressions.Regex.Unescape(json.ToString());

            if (eventName == EventName.chat_finished.ToString())
            {
                var chatFinished = JsonConvert.DeserializeObject<chat_finished>(json.ToString());

                if (chatFinished.chat == null)
                    return;

                //var topic = chatFinished.topic is null ? "Тема не выбрана" : chatFinished.topic.title;

                await SendToQolio(chatFinished, "");
            }

            Log.Information(convertedBody);
        }

        public async Task SendToQolio(chat_finished chat_Finished, string topic)
        {
            var clientTempIdentity = chat_Finished.visitor.name ?? chat_Finished.visitor.phone;

            var plainText = chat_Finished.chat.messages.Split("\n").ToList();

            var messages = plainText.Take(plainText.Count - 1).ToList();

            var conversation = await RefactoringConversation(messages);
            var commPart = new List<CommunicationPart>();

            //string author = String.Empty;


            foreach (var item in conversation)
            {
                var author = item.Name == "visitor" ? "client" : "operator";

                commPart.Add(new CommunicationPart
                {
                    communication_part_id = Guid.NewGuid().ToString(),
                    author = new Author
                    {
                        id = author == "client" ? null : chat_Finished.agents != null ? chat_Finished.agents.id : "0d620a73-6190-49fc-93e6-4bc86d9a29cf",
                        type = author 
                    },
                    body = item.Message,
                    created_at = DateTime.Now.AddMinutes(- 0.5),
                    content_type = "text/plain"
                });

            }

            var customFields = new CustomFields
            {
                chat_id = chat_Finished.chat_id,
                client_name = chat_Finished.visitor.name ?? "No Content",
                phone_number = chat_Finished.visitor.phone ?? "No Content",
                client_rate = chat_Finished.rate ?? "Нет оценки",
                url = chat_Finished.page.url ?? "Нет данных"
            };

            var root = new Root
            {
                client_id = chat_Finished.visitor.number.ToString(),// 58097722
                communication_type = "chat",
                title = chat_Finished.topic == null ? "Нет темы обращения" : chat_Finished.topic.title,
                source = "chat",
                nps = 8,
                client_feedback = 1,
                custom_fields = customFields,
                status = "closed",
                direction = "incoming",
                started_at = DateTime.Now,
                email = clientTempIdentity,
                communication_parts = commPart,
                operator_id = chat_Finished.agents != null ? chat_Finished.agents.id : "0d620a73-6190-49fc-93e6-4bc86d9a29cf"
            };

            var serialisedRoot = Newtonsoft.Json.JsonConvert.SerializeObject(root);

            var query = await SendPostQuery(serialisedRoot);

            if (query.Code == HttpStatusCode.NotFound)
            {
                await IntegrationAddNewStaff(chat_Finished.agents);

                await SendPostQuery(serialisedRoot);
            }
            else if (query.Code == HttpStatusCode.BadRequest)
                throw new Exception();
        }

        private async Task<List<MessageObj>> RefactoringConversation(List<string> inputText)
        {
            var lines = new List<MessageObj>();

            var userName = string.Empty;
            var text = string.Empty;
            var communicator = string.Empty;

            foreach (var input in inputText)
            {
                var splitTextMessage = input.IndexOf(':');

                if(splitTextMessage is not -1)
                    userName = input.Substring(0, splitTextMessage);

                var spltMessage = input.Split($"{userName}: ").ToList();

                if (spltMessage.Count > 1)
                {
                    text = spltMessage[1];
                    communicator = userName is "visitor" ? "Клиент" : userName is "bot" ? "Гулчехра Бот" : userName;

                    lines.Add(new MessageObj { Name = userName, Message = communicator + "\n\n" + text });
                }
                else if (spltMessage.Count == 1)
                {
                    text = text + "\n" + spltMessage[0];
                    lines[lines.Count - 1].Message = communicator + "\n\n" + text;
                }
            }

            return lines;
        }

        //private string GetCommunicator(string userName)
        //{
        //    return userName switch
        //    {
        //        "visitor" => "Клиент",
        //        "bot" => "Гулчехра Бот",
        //        _ => userName,
        //    };
        //}

        private async Task<ResponseMessage> SendPostQuery(string json)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prod1.qolio.ru/api/v1/integrations/0d620a73-6190-49fc-93e6-4bc86d9a29cf/text");
            request.Headers.Add("Authorization", GetAuthToken());
            var content = new StringContent(json, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK && responseContent.Contains("Необходимо, чтобы был определен активный оператор"))
            {
                Log.Warning("Staff was not found. Trying to add a staff ....");
                return new ResponseMessage { Code = HttpStatusCode.NotFound, Message = responseContent };
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error(responseContent);
                return new ResponseMessage { Code = HttpStatusCode.BadRequest, Message = responseContent };
            }
                

            return new ResponseMessage { Code = HttpStatusCode.OK, Message = responseContent };
        }

        private async Task IntegrationAddNewStaff(Agents agent)
        {
            var bearer = await SignIn();

            var getStaffList = await GetAllStaffs(bearer);

            getStaffList.Add(new Datum { email = agent.email, first_name = agent.name, last_name = "XXX", integration_uid = agent.id });

            var json = await GenerateJson(getStaffList);

            await SaveStaffs(json, bearer);
        }

        private async Task<string> GenerateJson(List<Datum> datas)
        {
            var data = new StaffIntegrationDTO { data = datas };
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }


        public async Task<string> SignIn()
        {
            var adminData = await GetAdminData();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prod1.dealapp.io/auth/sign_in");
            var content = new StringContent($"{{\"email\":\"{adminData.Login}\",\"password\":\"{adminData.Password}\"}}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);


            return response.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");
        }

        public async Task<List<Datum>> GetAllStaffs(string bearerToken)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.prod1.qolio.ru/api/v1/api_integrations/0d620a73-6190-49fc-93e6-4bc86d9a29cf/user_account_bindings_csv");
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
  
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var ResponseContent = await response.Content.ReadAsStringAsync();

            var content = ResponseContent.ToString().Split('\n').ToList();

            var result = content.Skip(1).Take(content.Count - 2);

            var datas = new List<Datum>();

            foreach (var item in result)
            {
                var splitData = item.Split(',').ToList();
                datas.Add(new Datum
                {
                    email = splitData[0],
                    last_name = splitData[1],
                    first_name = splitData[2],
                    integration_uid = splitData[3]
                });
            }

            
            return datas;
        }

        private async Task SaveStaffs(string Json, string bearerToken)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, "https://api.prod1.qolio.ru/api/v1/api_integrations/0d620a73-6190-49fc-93e6-4bc86d9a29cf/import_user_account_bindings");
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            var content = new StringContent(Json, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);

            Log.Information(await response.Content.ReadAsStringAsync());
        }

        private async Task<admin_dataDto> GetAdminData()
        {
            var env = Environment.GetEnvironmentVariable("Sis_Admin");
            var splt = env.Split(new char[] { '=', ';' });

            return new admin_dataDto { Login = splt[1], Password = splt[3] };
        }

        private string? GetAuthToken() => Environment.GetEnvironmentVariable("AUTH_TOKEN");


        private string? GetEventName(JsonElement element)
        {
            string? eventName;
            try
            {
                return element.GetProperty("event_name").ToString();
            }
            catch (/* Надо тут логгер поставить */ Exception ex)
            {
                return null;
            }
        }

        private async Task<DateTime> ToDateTime(int timeStamp)
                => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp).ToLocalTime();
    }
}



