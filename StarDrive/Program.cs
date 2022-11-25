
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using StarDrive;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Star Drive Service";
    })
    .ConfigureServices(services =>
    {
        LoggerProviderOptions.RegisterProviderOptions<
            EventLogSettings, EventLogLoggerProvider>(services);

        services.AddHostedService<StarDriveHost>();
    })
    .ConfigureLogging((context, logging) =>
    {
        // See: https://github.com/dotnet/runtime/issues/47303
        logging.AddConfiguration(
            context.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();



//Console.WriteLine("Hello, World!");

//string rootPath = @"C:\StarDrive";
//var files = Directory.EnumerateFiles(rootPath, "*.*");
//foreach (var file in files)
//{
//    Console.WriteLine(file);
//}


//var directories = Directory.EnumerateDirectories(rootPath);
//foreach (var d in directories)
//{
//    Console.WriteLine("");
//    Console.WriteLine(d);
//    var f = Directory.EnumerateFiles(d, "*.*");
//    foreach (var file in f)
//    {
//        Console.WriteLine("  " + file);
//    }
//}



