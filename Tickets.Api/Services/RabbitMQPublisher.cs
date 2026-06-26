using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Tickets.Api.Interfaces;
using Tickets.Api.Messages;

namespace Tickets.Api.Services
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private const string QueueName = "ticket-events";

        public RabbitMQPublisher(IConfiguration configuration)
        {

            _configuration = configuration;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            var retries = 5;
            while (retries > 0)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    break;
                }
                catch (Exception)
                {
                    retries--;
                    Thread.Sleep(3000);
                    if (retries == 0) throw;
                }
            }
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