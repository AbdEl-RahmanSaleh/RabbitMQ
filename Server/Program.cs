using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using System.Text;

namespace Producer
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

            channel.ExchangeDeclare(exchange: rabbitmqConfig.ExchangeName, type: ExchangeType.Direct);
            
            var message = "Hello from Producer!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(rabbitmqConfig.ExchangeName, "", null,body);

            Console.WriteLine($"Send Message : {message}");

            Console.ReadLine();
        }
    }
}
