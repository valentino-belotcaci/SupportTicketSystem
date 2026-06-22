using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tickets.Api.Models;

//giant class thats going to allow us to search our individual tables

namespace Tickets.Api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbConctextOptions)
        : base(dbConctextOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<Ticket>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Category)
                .HasConversion<string>();
        }

        public DbSet <Ticket> Tickets { get; set; }//manipulate ticket table
    }
}