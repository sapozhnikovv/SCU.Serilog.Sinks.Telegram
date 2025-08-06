using Serilog;
using Serilog.Events;
using SCU.Serilog.Sinks.Telegram;

var interval = 5;
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.TelegramSerilog(
                apiKey: "12345:AAAAAAAAAAAA",
                chatIds: ["12345", "12346"],
                batchInterval: interval,
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .CreateLogger();
try
{
    //You can change settings for sender via static fields in TelegramSender.Settings

    //TelegramSender.Settings.DefaultWaitTimeAfterSendMs = 250;
    //TelegramSerilogSink.Settings.DisposeTimeout = TimeSpan.FromSeconds(5);

    //If you need to print error from TelegramSender - you can enable SelfLoggig in Serilog
    Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

    Log.Information("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Debug("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    await Task.Delay(TimeSpan.FromSeconds(interval + 1));
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Log.Warning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    try
    {
        string x = null;
        x = x.ToLower();
    }
    catch (Exception e)
    {
        Log.Error(e, "Test logger");
    }
    string y = null;
    y = y.ToLower();
}
catch (Exception e)
{
    Log.Fatal(e, "Something went wrong (log critical=fatal immediately)");
}
finally
{
    await Log.CloseAndFlushAsync();//disposing logger will wait 1.5s by default for Send logs + 1.5s for termination of logger.
}
Console.ReadKey();