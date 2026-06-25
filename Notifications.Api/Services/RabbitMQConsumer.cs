// Notifications.Api/Services/RabbitMQConsumer.cs
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Notifications.Api.Data;
using Notifications.Api.Models;
using Notifications.Api.Enums;
using Notifications.Api.Messages;

namespace Notifications.Api.Services
{
    // IHostedService runs in background automatically when app starts
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;
        private const string QueueName = "ticket-events";

        public RabbitMQConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<RabbitMQConsumer> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            // retry up to 5 times with 3 second delay
            var retries = 5;
            while (retries > 0)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _logger.LogInformation("Connected to RabbitMQ successfully");
                    break; // connection succeeded, exit retry loop
                }
                catch (Exception ex)
                {
                    retries--;
                    _logger.LogWarning(ex, "RabbitMQ not ready, retrying in 3 seconds... ({Retries} attempts left)", retries);
                    Thread.Sleep(3000); // wait 3 seconds before retrying

                    if (retries == 0)
                    {
                        _logger.LogError("Failed to connect to RabbitMQ after all retries");
                        return Task.CompletedTask; // give up gracefully
                    }
                }
            }

            _channel = _connection.CreateModel();

            //declare same queue, it must match publisher exactly
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    //deserialize the message
                    var message = JsonSerializer.Deserialize<TicketEventMessage>(json);

                    if (message != null)
                    {
                        // save notification to DB
                        using var scope = _scopeFactory.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                        var notification = new Notification
                        {
                            TicketId = message.TicketId,
                            Type = Enum.Parse<NotificationType>(message.Type),
                            Message = message.Message,
                            SentAt = message.SentAt
                        };

                        context.Notifications.Add(notification);
                        await context.SaveChangesAsync();

                        //acknowledge message, tells RabbitMQ it was processed
                        _channel.BasicAck(ea.DeliveryTag, false);

                        _logger.LogInformation(
                            "Notification saved for ticket {TicketId}", message.TicketId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message: {Json}", json);
                    // reject message — sends it back to queue
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}