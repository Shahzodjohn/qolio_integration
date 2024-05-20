using System.ComponentModel.DataAnnotations;

namespace WebHook.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public int? RateMessageID { get; set; }
        public string Text { get; set; }
        public int TimeStamp { get; set; }
        public int ChatId { get; set; }
     
        public virtual Chat Chat { get; set; }
    }
}
