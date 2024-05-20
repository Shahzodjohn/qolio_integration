namespace WebHook.DTOs
{
    public class Root
    {
        public string client_id { get; set; }
        public string communication_type { get; set; }
        public string title { get; set; }
        public string source { get; set; }
        public int nps { get; set; }
        public int client_feedback { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
        public DateTime started_at { get; set; }
        public string? email { get; set; } = "Не обнаружен";
        public List<CommunicationPart> communication_parts { get; set; }
        public string operator_id { get; set; }
       
        public CustomFields custom_fields { get; set; }
    }

    public class CustomFields
    {
        public string client_name { get; set; }
        public string phone_number { get; set; }
        public string chat_id { get; set; }
        public string client_rate { get; set; }
        public string url { get; set; }
    }

    public class Author
    {
        public string type { get; set; }
        public string? id { get; set; }
    }

    public class CommunicationPart
    {
        public string communication_part_id { get; set; }
        public Author author { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public string content_type { get; set; }
    }
}
