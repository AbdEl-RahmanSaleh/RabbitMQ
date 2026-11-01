using MassTransit;
using Shared.Messages;

namespace Consumer
{
    internal class DemoConsumer : IConsumer<FirstDemoMessage>
    {
        public async Task Consume(ConsumeContext<FirstDemoMessage> context)
        {
            var message = context.Message;
            Console.WriteLine($"From: {message.From}\nTo: {message.To}\nSubject:{message.Title}" +
                $"\n{message.Content}");
        }
    }
}