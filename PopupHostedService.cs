



using H.NotifyIcon.Core;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

using System.Diagnostics;

internal class PopupHostedService : IHostedService
{
    private readonly TrayIconWithContextMenu _menu;
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _host;
    private Process? _process;
    public PopupHostedService(TrayIconWithContextMenu menu, IServer server, IHostApplicationLifetime host)
    {
        _menu = menu;
        _server = server;
        _host = host;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _host.ApplicationStarted.Register(OnApplicationStarted);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _menu.ShowNotification("MinapiTray", "service is stopping");
        _process?.Close();
        _process?.Dispose();
        _menu.Dispose();
        return Task.CompletedTask;
    }

    private void OnApplicationStarted()
    {
        if (_server.Features.Get<IServerAddressesFeature>()!
                   .Addresses
                   .FirstOrDefault() is string url
               && _menu.ContextMenu!.Items.First() is PopupMenuItem item
               && item.Text == "server not ready")
        {
            item.Text = url;
            item.Click += (_, _) => _process = CreateProcess(url);
            _menu.Created += (_, _) => _menu.ShowNotification("MinapiTray", "service is started");
            _menu.Create();
        }

        static Process? CreateProcess(string url)
            => Process.Start(
                new ProcessStartInfo(
                    "cmd",
                    $"/c start {url}")
                {
                    CreateNoWindow = true
                });
    }
}