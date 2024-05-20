namespace WebHook.DTOs
{
    public class MessageObj
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class MessageDetails
    {
        public string AuthorName { get; set; }
        public string ClientName { get; set; }

        public List<MessageObj> Messages { get; set; }
    }
}
