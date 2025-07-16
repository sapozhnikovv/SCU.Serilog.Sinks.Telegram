using Serilog;
using Serilog.Events;
using SCU.Serilog.Sinks.Telegram;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true);
builder.Logging.ClearProviders();

builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
/*or*/
/*builder.Services.AddSerilog(logger => logger.WriteTo.TelegramSerilog(
        apiKey: "12345:AAAAAAA",
        chatIds: ["12345", "12346"],
        restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.Console());*/

//You can change settings for sender via static fields in TelegramSender.Settings
TelegramSender.Settings.DefaultWaitTimeAfterSendMs = 100;
TelegramSerilogSink.Settings.DisposeTimeout = TimeSpan.FromSeconds(5);
//If you need to print error from TelegramSender - you can enable SelfLoggig in Serilog
Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
