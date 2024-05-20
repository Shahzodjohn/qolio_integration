using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ngrok.AgentAPI;
using Serilog;
using System;
using System.Text;
using System.Text.Json;
using WebHook.DTOs;
using WebHook.Enums;
using WebHook.Models;

namespace WebHook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WeatherForecastController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> OK()
        {
            //await Console.Out.WriteLineAsync(" Салом, ман ёрдамчии виртуалӣ. Саволи худро пурра дар як матн нависед");
            var env = Environment.GetEnvironmentVariable("AUTH_TOKEN");

            Log.Information(" => Calling a service: {ServiceName}", "NameOfService");


            //var env = Environment.GetEnvironmentVariable("Sis_Admin");
            var splt = env.Split(new char[] { '=', ';' });

            var login = splt[1];
            var pass = splt[3];


            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prod1.dealapp.io/auth/sign_in");
            //request.Headers.Add("Cookie", "_interslice_session=6dZbt4GU5M%2F29yeeCMvQ%2BO5JL5ABdSTF0Y7yIhKYOQciLvEwfb%2BcVU1ZQFjFdw%2BQ%2Fj5ax1f%2FrpRXHbkl6mS04HL7LiPuGA%2Bh24XU7TMIxC%2F4X%2BdnMcM8DpW%2FRtrBAYZW446IQieOjpddFjVG8nLFEGOxFvfJUgnmeuNmlmrcX0nXIaCXFZjVrpevtBhvs52dgNNcDl7wJKzUjLV5OimcgQcCb8G5TbSWtmCpn6Tm1vNiGOoQYIJwwzulcvWbWYqOG9a9CwuMa9JrnNg6GfSpshwbFNOf99UQBOiYQHwExQfZkCZHKcJpTroG1q548wl67wYhZJIS%2FPupLKt3oBSiUxiOPx2qKEL8Vk3HuxI9SL86i%2B%2BUnj1NM%2BzeflWTQC0%3D--XhtSqp1DBflp4Qme--uWr476yhhknLgqO7Esaqhw%3D%3D");
            var content = new StringContent($"{{\"email\":\"{login}\",\"password\":\"{pass}\"}}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            string bearerToken = response.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");
            Console.WriteLine($"Bearer Token: {bearerToken}");

             client = new HttpClient();
             request = new HttpRequestMessage(HttpMethod.Get, "https://api.prod1.qolio.ru/api/v1/api_integrations/0d620a73-6190-49fc-93e6-4bc86d9a29cf/user_account_bindings_csv");
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            //request.Headers.Add("Cookie", "_interslice_session=Xe2IrqcXjnAt6kcD%2FqTkzRzuSDcWueOwqbyFcflyAE%2FGF1f9gwWYaGMIDo0HTKKnnU6BXp9BTYgGshqEUlPFUdkrg%2F6UhFp%2BnFC7KuFZp33zRztNwBzZMhMMye35Z%2BQkhnWlhTtUlIzEJZO6r0Cbgma1yk9w28NEYXPRIzsqR4cn9VRKCTU9bzRmptFoJwbKa2aP0ynutV60gGaozP3EChmJVLXt%2B4kCKCp4FBBC1S%2BKE94noxBW42Ril4NDsl%2BPcCntcAror1k4gTjJ6OdbqsWSVM38Q1dEdkviHOMMNpw9vUIFYEiiDJxmWLv%2BUHrNeIhdjAVZi0nekXS6ERxDZWenxdmktHTmBkMVSWyguqCbmb8mWFcG8qg6lA6ZGSY%3D--4OKlCXukNxbC%2B2eQ--gaTvmkSiTEJAkFXpl3Y2vA%3D%3D");
             response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var ResponseContent = await response.Content.ReadAsStringAsync();
            var res = ResponseContent.ToString().Split('\n').ToList();

            var result = res.Skip(1).Take(res.Count - 2);

            var lst = new List<Datum>();

            foreach (var item in result)
            {
                var splitData = item.Split(',').ToList();
                lst.Add(new Datum
                {
                    email = splitData[0],
                    last_name = splitData[1],
                    first_name = splitData[2],
                    integration_uid = splitData[3]
                });
            }
            var root = new StaffIntegrationDTO { data = lst };

            var serialisedRoot = Newtonsoft.Json.JsonConvert.SerializeObject(root);

            request = new HttpRequestMessage(HttpMethod.Put, "https://api.prod1.qolio.ru/api/v1/api_integrations/0d620a73-6190-49fc-93e6-4bc86d9a29cf/import_user_account_bindings");
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            //request.Headers.Add("Cookie", "_interslice_session=5lA85Hu054nxZDLvb6Q0OOylxv0tpvMWiYn8IpUI59GdRRb7guR10lFOjKjjTM4FLhCnJWYxu2UsBf7UpP%2Bpfk8L8903eN3yUfPRKx6eWLpazv1TjLisOrLkvtdq5GYhiW85F0y2vhXQBCfm9pBnCpGQeX2PpEBo%2Bs%2BvN2T61vmyla%2FCZmWEEDp4mUUS7pJqaESpYHcBnIdDfwjHRrHB3V7Si0PTqdY7GjZfnPuqXJ1sk%2FeCBXzMCROgiJSwy3m4gBIzAp9L1tYrptEd0LmfQPSvQXFplXdvyssl7EJ%2FH692H153%2BVvLLe0IvfFD6TEW7q3ma5hFvXahj%2B3RHxMKmsg9hasOdUZxExs5FblFWntOXQceLtdZ%2B%2F%2ByEGFmBoM%3D--2cQCH58etP0vf%2BTv--5WHWYNwmV6YzZVEvBvuF8w%3D%3D");
            content = new StringContent(serialisedRoot, null, "application/json");
            request.Content = content;
            response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            ResponseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine(await response.Content.ReadAsStringAsync());



            //var rootPath = _environment.WebRootPath;

            //var imagesDirectory = rootPath + "/images/";

            //if (!Directory.Exists(imagesDirectory))
            //    Directory.CreateDirectory(imagesDirectory);

            return Ok();
        }

        [HttpGet("GetValue")]
        public async Task<IActionResult> GetValues()
        {
            var login = "shahzod.akhmedov@bk.ru";
            var password = "@5I4Qk~qJTTR2s#";

            var url = "https://api.prod1.qolio.ru/api/v1/users";

            var requestBody = "{\"email\": \"test-user@test.org\",\"password\": \"some-password\",\"unit_id\": \"877408ca-1f18-48ee-b59c-2a72fbbf5729\",\"role_id\": \"78376e55-a6ca-4e74-94c3-9a6b562abc78\",\"last_name\": \"Test\",\"first_name\": \"User\",\"integration_uid\": \"402\"}";

            using (var client = new HttpClient())
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("POST request successful");
                }
                else
                {
                    Console.WriteLine($"POST request failed with status code {response.StatusCode}");
                }
            }
            return Ok();

        }
        private async Task<DateTime> ToDateTime(int timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp).ToLocalTime();
        }
    }
}




