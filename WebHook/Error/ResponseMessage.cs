using System.Net;

namespace WebHook.Error
{
    public class ResponseMessage
    {
        public HttpStatusCode Code { get; set; }
        public string Message { get; set; }
    }
}
