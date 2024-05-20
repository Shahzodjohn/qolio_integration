using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebHook.Enums;
using WebHook;
using WebHook.interfaces;
using Serilog;
using Ngrok.AgentAPI;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

DotNetEnv.Env.Load();
var connectionString = Environment.GetEnvironmentVariable("CSTR");


builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseNpgsql(connectionString).UseLazyLoadingProxies());

builder.Services.AddScoped<IServiceInterface, Services>();
var app = builder.Build();

app.UseStaticFiles();

app.MapPost("/webhook", async context =>
{
    Log.Information("New conversation...");

    var requestBody = await context.Request.ReadFromJsonAsync<JsonElement>();
    
    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;

        var myDependency = services.GetRequiredService<IServiceInterface>();

        await myDependency.SaveToDb(requestBody);
    }

});


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();