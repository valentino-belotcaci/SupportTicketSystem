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

        [Fact]
        public async Task Create_ReturnsCreated_WhenTicketIsValid()
        {
            // ARRANGE
            var request = new CreateTicketRequestDto
            {
                Title = "New ticket"
            };

            _mockRepo
                .Setup(repo => repo.CreateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket ticket) => ticket);

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            var created = result.Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            created.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenTitleIsMissing()
        {
            // ARRANGE
            var request = new CreateTicketRequestDto
            {
                Title = ""
            };

            // simulate validation failure
            _controller.ModelState.AddModelError(
                "Title",
                "Title is required"
            );

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            result.Should()
                .BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenStatusTransitionIsValid()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            var existingTicket = new Ticket
            {
                Id = id,
                Status = TicketStatus.Open
            };

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(existingTicket);

            _mockRepo
                .Setup(repo => repo.UpdateStatusAsync(
                    id,
                    It.IsAny<UpdateStatusDto>()))
                .ReturnsAsync(new Ticket
                {
                    Id = id,
                    Status = TicketStatus.InProgress
                });

            // ACT
            var result = await _controller.UpdateStatus(
                id,
                new UpdateStatusDto
                {
                    Status = TicketStatus.InProgress
                });

            // ASSERT
            result.Should()
                .BeOfType<OkObjectResult>();
        }



        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenTicketMissing()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync((Ticket?)null);

            // ACT
            var result = await _controller.UpdateStatus(
                id,
                new UpdateStatusDto
                {
                    Status = TicketStatus.Open
                });

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenStatusMovesBackwards()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(new Ticket
                {
                    Id = id,
                    Status = TicketStatus.Closed
                });

            // ACT
            var result = await _controller.UpdateStatus(
                id,
                new UpdateStatusDto
                {
                    Status = TicketStatus.Open
                });

            // ASSERT
            result.Should()
                .BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AssignTicket_ReturnsOk_WhenSuccessful()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.AssignTicketAsync(
                    id,
                    It.IsAny<AssignTicketDto>()))
                .ReturnsAsync(new Ticket
                {
                    Id = id
                });

            // ACT
            var result = await _controller.AssignTicket(
                id,
                new AssignTicketDto());

            // ASSERT
            result.Should()
                .BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssignTicket_ReturnsNotFound_WhenMissing()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.AssignTicketAsync(
                    id,
                    It.IsAny<AssignTicketDto>()))
                .ReturnsAsync((Ticket?)null);

            // ACT
            var result = await _controller.AssignTicket(
                id,
                new AssignTicketDto());

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenTicketExists()
        {
            // ARRANGE

            var id = Guid.NewGuid();

            var request = new UpdateTicketDto
            {
                Title = "Updated title",
                Description = "Updated description"
            };

            _mockRepo
                .Setup(repo => repo.UpdateAsync(
                    id,
                    request))
                .ReturnsAsync(new Ticket
                {
                    Id = id,
                    Title = "Updated title"
                });


            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT

            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var ticket = okResult.Value
                .Should()
                .BeOfType<TicketDto>()
                .Subject;

            ticket.Title.Should().Be("Updated title");
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            var request = new UpdateTicketDto
            {
                Title = "Updated"
            };

            _mockRepo
                .Setup(repo => repo.UpdateAsync(
                    id,
                    request))
                .ReturnsAsync((Ticket?)null);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenTicketExists()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.DeleteAsync(id))
                .ReturnsAsync(new Ticket
                {
                    Id = id
                });

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            result.Should()
                .BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.DeleteAsync(id))
                .ReturnsAsync((Ticket?)null);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }


    }
}