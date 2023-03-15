
using AutoArchiveLog.Models;
using AutoArchiveLog.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.InitializeServices(builder.Configuration);

builder.Services.AddHostedService<AutoArchiveLogHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapGet("/",
        async context => { await context.Response.WriteAsync("Fanex.AutoArchiveLog.Service"); });

#if DEBUG
app.Run();
#else
await app.RunAsync("http://0.0.0.0:5000");
#endif