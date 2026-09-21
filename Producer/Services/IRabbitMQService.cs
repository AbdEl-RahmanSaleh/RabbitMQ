namespace Producer.Services
{
    public interface IRabbitMQService
    {
        public void publish<T>(T message, string routingKey);
    }
}
