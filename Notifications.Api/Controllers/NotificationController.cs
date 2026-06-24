using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Interfaces;
using Notifications.Api.Mappers;
using Notifications.Api.Dtos;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id){
            var notification = await _notifictionRepo.GetByIdAsync(id);

            if (notification == null)
                return NotFound();

            return Ok(notification.ToNotificationDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create ([FromBody] CreateNotificationRequestDto notificationDto)
        {
            var notificationModel = notificationDto.ToNotificationFromCreateDto();
            await _notifictionRepo.CreateAsync(notificationModel);

            return CreatedAtAction(
                nameof(GetById), //execute getById method
                new { id = notificationModel.Id}, //pass this new object into the id of the getById method
                notificationModel.ToNotificationDto()  //then return into the form of ticketDto
            );
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetTicketNotifications ([FromRoute] Guid ticketId)
        {
            var notificationModel = await _notifictionRepo.GetTicketNotificationsAsync(ticketId);

            if(notificationModel == null)
                return NotFound();

            var result = notificationModel.Select(n => n.ToNotificationDto()).ToList();

            return Ok(result);
        }

        
    }
}