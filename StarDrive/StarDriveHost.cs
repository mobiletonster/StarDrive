using Microsoft.AspNetCore.SignalR.Client;
using StarDrive.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarDrive;

internal class StarDriveHost : IHostedService
{
    private CancellationToken _cancellationToken;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StarDriveHost> _logger;
    private HubConnection _connection;
    private readonly string _serverUrl="https://localhost:5001/";
    public StarDriveHost(ILogger<StarDriveHost> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        var machineName = System.Environment.MachineName;
        _connection = new HubConnectionBuilder()
        .WithUrl($"{_serverUrl}drivehub?role=client&machineName={machineName}", options =>
        {
            options.AccessTokenProvider = () => Task.FromResult("SuperSecretKey");
        })
        .Build();

        _connection.Closed += _connection_Closed;
        _connection.Reconnected += _connection_Reconnected;
        _connection.Reconnecting += _connection_Reconnecting;
        _connection.On<string, List<DirectoryItem>>("ReadDir", ReadDirectory);
        _connection.On<string, byte[]>("ReadFile", ReadFile);
    }
    public async Task<List<DirectoryItem>> ReadDirectory(string path)
    {
        List<DirectoryItem> directoryItems = new List<DirectoryItem>();
        //string rootPath = @"C:\StarDrive";
        DirectoryInfo di = new DirectoryInfo(path);
        
        var files = di.GetFiles().Where(m=>!m.Attributes.HasFlag(FileAttributes.Hidden));
        foreach (var file in files)
        {
            directoryItems.Add(new DirectoryItem() { Name= file.Name, Path = file.FullName, IsDirectory=false });
        }

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
        //if (directoryItems.Count > 50)
        //{
        //    await _connection.InvokeAsync("DirectoryResult", directoryItems);
        //}
        return directoryItems.ToList();
        // await _connection.InvokeAsync("DirectoryResult", directoryItems);
    }
    public async Task<byte[]> ReadFile(string path)
    {
        //DirectoryInfo di = new DirectoryInfo(path);

        var fileStream = File.OpenRead(path);
        var fileBytes = await File.ReadAllBytesAsync(path);
        var bytes =new byte[fileStream.Length];
        while (fileStream.Position < fileStream.Length)
        {
            var fileByte = fileStream.ReadByte();
            bytes[fileStream.Position-1]= (byte)fileByte;
        }
        return fileBytes;
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
        throw new NotImplementedException();
    }

    private Task _connection_Reconnected(string? arg)
    {
        throw new NotImplementedException();
    }

    private async Task _connection_Closed(Exception? arg)
    {
        _logger.LogWarning($"Separated from StarDrive. - {DateTime.Now.ToString()}");
        _logger.LogError("Booooom", arg);
        await this.StartAsync(_cancellationToken);
    }
}
