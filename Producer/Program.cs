using MassTransit;
using Producer.Services;
using Shared.Configurations;

namespace Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var rabbitmqConfig = builder.Configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();

            builder.Services.AddMassTransit(options =>
            {
                options.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitmqConfig.Server, h =>
                    {
                        h.Username(rabbitmqConfig.Username);
                        h.Password(rabbitmqConfig.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                    cfg.Exclusive = false;
                    cfg.Durable = true;
                });
            });

            //builder.Services.AddSingleton(rabbitmqConfig);
            //builder.Services.AddScoped<IRabbitMQService,RabbitMQService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
