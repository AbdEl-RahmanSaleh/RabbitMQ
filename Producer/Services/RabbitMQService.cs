using RabbitMQ.Client;
using Shared.Configurations;
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
        public void publish<T>(T message)
        {
            using var connection =  _factory.CreateConnection();
            using var channel =  connection.CreateModel();

            

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));


            channel.ExchangeDeclare(exchange : _configuration.ExchangeName, type: ExchangeType.Fanout);

             
            // Exchange name is empty string it means it's a default exchange (deprecated from RabbitMQ 4 and up )
            // the routing key in the default exchanege between the exchange and the queue is the queue name 

            channel.BasicPublish(exchange:_configuration.ExchangeName,
                    routingKey:"",
                    basicProperties:null,
                    body:body);

        }
    }
}
