using Quee.Extensions;
using Quee.WebApp.Quee.Commands;
using Quee.WebApp.Quee.Consumers;

namespace QueueUtility.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.QueeWithAzureServiceBus(builder.Configuration["ServiceBusConnectionString"]!, options =>
            {
                options.AddQueueProcessors<LogMessageCommand, LogMessageConsumer>("LogMessage-Queue");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
