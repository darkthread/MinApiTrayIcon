using H.NotifyIcon.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

using MinApiTrayIcon;

using System.Diagnostics;
using System.Reflection;
const string appToolTip = "常駐小程式示範";
const string appUuid = "{9BE6C0F7-13F3-47BA-8B91-FB6A50BE09C5}";
// Prevent re-entrance
using Mutex m = new(false, $"Global\\{appUuid}");
if (!m.WaitOne(0, false))
{
    return;
}
CancellationTokenSource _source = new();
string AssemblyName = typeof(Program).Assembly.GetName() is AssemblyName name && name.Name is not null
    ? name.Name : Assembly.GetExecutingAssembly().GetCustomAttribute<RootNamespaceAttribute>()?.RootNamespace
    ?? throw new Exception("unknown resources namespace");
using var indexHtml = GetResourceStream("index.html");
using var icon = new Icon(GetResourceStream("App.ico"));
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .Configure<ExceptionHandlerOptions>(o => o.ExceptionHandler =
        ctx => ctx.Features.Get<IExceptionHandlerFeature>() is IExceptionHandlerFeature feature
            ? ctx.Response.WriteAsJsonAsync(feature.Error.Message)
            : throw new UnreachableException())
    .AddSingleton<TrayIconWithContextMenu>()
    .AddHostedService<PopupHostedService>()

    .AddSingleton(provider =>
    {
        return new 
        {
            Icon = icon.Handle,
            ToolTip = appToolTip,
            ContextMenu = new PopupMenu
            {
                Items =
                {
                    new PopupMenuItem("server not ready", (_, _) => {}),
                    new PopupMenuItem("Exit", (_, _) => _source.Cancel())
                }
            }
        };
    });
var app = builder.Build();
app.UseExceptionHandler();
app.MapPost("/enc", (DataObject data) => AesUtil.AesEncrypt(data.Key, data.Plain));
app.MapPost("/dec", (DataObject data) => AesUtil.AesDecrypt(data.Key, data.Enc));
app.MapFallback("/", ctx => indexHtml.CopyToAsync(ctx.Response.Body));
await app.RunAsync(_source.Token);

string TransferToResourcesName(string name)
    => string.Join(".", AssemblyName, "Assets", name);
Stream GetResourceStream(string path)
    => typeof(Program).Assembly.GetManifestResourceStream(TransferToResourcesName(path))
        ?? throw new FileNotFoundException(path);

record DataObject(string Key, string Plain, string Enc);
