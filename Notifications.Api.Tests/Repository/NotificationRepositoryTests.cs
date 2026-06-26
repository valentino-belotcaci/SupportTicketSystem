using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notifications.Api.Data;
using Notifications.Api.Models;
using Notifications.Api.Repository;

namespace Notifications.Api.Tests.Repository
{
    public class NotificationRepositoryTests
    {
        private ApplicationDBContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        private NotificationRepository CreateRepo(ApplicationDBContext context)
        {
            return new NotificationRepository(context);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllNotifications()
        {
            // ARRANGE
            var context = GetDbContext();

            context.Notifications.AddRange(
                new Notification { Message = "N1" },
                new Notification { Message = "N2" }
            );

            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            // ACT
            var result = await repo.GetAllAsync();

            // ASSERT
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectNotification()
        {
            // ARRANGE
            var context = GetDbContext();

            var notification = new Notification
            {
                Message = "Test notification"
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);

            // ACT
            var result = await repo.GetByIdAsync(notification.Id);

            // ASSERT
            result.Should().NotBeNull();
            result!.Message.Should().Be("Test notification");
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
    }
}