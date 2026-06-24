using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Dtos.Notification;
using Notifications.Api.Interfaces;
using Notifications.Api.Mappers;

namespace Notifications.Api.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notifictionRepo;
        public NotificationController(INotificationRepository notificationRepo) {
            _notifictionRepo = notificationRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(){
            var notificationModel = await _notifictionRepo.GetAllAsync();

            var result = notificationModel.Select(n => n.ToNotificationDto()).ToList();

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create ([FromBody] CreateNotificationRequestDto notificationDto)
        {
            var notificationModel = notificationDto.ToNotificationFromCreatedDto();
            await _notifictionRepo.CreateAsync(notificationModel);

            return CreatedAtAction(
                nameof(GetById), //execute getById method
                new { id = notificationModel.Id}, //pass this new object into the id of the getById method
                notificationModel.ToNotificationDto()  //then return into the form of ticketDto
            );
        }

        
    }
}