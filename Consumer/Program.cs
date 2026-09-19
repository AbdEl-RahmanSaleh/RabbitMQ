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

            channel.QueueDeclare(queue: rabbitmqConfig.QueueName,
                                           durable: false,
                                           exclusive: false,
                                           autoDelete: false,
                                           arguments: null);


            // Basic quality of service Set the prefetch count to 1 to ensure that the consumer processes one message at a time
            // This prevents the consumer from being overwhelmed with multiple messages and allows for better control over message processing.
            // The prefetchSize parameter is set to 0, which means there is no limit on the size of the messages that can be sent to the consumer.
            // The global parameter is set to false, which means that the prefetch count applies only to the current channel and not to all channels on the connection.

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


            var consumer = new EventingBasicConsumer(channel);


            var random = new Random();  
            consumer.Received += (sender, args) =>
            {
                var processingTime = random.Next(1, 10); // Random processing time between 1 and 10 seconds
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message Received : {message} " +
                    $"will take {processingTime} seconds to process");

                Thread.Sleep(TimeSpan.FromSeconds(processingTime)); // Simulate processing time

                //deliveryTag : is a unique identifier for the message received from the queue.
                             // It is used to acknowledge the message after processing. 

                //multiple : is a boolean flag that indicates whether to acknowledge multiple messages at once.
                          // If set to true, all messages up to and including the specified deliveryTag will be acknowledged.
                          // If set to false, only the message with the specified deliveryTag will be acknowledged.
                channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);

            };


            channel.BasicConsume(queue:rabbitmqConfig.QueueName , autoAck : false,consumer:consumer);

            Console.ReadLine();

        }
    }
}
