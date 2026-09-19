using Shared.Messages;

namespace Producer.Services
{
    public interface IRabbitMQService
    {
        public void publish(CompetingConsumersMessage message);
    }
}
