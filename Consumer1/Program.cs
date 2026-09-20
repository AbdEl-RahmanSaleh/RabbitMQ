using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using System.Text;

namespace Consumer1
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


            channel.ExchangeDeclare(exchange: rabbitmqConfig.ExchangeName, type: ExchangeType.Fanout);

            //Declare a Temp queue and bind it to the exchange 
            //Temp Queue will be deleted when the consumer disconnects

            var queuename = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queuename, exchange: rabbitmqConfig.ExchangeName, routingKey: "");
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"First Consumer :  Message Received : {message}");

            };
            channel.BasicConsume(queue: queuename, autoAck: true, consumer: consumer);

            Console.ReadLine();
        }
    }
}
