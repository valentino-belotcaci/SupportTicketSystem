using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Notifications.Api.Controllers;
using Notifications.Api.Dtos;
using Notifications.Api.Interfaces;
using Notifications.Api.Models;

namespace Notifications.Api.Tests.Controllers
{
    public class NotificationControllerTests
    {
        //mock replaces the real repository (no DB needed)
        private readonly Mock<INotificationRepository> _mockRepo;
        private readonly NotificationController _controller;


        public NotificationControllerTests()
        {
            //create fake repository
            _mockRepo = new Mock<INotificationRepository>();

            //inject fake repo into controller
            _controller = new NotificationController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithAllNotifications()
        {
            // ARRANGE

            var fakeNotifications = new List<Notification>
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    TicketId = Guid.NewGuid(),
                    Message = "Ticket created",
                    Type = "Created"
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    TicketId = Guid.NewGuid(),
                    Message = "Status changed",
                    Type = "StatusChanged"
                }
            };

            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fakeNotifications);

            // ACT
            var result = await _controller.GetAll();

            // ASSERT
            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var notifications = okResult.Value
                .Should()
                .BeAssignableTo<List<NotificationDto>>()
                .Subject;

            notifications.Should().HaveCount(2);

            notifications[0].Message
                .Should()
                .Be("Ticket created");

            notifications[1].Message
                .Should()
                .Be("Status changed");
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenNotificationExists()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            var notification = new Notification
            {
                Id = id,
                Message = "Test notification",
                Type = "Created"
            };

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync(notification);

            // ACT
            var result = await _controller.GetById(id);

            // ASSERT
            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var dto = okResult.Value
                .Should()
                .BeOfType<NotificationDto>()
                .Subject;

            dto.Message
                .Should()
                .Be("Test notification");
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotificationDoesNotExist()
        {
            // ARRANGE
            var id = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.GetByIdAsync(id))
                .ReturnsAsync((Notification?)null);

            // ACT
            var result = await _controller.GetById(id);

            // ASSERT
            result.Should()
                .BeOfType<NotFoundResult>();
        }

    }
}