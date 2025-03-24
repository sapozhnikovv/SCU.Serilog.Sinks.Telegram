# SCU.Serilog.Sinks.Telegram
![Logo](https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/blob/main/img/tg.sink.png)

Minimal, Effective, Safe anf Fully Async Serilog Sink that allows sending messages to pre-defined list of users in Telegram chat. 
It uses HttpClient singleton, Channel as queue and StringBuilder pool to optimize memory using (no mem leaks) and performance. 

This Sink designed to be simple, fast, safe and stable. 

Works without issues in docker container's too.

Logging in Telegram chat is a simple task. You don't need to have many dependencies, dozens of code files, a huge wiki on how to configure it. If you hate monstrous projects, this Serilog extension is your choice.

If the functionality of this solution does not meet your needs, you can always make your own version of this extension.

# Nuget
will be soon...

## Example of using
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
          "batchTextLength": 1500,
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
 - batchTextLength - value is 'int'. This is the number of characters in logged messages. Sink will trigger the sending if reached
 - batchInterval - value is 'int'. It is time in seconds. Sink will trigger the sending if reached
 - maxCapacity - value is 'int'. It is the limit of messages in queue. Of value <= 0 then int.MaxValue will be used.
 - excludedByContains - array of 'string's. If a logged message contains any of these values - message will not be logged.


you can skip and not use all parameters except the first 2.
apiKey and chatIds are required.

Fatal/Critical log messages will be logged immediately.
  
### If you need to print error from TelegramSender - you can enable SelfLoggig in Serilog
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


## License
Free MIT license (https://github.com/sapozhnikovv/SCU.Serilog.Sinks.Telegram/blob/main/LICENSE)

# P.S:
This Serilog extension was designed for [Color Disco](https://color-disco.ru) platform and proven to work well. As the founder and developer of the platform, I decided to make this extension available on GitHub.
