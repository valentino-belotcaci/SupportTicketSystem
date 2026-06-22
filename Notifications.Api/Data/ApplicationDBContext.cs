using Microsoft.EntityFrameworkCore;
using Notifications.Api.Models;

//giant class thats going to allow us to search our individual tables

namespace Notifications.Api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbConctextOptions)
        : base(dbConctextOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>();
        }

        public DbSet <Notification> Notifications { get; set; }//manipulate notification table

    }
}