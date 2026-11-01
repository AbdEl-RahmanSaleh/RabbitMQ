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

            channel.ExchangeDeclare(exchange:rabbitmqConfig.SecondExchangeName, type: ExchangeType.Fanout);
            channel.QueueDeclare(queue: rabbitmqConfig.QueueName, durable: true, exclusive: false, autoDelete: false); 
            channel.QueueBind(queue: rabbitmqConfig.QueueName, exchange: rabbitmqConfig.SecondExchangeName, routingKey: "");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Main - Received new Message : {message}");
            };

            channel.BasicConsume(queue: rabbitmqConfig.QueueName, consumer: consumer);

         
            Console.WriteLine("Consuming messages...");
            Console.ReadLine();
        }
    }
}
