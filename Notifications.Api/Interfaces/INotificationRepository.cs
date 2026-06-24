using Notifications.Api.Models;

namespace Notifications.Api.Interfaces
{
    public interface INotificationRepository
    {

        Task<List<Notification>> GetAllAsync();
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notificationModel);
    }
}