using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiddleWare.Email;
using MiddleWare.RabbitMQClient;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MiddleWare.RabbitMQClient
{
    public class RabbitMQService : IRabbitMQService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _hostname = "localhost";  // Change if needed
        private readonly string _queueName = "GreetApp"; // Queue name
        private readonly SMTP _smtp;
        public RabbitMQService(SMTP _smtp)
        {

            this._smtp = _smtp;
        }

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

                _logger.Info("Message sent to queue: {0}", message);
            }
        }

        public void ReceiveMessage()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // ✅ Print the message to console
                Console.WriteLine("\n\n\n\nConsumed Message: " + message + "\n\n\n");

                _logger.Info("Received message from queue: {0}", message);
            };

            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }

    }
}