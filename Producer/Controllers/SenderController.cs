using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.Services;
using Shared.Configurations;
using Shared.Messages;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {

        //private readonly IRabbitMQService _rabbitmqService;

        private readonly IBus _bus;
        private readonly IConfiguration _configuration;

        public SenderController(/*IRabbitMQService rabbitmqService*/IBus bus, IConfiguration configuration)
        {
            //_rabbitmqService = rabbitmqService;
            _bus = bus;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage([FromBody]FirstDemoMessage message)
        {
            try
            {
                //_rabbitmqService.publish(message);
                var rabbitMQConfig = _configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();
                var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{rabbitMQConfig.QueueName}"));
                await endpoint.Send(message);
                return Ok("Message Sent Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                
            }


        }
    }
}
