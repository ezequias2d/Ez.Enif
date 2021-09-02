using Example.Properties;
using Ez.Enif;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = Resources.test;

            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, System.Linq.Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, System.Linq.Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            var loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Information });

            ILogger logger = loggerFactory.CreateLogger<Program>();

            var context = new Context();

            context.LogError += (sender, e) => 
            {
                logger.LogError(e.Message);
            };

            var rootSession = context.Read(text);
            Console.WriteLine("ROOT");
            PrintSession(rootSession);
        }

        private static void PrintSession(Session session, string init = "")
        {
            foreach(var value in session.Properties)
                Console.WriteLine($"{init}{value.Key}={value.Value}");

            foreach (var sub in session)
            {
                Console.WriteLine(init + sub.Key);
                PrintSession(sub.Value, init + "--");
            }
        }
    }
}
