namespace Quee.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.MapGet("/", () => TypedResults.Ok("Running"));
        app.Run();
    }
}
