using Tickets.Api.Messages;

namespace Tickets.Api.Interfaces
{
    public interface IRabbitMQPublisher
    {
        void Publish(TicketEventMessage message);
    }
}