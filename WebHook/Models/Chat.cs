namespace WebHook.Models
{
    public class Chat
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int ClientId { get; set; }
        public int AgentId { get; set; }

        public virtual Agent Specialist { get; set; }
        public virtual Client Client { get; set; }
    }
}
