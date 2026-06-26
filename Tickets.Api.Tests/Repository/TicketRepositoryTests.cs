using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tickets.Api.Data;
using Tickets.Api.Enums;
using Tickets.Api.Interfaces;
using Tickets.Api.Models;
using Tickets.Api.Repository;
using Tickets.Api.Dtos.Ticket;

namespace Tickets.Api.Tests.Repository
{
    public class TicketRepositoryTests
    {
        private ApplicationDBContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }


        private TicketRepository CreateRepo(ApplicationDBContext context)
        {
            var publisher = new Mock<IRabbitMQPublisher>();
            var logger = new Mock<ILogger<TicketRepository>>();

            return new TicketRepository(context, publisher.Object, logger.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTickets()
        {
            // ARRANGE
            var context = GetDbContext();

            context.Tickets.AddRange(
                new Ticket { Title = "T1" },
                new Ticket { Title = "T2" }
            );

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            // ACT
            var result = await repo.GetAllAsync();

            // ASSERT
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTicket_WhenExists()
        {
            // ARRANGE
            var context = GetDbContext();

            var ticket = new Ticket { Title = "Test" };
            context.Tickets.Add(ticket);

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            // ACT
            var result = await repo.GetByIdAsync(ticket.Id);

            // ASSERT
            result.Should().NotBeNull();
            result!.Title.Should().Be("Test");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            // ARRANGE
            var context = GetDbContext();
            var repo = CreateRepo(context);

            // ACT
            var result = await repo.GetByIdAsync(Guid.NewGuid());

            // ASSERT
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_SavesAndReturnsTicket()
        {
            // ARRANGE
            var context = GetDbContext();
            var repo = CreateRepo(context);

            var ticket = new Ticket
            {
                Title = "New Ticket",
                CreatedBy = "Tester"
            };

            // ACT
            var result = await repo.CreateAsync(ticket);

            // ASSERT
            result.Id.Should().NotBeEmpty();
            context.Tickets.Should().HaveCount(1);
        }
    }
}