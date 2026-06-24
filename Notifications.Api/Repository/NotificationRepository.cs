
using Notifications.Api.Data;
using Notifications.Api.Interfaces;
using Notifications.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Api.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDBContext _context;
        public NotificationRepository(ApplicationDBContext context)//dependency injection
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<Notification> CreateAsync(Notification notificationModel){
            _context.Notifications.Add(notificationModel);
            await _context.SaveChangesAsync();
            return notificationModel;
        }
    }
}