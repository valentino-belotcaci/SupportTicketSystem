using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Data;

namespace Notifications.Api.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public NotificationController(ApplicationDBContext context) {

            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll(){
            var notifications = _context.Notifications.ToList();

            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] Guid id){
            var notification = _context.Notifications.Find(id);

            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        
    }
}