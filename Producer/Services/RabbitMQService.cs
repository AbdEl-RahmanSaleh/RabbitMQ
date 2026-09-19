using RabbitMQ.Client;
using Shared.Configurations;
using Shared.Messages;
using System.Text;
using System.Text.Json;

namespace Producer.Services
{
    public class RabbitMQService : IRabbitMQService
    {

        private readonly RabbitMQConfiguration _configuration;
        private readonly ConnectionFactory _factory;

        public RabbitMQService(RabbitMQConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory()
            {
                HostName = _configuration.Server,
                UserName = _configuration.Username,
                Password = _configuration.Password,
            };
        }
        public void publish(CompetingConsumersMessage message)
        {
            using var connection =  _factory.CreateConnection();
            using var channel =  connection.CreateModel();

             channel.QueueDeclare(queue: _configuration.QueueName,
                                            // durable if true means : the queue will be saved on the hard disk so if the server restart the messages will not deleted
                                            durable: false,
                                            // exclusive if true : means this queue is exclusive for this connection so if the connection closes the queue will be deleted
                                            exclusive: false,
                                            // autoDelete if true : means when the last consumer unsbscribe delete the queue
                                            autoDelete:false,
                                            arguments:null);


            for (int i = 0; i < 5; i++)
            {

                message.Id = i;
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                // Exchange name is empty string it means it's a default exchange (deprecated from RabbitMQ 4 and up )
                // the routing key in the default exchanege between the exchange and the queue is the queue name 

                 channel.BasicPublish(exchange:"",
                        routingKey:_configuration.QueueName,
                        basicProperties:null,
                        body:body,
                        mandatory:true);

            }


            
            


        }
    }
}
