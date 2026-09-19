using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.Services;
using Shared.Messages;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {

        private readonly IRabbitMQService _rabbitmqService;

        public SenderController(IRabbitMQService rabbitmqService)
        {
            _rabbitmqService = rabbitmqService;
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage([FromBody]CompetingConsumersMessage message)
        {
            try
            {
                _rabbitmqService.publish(message);
                return Ok("Message Sent Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                
            }


        }
    }
}
