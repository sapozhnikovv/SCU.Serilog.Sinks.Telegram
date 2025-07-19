# SCU.Serilog.Sinks.Telegram
![Logo](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/blob/main/img/tg.sink.png)

Minimal, Effective, Safe and Fully Async Serilog Sink that allows sending messages to pre-defined list of users in Telegram chat. 
It uses HttpClient with lifetime of logger (singleton in fact), Channel as queue and StringBuilder pool to optimize memory using (no mem leaks) and performance. 

Also uses SCU.MemoryChunks extenstion (split strings by size into chunks, without allocation redundant intermediate arrays like in LINQ version)
https://github.com/sapozhnikovv/SCU.MemoryChunks (very small, 3 lines of code)

This Sink designed to be simple, fast, safe and stable. 

Works without issues in docker container's too.

The Logging messages in Telegram chat is a simple task. You don't need to have many dependencies, dozens of code files, a huge wiki on how to configure it. If you hate monstrous projects, this Serilog extension is your choice.  Sources of this project - 4 files. Low cognitive complexity.

If the functionality of this solution does not meet your needs, you can always make your own version of this extension.

# Nuget (only .net8.0 for now)
https://www.nuget.org/packages/Serilog.Sinks.SCU.Telegram
```shell
dotnet add package Serilog.Sinks.SCU.Telegram
```
or
```shell
NuGet\Install-Package Serilog.Sinks.SCU.Telegram
```

## Example of using
Note: Nuget not passing name SCU.Serilog.Sinks.Telegram, so in Nuget this extension has name Serilog.Sinks.SCU.Telegram, but in C# code name of lib is SCU.Serilog.Sinks.Telegram.
```c#
using SCU.Serilog.Sinks.Telegram;
```

### Basic

```c#
using Serilog;
using Serilog.Events;
using SCU.Serilog.Sinks.Telegram;

Log.Logger = new LoggerConfiguration()
            .WriteTo.TelegramSerilog(
                apiKey: "12345:AAAAAAAAAAAA",
                chatIds: ["12345", "12346"],
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .CreateLogger();
```
[example](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/tree/main/Examples/ConsoleApp)

### with Serilog.AspNetCore

```c#
using Serilog;
using Serilog.Events;
using SCU.Serilog.Sinks.Telegram;

builder.Services.AddSerilog(logger => logger.WriteTo.TelegramSerilog(
        apiKey: "12345:AAAAAAA",
        chatIds: ["12345", "12346"],
        restrictedToMinimumLevel: LogEventLevel.Warning));
```

### with Serilog.AspNetCore and Serilog.Settings.Configuration
Loading from appsettings.json

```c#
using Serilog;
using Serilog.Events;
using SCU.Serilog.Sinks.Telegram;

builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
```

appsettings.json
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "TelegramSerilog",
        "Args": {
          "apiKey": "12345:AAAAAAAAAAAA",
          "chatIds": [ "12345", "12346" ],
          "restrictedToMinimumLevel": "Warning",
          "batchTextLength": 1500, //recommended value
          "batchInterval": 5,
          "maxCapacity": 1000000,
          "excludedByContains": [
            "Failed to determine the https port for redirect"
          ]
        }
      }
    ]
  }
}

```

 - apiKey - telegram channel key
 - chatIds - array of 'string's with chat IDs
 - restrictedToMinimumLevel - log level
 - batchTextLength - value is 'int'. This is the number of characters in logged messages. Sink will trigger the sending if reached. If value <= 0 then 1500 will be used.
 - batchInterval - value is 'int'. It is time in seconds. Sink will trigger the sending if reached. If value <= 0 then 5s will be used.
 - maxCapacity - value is 'int'. It is the limit of messages in queue. If value <= 0 then int.MaxValue will be used.
 - excludedByContains - array of 'string's. If a logged message contains any of these values - message will not be logged.


you can skip and not use all parameters except the first 2.
apiKey and chatIds are required.

Fatal/Critical log messages will be logged immediately.
  
[example](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/tree/main/Examples/WebAPI)

.
  
### Combination of Loggers with different credentials and 2 separate Loggers in one app (not recommended, but can be implemented)
using SerilogLoggerProvider from Serilog.Extensions.Logging

```c#
using Serilog;
using Serilog.Extensions.Logging;

var comboLogger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:ComboLogger")
    .CreateLogger();//log messages to channel A and B
var logger1 = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:Logger1")
    .CreateLogger();//log messages to channel A
var logger2 = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, "Serilog:Logger2")
    .CreateLogger();//log messages to channel B
builder.Services.AddLogging(logger => logger.AddSerilog(comboLogger, true));
var log1Provider = new SerilogLoggerProvider(logger1, true);
var log2Provider = new SerilogLoggerProvider(logger2, true);
builder.Services.AddKeyedSingleton("Logger1", log1Provider.CreateLogger(null));
builder.Services.AddKeyedSingleton("Logger2", log2Provider.CreateLogger(null));

...

app.Lifetime.ApplicationStopping.Register(() =>
{
    if (log1Provider is IDisposable d1) d1.Dispose();
    if (log2Provider is IDisposable d2) d2.Dispose();
});

