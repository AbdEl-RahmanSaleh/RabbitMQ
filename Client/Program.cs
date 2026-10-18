using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using System.Text;

namespace Consumer
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

            channel.ExchangeDeclare(exchange:rabbitmqConfig.ExchangeName, type: ExchangeType.Direct);


            channel.ExchangeDeclare(exchange:rabbitmqConfig.DLExchangeName, type: ExchangeType.Fanout);


            var argument = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", rabbitmqConfig.DLExchangeName },
                {"x-message-ttl", 1000 }
            };

            channel.QueueDeclare(queue: rabbitmqConfig.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: argument); 
            channel.QueueBind(queue: rabbitmqConfig.QueueName, exchange: rabbitmqConfig.ExchangeName, routingKey: "");

            var mainConsumer = new EventingBasicConsumer(channel);

            mainConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Main - Received new Message : {message}");
            };

            //channel.BasicConsume(queue: rabbitmqConfig.QueueName, autoAck: true, consumer: mainConsumer);

            channel.QueueDeclare(queue: rabbitmqConfig.DLXQueueName);
            channel.QueueBind(queue: rabbitmqConfig.DLXQueueName, exchange: rabbitmqConfig.DLExchangeName, routingKey: "");

            var dlxConsumer = new EventingBasicConsumer(channel);

            dlxConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"DLX - Received new Message : {message}");
            };

            channel.BasicConsume(queue: rabbitmqConfig.DLXQueueName, autoAck: true, consumer: dlxConsumer);
            Console.WriteLine("Consuming messages...");
            Console.ReadLine();
        }
    }
}
