using Microsoft.EntityFrameworkCore;
using WebHook.Models;

namespace WebHook
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {   }

        public AppDbContext()
        {}

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Client> Clients { get; set; }

        //public DbSet<chat_assigned> chat_assigned { get; set; }
        //public DbSet<chat_finished> call_Events { get; set; }
        //public DbSet<chat_updated> chat_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
        //public DbSet<Tpic> client_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
        //public DbSet<client_updated> client_updated { get; set; }
    }
}
