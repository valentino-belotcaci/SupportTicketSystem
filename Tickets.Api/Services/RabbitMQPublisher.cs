using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Tickets.Api.Messages;
namespace Tickets.Api.Services
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "ticket-events";

        public RabbitMQPublisher()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //declare the queue(creates it if it doesn't exist)
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,    //queue survives RabbitMQ restart
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public void Publish(TicketEventMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; //message survives RabbitMQ restart

            _channel.BasicPublish(
                exchange: "",
                routingKey: QueueName,
                basicProperties: properties,
                body: body
            );
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}