```

appsettings.json
```json
{
  "Serilog": {
    "ComboLogger": {
      "WriteTo": [
        { "Name": "Console" },
        {
          "Name": "TelegramSerilog",
          "Args": {
            "apiKey": "12345:AAACCC",
            "chatIds": [ "456", "567" ],
            "restrictedToMinimumLevel": "Warning",
            "batchTextLength": 1500,
            "batchInterval": 5,
            "maxCapacity": 1000000,
            "ExcludedByContains": []
          }
        },
        {
          "Name": "TelegramSerilog",
          "Args": {
            "apiKey": "12346:AAABBB",
            "chatIds": [ "123", "234" ],
            "restrictedToMinimumLevel": "Error",
            "batchTextLength": 1500,
            "batchInterval": 5,
            "maxCapacity": 1000000,
            "ExcludedByContains": []
          }
        }
      ]
    },
    "Logger1": {
      "WriteTo": [
        { "Name": "Console" },
        {
          "Name": "TelegramSerilog",
          "Args": {
            "apiKey": "12345:AAACCC",
            "chatIds": [ "456", "567" ],
            "restrictedToMinimumLevel": "Warning",
            "batchTextLength": 1500,
            "batchInterval": 5,
            "maxCapacity": 1000000,
            "ExcludedByContains": []
          }
        }
      ]
    },
    "Logger2": {
      "WriteTo": [
        { "Name": "Console" },
        {
          "Name": "TelegramSerilog",
          "Args": {
            "apiKey": "12346:AAABBB",
            "chatIds": [ "123", "234" ],
            "restrictedToMinimumLevel": "Error",
            "batchTextLength": 1500,
            "batchInterval": 5,
            "maxCapacity": 1000000,
            "ExcludedByContains": []
          }
        }
      ]
    }
  }
}

```


DI
```c#
ILogger<T> comboLogger, [FromKeyedServices("Logger1")] ILogger logger1, [FromKeyedServices("Logger2")] ILogger logger2
```
  
[example](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/tree/main/Examples/WebAPI_ManyLoggers)

.

### If you need to print error from TelegramSender - you can enable SelfLogging in Serilog
```c#
Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
```

### If you need to re-configure Telegram Sender - you can use static fields in TelegramSender.Settings and change them on the fly
```c#
// These values ​​are default values ​​and are suitable for most (small business) projects.
TelegramSender.Settings.DefaultWaitTimeAfterSendMs = 750;
TelegramSender.Settings.ChunkSize = 3000;
TelegramSender.Settings.RetryWaitTimeWhenTooManyRequestsSeconds = 40;
TelegramSender.Settings.RetryCountWhenTooManyRequests = 2;
```

#### How to find the Telegram chatId?
Send any message to your bot if bot is new and empty.
Use the Telegram API to get the last updates for your bot and you can find the chatId(s) there:
```
curl -X GET \
  https://api.telegram.org/bot<my-bot-api-key>/getUpdates \
  -H 'Cache-Control: no-cache'
```
or just open in browser
```
https://api.telegram.org/bot<my-bot-api-key>/getUpdates
```

#### About Disposing Sink:
This Sink implements IDisposable and IAsyncDisposable, but in real world Dispose will be called only when app is shooting down. 
If for some reason you control when loggers are disposed - this Sink implements the dispose methods. 
If for some reason Dispose is not called - it will not affect your application and environment. 
This Sink works with the network and may not use Dispose at all. This Sink is designed to work as singleton forever.
For 'Dispose' you can configure DisposeTimeout
```c#
TelegramSerilogSink.Settings.DisposeTimeout = TimeSpan.FromSeconds(3);
```

#### If you have question about HttpClient with lifetime of logger (singleton in fact):
https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
HttpClient should be singleton.

#### Why PeriodicTimer not used?
##### About logic:
PeriodicTimer logic is fixed interval, but Logic of this Sink is 'wait X seconds after the sending message (even if sending is very long running)'.
Logic of this Sink is not try to avoid 'time drift', but the opposite.
Logic of this Sink is to *use 'time drift'*. (*It is needed to prevent ToManyRequests response from tg chat.*)
##### About stability:
Task.Delay creates one-time timer (Infinite time for repeat) and 'Dispose'/collect/release it automatically. OS deactivate one-time timers automatically, it is not on the .net side. PeriodicTimer create long-lived timer, it should be disposed manually. Without Dispose (Disposal may not be caused), PeriodicTimer can leak system resources (Windows WaitableTimer/Linux timerfd) and block app shutdown for its full interval. Task.Delay only leaves orphaned Task objects that GC cleans, while unfinished PeriodicTimer calls actively prevent process termination. Though both delay shutdown without cancellation, PeriodicTimer risks hung timers and descriptor leaks on Linux. Even if timer in linux cannot do executions (it will be closed) when app process is closed, this still can cause some issues with containers where may be checking how and when app/container will terminate. Task.Delay lightweight approach avoids these OS dependencies. For reliability without strict disposal, Task.Delay is preferable.
##### About memory:
PeriodicTimer is memory efficient, but in current implementation of this Sink - Task.Delay used on seconds-based intervals, so memory allocation of DelayPromises is acceptable.
(Example: ~28Kb memory will be allocated and then collected in one hour for 5 seconds interval. 40 bytes / interval. It is small numbers of memory for GC.)

So, this construct consists of explicit and implicit singletons.

## Memory Profiling
Memory profiling was done via JetBrains dotMem and using the old-school method of marking objects in RAM.

## License
Free MIT license (https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/blob/main/LICENSE)

## Versions
*1.1.0* - stable. Contains static HttpClient. Without IDisposable and IAsyncDisposable. [Repo at this point](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/tree/2f77748dcd4da3cdb4d944b5e2f4faee89af85ed)

*1.2.2* - stable. Each instance of Sink contains own (not static) HttpClient. Support IDisposable and IAsyncDisposable. 

*Note:* In real world v1.1.0 can be used without issues and probably it will be more productive.

This Sink is designed to work as singleton forever.

# P.S:
This Serilog extension was designed for [Color Disco](https://color-disco.ru) platform and proven to work well. As the founder and developer of the platform, I decided to make this extension available on GitHub.
