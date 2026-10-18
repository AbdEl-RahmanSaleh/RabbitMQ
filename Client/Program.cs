using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            var rabbitmqConfig = configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();

            var factory = new ConnectionFactory()
            {
                HostName = rabbitmqConfig.Server,
                UserName = rabbitmqConfig.Username,
                Password = rabbitmqConfig.Password,
            };


            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var replyQueue = channel.QueueDeclare(queue: "", exclusive: true);
            channel.QueueDeclare(queue: rabbitmqConfig.QueueName, exclusive: false);



            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Client Reply Received :  {message}");

            };
            channel.BasicConsume(queue: replyQueue.QueueName, autoAck: true, consumer: consumer);
            
            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = replyQueue.QueueName;
            properties.CorrelationId = Guid.NewGuid().ToString();

            var message = "Hello from Client!";
            var body = Encoding.UTF8.GetBytes(message);

            Console.WriteLine($"Client Sending Request: {message}\n{properties.CorrelationId}");

            channel.BasicPublish("", rabbitmqConfig.QueueName, properties, body);
            Console.ReadLine();
        }
    }
}
