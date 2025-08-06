using SCU.Serilog.Sinks.Telegram;
using Serilog;
using Serilog.Extensions.Logging;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true);
builder.Logging.ClearProviders();

var comboLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:ComboLogger")
    .CreateLogger();
var logger1 = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:Logger1")
    .CreateLogger();
var logger2 = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:Logger2")
    .CreateLogger();
builder.Services.AddLogging(logger => logger.AddSerilog(comboLogger, true));
var log1Provider = new SerilogLoggerProvider(logger1, true);
var log2Provider = new SerilogLoggerProvider(logger2, true);
builder.Services.AddKeyedSingleton("Logger1", log1Provider.CreateLogger(null));
builder.Services.AddKeyedSingleton("Logger2", log2Provider.CreateLogger(null));

//You can change settings for sender via static fields in TelegramSender.Settings
//TelegramSender.Settings.DefaultWaitTimeAfterSendMs = 100;
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
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (log1Provider is IDisposable d1) d1.Dispose();
    if (log2Provider is IDisposable d2) d2.Dispose();
});
app.Run();
