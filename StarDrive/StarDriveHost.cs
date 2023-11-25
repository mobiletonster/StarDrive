using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.SignalR.Client;
using StarDrive.Shared;
using System.Data;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading.Channels;

namespace StarDrive;

internal class StarDriveHost : IHostedService
{
    private CancellationToken _cancellationToken;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StarDriveHost> _logger;
    private HubConnection _connection;
#if DEBUG
   private readonly string _serverUrl="https://localhost:5001/";
#else
   private readonly string _serverUrl = "https://stardrive.azurewebsites.net/";
#endif

    public StarDriveHost(ILogger<StarDriveHost> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        var machineName = System.Environment.MachineName;
        _connection = new HubConnectionBuilder()
        .WithUrl($"{_serverUrl}drivehub?role=client&machineName={machineName}", options =>
        {
            options.AccessTokenProvider = () => Task.FromResult("SuperSecretKey");
        }).WithAutomaticReconnect(new AlwaysRetryPolicy())
        .Build();

        _connection.Closed += _connection_Closed;
        _connection.Reconnected += _connection_Reconnected;
        _connection.Reconnecting += _connection_Reconnecting;
        _connection.On<string, List<DirectoryItem>>("ReadDir", ReadDirectory);
        _connection.On<string, byte[]>("ReadFile", ReadFile);
        _connection.On<string, int>("ReadFileStream", ReadFileStream);
        _connection.On<string, int>("ReadFileChannel", ReadFileToChannel);
    }
    public async Task<List<DirectoryItem>> ReadDirectory(string path)
    {
        List<DirectoryItem> directoryItems = new List<DirectoryItem>();
        //string rootPath = @"C:\StarDrive";
        DirectoryInfo di = new DirectoryInfo(path);
        
        var directories = di.GetDirectories().Where(m=>!m.Attributes.HasFlag(FileAttributes.Hidden));
        foreach (var d in directories)
        {
            try
            {
                directoryItems.Add(new DirectoryItem() { Name=d.Name, Path = d.FullName, IsDirectory = true });
            }
            catch (Exception ex)
            {
                // swallow the errors
                Console.WriteLine($"Something broke {ex.Message}");
            }
        }
        var files = di.GetFiles().Where(m => !m.Attributes.HasFlag(FileAttributes.Hidden));
        foreach (var file in files)
        {
            directoryItems.Add(new DirectoryItem() { Name = file.Name, Path = file.FullName, IsDirectory = false, Size = file.Length, Modified = file.LastWriteTime });
        }
        await Task.CompletedTask;
        return directoryItems.ToList();
        // await _connection.InvokeAsync("DirectoryResult", directoryItems);
    }
    public async Task<byte[]> ReadFile(string path)
    {
        //DirectoryInfo di = new DirectoryInfo(path);
        var fileinfo = ImageMetadataReader.ReadMetadata(path);
        var exifinfo = fileinfo.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var dateTime = exifinfo?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
        // var fileStream = File.OpenRead(path);
        var fileBytes = await File.ReadAllBytesAsync(path);
        //var bytes =new byte[fileStream.Length];
        //while (fileStream.Position < fileStream.Length)
        //{
        //    var fileByte = fileStream.ReadByte();
        //    bytes[fileStream.Position-1]= (byte)fileByte;
        //}
        return fileBytes;
    }

    public async Task ReadFileStream(string path, int bytesize)
    {
        //var fileStream = File.OpenRead(path);

        //while (fileStream.Position < fileStream.Length)
        //{
        //    var fileByte = fileStream.ReadByte();
        //    yield return (byte)fileByte;
        //    //bytes[fileStream.Position - 1] = (byte)fileByte;
        //}
        //await Task.CompletedTask;
        await _connection.SendAsync("UploadStream", ReadFileToStream(path,bytesize),path, bytesize ) ;
    }

    private async IAsyncEnumerable<byte[]> ReadFileToStream(string path, int bytesize = 1048)
    {
        var filename = Path.GetFileName(path);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int chunkcount = 0;
        int totalbytes = 0;
        var chunk = new byte[bytesize];
        int chunkPos = 0;
        var fileStream = File.OpenRead(path);
        while (fileStream.Position < fileStream.Length)
        {
            var fileByte = fileStream.ReadByte();
            totalbytes++;
            chunk[chunkPos] = (byte)fileByte;
            if(chunkPos >= (bytesize-1))
            {
                yield return chunk;
                chunkPos = 0;
                chunk = new byte[bytesize];
                chunkcount++;
            }
            else
            {
                chunkPos++;
            }

        }
        stopwatch.Stop();
        Console.WriteLine($"stream,{chunkcount},{stopwatch.ElapsedMilliseconds},{bytesize},{totalbytes},{filename}");
        await Task.CompletedTask;
    }

    internal async Task ReadFileToChannel(string path, int bytesize=1048)
    {
        var filename = Path.GetFileName(path);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int chunkcount = 0;
        int totalbytes = 0;
        var chunk = new byte[bytesize];
        int chunkPos = 0;
        var channel = Channel.CreateBounded<byte[]>(10);
        await _connection.SendAsync("UploadChannel", channel.Reader, path, bytesize);

        var fileStream = File.OpenRead(path);
        while (fileStream.Position < fileStream.Length)
        {
            var fileByte = (byte)fileStream.ReadByte();
            totalbytes++;
            chunk[chunkPos] = fileByte;
            if (chunkPos >= (bytesize-1))
            {
                await channel.Writer.WriteAsync(chunk);
                chunkPos = 0;
                chunk = new byte[bytesize];
                chunkcount++;
            }
            else
            {
                chunkPos++;
            }
        }
        channel.Writer.Complete();
        stopwatch.Stop();
        Console.WriteLine($"channel,{chunkcount},{stopwatch.ElapsedMilliseconds},{bytesize},{totalbytes},{filename}");
    }

   

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        // Loop is here to wait until the server is running
        while (true)
        {
            try
            {
                await _connection.StartAsync(cancellationToken);
                var _connectionId = _connection.ConnectionId;
                _logger.LogInformation($"Connected to StarDrive at {_serverUrl} - {DateTime.Now.ToString()} with cid: {_connectionId}  ");
                // await EnsureScanFoldersExist();
                break;
            }
            catch (Exception ex)
            {
                await Task.Delay(1000, cancellationToken);
                _logger.LogError( $"Unable to find StarDrive at {_serverUrl} - {DateTime.Now.ToString()}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private Task _connection_Reconnecting(Exception? arg)
    {
        _logger.LogError($"Attempting to reconnect to StarDrive at {_serverUrl} - {DateTime.Now.ToString()}");
        return Task.CompletedTask;
    }

    private Task _connection_Reconnected(string? arg)
    {
        //throw new NotImplementedException();
        _logger.LogError($"Reconnected to StarDrive at {_serverUrl} - {DateTime.Now.ToString()} {arg}");
        return Task.CompletedTask;
    }

    private async Task _connection_Closed(Exception? arg)
    {
        _logger.LogWarning($"Separated from StarDrive. - {DateTime.Now.ToString()}");
        _logger.LogError("Booooom", arg);
        await this.StartAsync(_cancellationToken);
    }

}


