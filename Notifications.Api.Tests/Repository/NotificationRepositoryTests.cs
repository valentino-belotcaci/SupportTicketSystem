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
    }
}