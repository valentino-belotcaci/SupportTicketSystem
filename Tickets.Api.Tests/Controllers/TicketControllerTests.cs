using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tickets.Api.Controllers;
using Tickets.Api.Dtos.Ticket;
using Tickets.Api.Enums;
using Tickets.Api.Interfaces;
using Tickets.Api.Models;
using Tickets.Api.Queries;

namespace Tickets.Api.Tests.Controllers
{
    public class TicketControllerTests
    {
        //mock replaces the real repository (no DB needed)
        private readonly Mock<ITicketRepository> _mockRepo;
        private readonly TicketController _controller;

        public TicketControllerTests()
        {
            //create a fake repo
            _mockRepo = new Mock<ITicketRepository>();

            //inject the fake repo into the controller
            //exactly like ASP.NET Core does via dependency injection
            _controller = new TicketController(_mockRepo.Object);
        }

        [Fact] // [Fact] = this is a test method
        public async Task GetAll_ReturnsOkWithAllTickets_WhenNoFiltersApplied()
        {
            //ARRANGE :set up fake data and tell mock what to return
            var fakeTickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "First ticket",
                    Description = "First description",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.High,
                    Category = TicketCategory.Bug,
                    CreatedBy = "Valentino"
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "Second ticket",
                    Description = "Second description",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.Low,
                    Category = TicketCategory.Support,
                    CreatedBy = "Marco"
                }
            };

            //tell to the mock: when GetAllAsync() is called, return fakeTickets
            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fakeTickets);

            //ACT — call the actual controller method
            var result = await _controller.GetAll(new TicketQuery());

            //ASSERT — verify the response is what we expect
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var tickets = okResult.Value.Should().BeAssignableTo<List<TicketDto>>().Subject;
            tickets.Should().HaveCount(2);
            tickets[0].Title.Should().Be("First ticket");
            tickets[1].Title.Should().Be("Second ticket");
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredTickets_WhenStatusFilterApplied()
        {
            // ARRANGE
            var fakeTickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "Open ticket",
                    Description = "Open description",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.High,
                    Category = TicketCategory.Bug,
                    CreatedBy = "Valentino"
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "InProgress ticket",
                    Description = "InProgress description",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.Low,
                    Category = TicketCategory.Support,
                    CreatedBy = "Marco"
                }
            };

            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fakeTickets);

            // filter by Open status
            var query = new TicketQuery { Status = "Open" };

            // ACT
            var result = await _controller.GetAll(query);

            // ASSERT — only the Open ticket should come back
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var tickets = okResult.Value.Should().BeAssignableTo<List<TicketDto>>().Subject;
            tickets.Should().HaveCount(1);
            tickets[0].Title.Should().Be("Open ticket");
            tickets[0].Status.Should().Be(TicketStatus.Open);
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredTickets_WhenPriorityFilterApplied()
        {
            // ARRANGE
            var fakeTickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "High priority",
                    Priority = TicketPriority.High
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "Low priority",
                    Priority = TicketPriority.Low
                }
            };

            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fakeTickets);


            var query = new TicketQuery { Priority = "High" };


            // ACT
            var result = await _controller.GetAll(query);


            // ASSERT
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            var tickets = okResult.Value
                .Should()
                .BeAssignableTo<List<TicketDto>>()
                .Subject;

            tickets.Should().HaveCount(1);
            tickets[0].Title.Should().Be("High priority");
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredTickets_WhenCategoryFilterApplied()
        {
            // ARRANGE
            var fakeTickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "Bug ticket",
                    Category = TicketCategory.Bug
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = "Support ticket",
                    Category = TicketCategory.Support
                }
            };

            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fakeTickets);


            var query = new TicketQuery { Category = "Bug" };


            // ACT
            var result = await _controller.GetAll(query);


            // ASSERT
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            var tickets = okResult.Value
                .Should()
                .BeAssignableTo<List<TicketDto>>()
                .Subject;

            tickets.Should().HaveCount(1);
            tickets[0].Category.Should().Be(TicketCategory.Bug);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenTicketExists()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = id,
                Title = "Test ticket"
            };


            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(ticket);


            // ACT
            var result = await _controller.GetById(id);

            // ASSERT
            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var dto = okResult.Value
                .Should()
                .BeOfType<TicketDto>()
                .Subject;

            dto.Title.Should().Be("Test ticket");
        }



        [Fact]
        public async Task GetById_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync((Ticket?)null);

            // ACT
            var result = await _controller.GetById(id);

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }

    }
}