using H.NotifyIcon.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Diagnostics;
using MinApiTrayIcon;

const string appToolTip = "常駐小程式示範";
const string appUuid = "{9BE6C0F7-13F3-47BA-8B91-FB6A50BE09C5}";

// Prevent re-entrance
using (Mutex m = new Mutex(false, $"Global\\{appUuid}"))
{
    if (!m.WaitOne(0, false))
    {
        return;
    }

    bool exitFlag = false;
    
    var builder = WebApplication.CreateBuilder(args);
    var app = builder.Build();

    app.MapGet("/", () => Results.Content(@"<!DOCTYPE html>
<html><head>
    <meta charset=utf-8>
    <title>AES256 Encryption/Decryption Demo</title>
    <style>
    textarea { width: 300px; display: block; margin-top: 3px; }
    div > * { margin-right: 3px; }
    </style>
</head>
<body>
    <div>
    <input id=key /><button onclick=encrypt()>Encrypt</button><button onclick=decrypt()>Decrypt</button>
    </div>
    <textarea id=plain></textarea>
    <textarea id=enc></textarea>
    <script>
    let setVal = (id,v) => document.getElementById(id).value=v;
    let val = (id) => document.getElementById(id).value;
    let getFetchOpt = (data) => {
        return {
            method: 'POST', headers: { 'Content-Type': 'application/json', 'Accept': 'text/plain' },
            body: JSON.stringify(data)
        }
    };
    function encrypt() { 
        setVal('enc', '');
        fetch('/enc',getFetchOpt({ key: val('key'), plain: val('plain') }))
        .then(r => r.text()).then(t => setVal('enc',t)); }
    function decrypt() { 
        setVal('plain', '');
        fetch('/dec',getFetchOpt({ key: val('key'), enc: val('enc') }))
        .then(r => r.text()).then(t => setVal('plain',t)); }
    </script>
</body></html>", "text/html"));
    Func<Func<string>, string> catchEx = (fn) =>
    {
        try { return fn(); } catch (Exception ex) { return "ERROR:" + ex.Message; }
    };
    app.MapPost("/enc", (DataObject data) => catchEx(() => AesUtil.AesEncrypt(data.key, data.plain)));
    app.MapPost("/dec", (DataObject data) => catchEx(() => AesUtil.AesDecrypt(data.key, data.enc)));

    var task = app.RunAsync();

    // Get web url
    var url = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()
        .Addresses.First();

    // Tray Icon
    using var iconStream = typeof(Program).Assembly.GetManifestResourceStream($"MinApiTrayIcon.App.ico");
    using var icon = new Icon(iconStream);
    using var trayIcon = new TrayIconWithContextMenu
    {
        Icon = icon.Handle,
        ToolTip = appToolTip
    };
    trayIcon.ContextMenu = new PopupMenu()
    {
        Items =
        {
            new PopupMenuItem(url, (_, _) =>
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {
                    CreateNoWindow= true
                });
            }),
            new PopupMenuItem("Exit", (_, _)=>
            {
                trayIcon.Dispose();
                exitFlag = true;
            })
        }
    };
    trayIcon.Create();
    trayIcon.Show();

    var appLife = app.Services.GetRequiredService<IHostApplicationLifetime>();
    Task.Factory.StartNew(async () =>
    {
        while (!exitFlag)
        {
            await Task.Delay(100);
        }
        appLife.StopApplication();
    });

    task.Wait();
}

class DataObject
{
    public string key { get; set; }
    public string plain { get; set; }
    public string enc { get; set; }
}
