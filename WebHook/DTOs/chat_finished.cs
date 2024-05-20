using WebHook.Models;

namespace WebHook.DTOs
{
    public class Agents
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class Chat
    {
        public string messages { get; set; }
    }

    public class Page
    {
        public string url { get; set; }
    }

    public class chat_finished
    {
        public string event_name { get; set; }
        public string widget_id { get; set; }
        public Visitor visitor { get; set; }
        public string assigned_agent { get; set; }
        public string chat_id { get; set; }
        public Topic topic { get; set; }
        public Page page { get; set; }
        public Agents agents { get; set; }
        public Chat chat { get; set; }
        public string rate { get; set; }
        public string comments { get; set; }
    }

    public class Topic
    {
        public string topic_id { get; set; }
        public string title { get; set; }
    }

    public class Visitor
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string number { get; set; }
    }



}
