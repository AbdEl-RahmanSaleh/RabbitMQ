namespace Shared.Configurations;

public class RabbitMQConfiguration
{

    public string Server { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string QueueName { get; set; }
    public string ExchangeName { get; set; }
}
