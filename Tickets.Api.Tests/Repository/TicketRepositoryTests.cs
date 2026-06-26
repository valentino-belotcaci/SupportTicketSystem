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

        [Fact]
        public async Task UpdateStatusAsync_UpdatesStatusCorrectly()
        {
            // ARRANGE
            var context = GetDbContext();

            var ticket = new Ticket
            {
                Title = "Test",
                Status = TicketStatus.Open
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var request = new UpdateStatusDto
            {
                Status = TicketStatus.InProgress
            };

            // ACT
            var result = await repo.UpdateStatusAsync(ticket.Id, request);

            // ASSERT
            result.Should().NotBeNull();
            result!.Status.Should().Be(TicketStatus.InProgress);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnsNull_WhenNotFound()
        {
            // ARRANGE
            var context = GetDbContext();
            var repo = CreateRepo(context);

            // ACT
            var result = await repo.UpdateStatusAsync(
                Guid.NewGuid(),
                new UpdateStatusDto { Status = TicketStatus.Closed });

            // ASSERT
            result.Should().BeNull();
        }

        [Fact]
        public async Task AssignTicketAsync_UpdatesAssignee()
        {
            // ARRANGE
            var context = GetDbContext();

            var ticket = new Ticket
            {
                Title = "Test",
                AssignedTo = null
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var request = new AssignTicketDto
            {
                AssignedTo = "John"
            };

            // ACT
            var result = await repo.AssignTicketAsync(ticket.Id, request);

            // ASSERT
            result!.AssignedTo.Should().Be("John");
        }

        [Fact]
        public async Task AssignTicketAsync_ReturnsNull_WhenNotFound()
        {
            // ARRANGE
            var context = GetDbContext();
            var repo = CreateRepo(context);

            // ACT
            var result = await repo.AssignTicketAsync(
                Guid.NewGuid(),
                new AssignTicketDto { AssignedTo = "John" });

            // ASSERT
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsCorrectly()
        {
            // ARRANGE
            var context = GetDbContext();

            var ticket = new Ticket
            {
                Title = "Old",
                Description = "Old desc"
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            var request = new UpdateTicketDto
            {
                Title = "New",
                Description = "New desc",
                Priority = TicketPriority.High,
                Category = TicketCategory.Bug,
                AssignedTo = "Mike"
            };

            // ACT
            var result = await repo.UpdateAsync(ticket.Id, request);

            // ASSERT
            result!.Title.Should().Be("New");
            result.Description.Should().Be("New desc");
            result.Priority.Should().Be(TicketPriority.High);
            result.Category.Should().Be(TicketCategory.Bug);
            result.AssignedTo.Should().Be("Mike");
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            // ARRANGE
            var context = GetDbContext();
            var repo = CreateRepo(context);

            // ACT
            var result = await repo.UpdateAsync(
                Guid.NewGuid(),
                new UpdateTicketDto());

            // ASSERT
            result.Should().BeNull();
        }
    }
}