using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notifications.Api.Models;

namespace Notifications.Api.Interfaces
{
    public interface INotificationRepository
    {

        Task<List<Notification>> GetAllAsync();
        Task<Notification> CreateAsync(Notification notificationModel);
    }
}