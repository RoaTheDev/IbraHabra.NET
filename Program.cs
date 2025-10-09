using dotenv.net;
using IbraHabra.NET.Infra.Extension;
using IbraHabra.NET.Infra.Extension.DI;
using IbraHabra.NET.Infra.Middleware;
using IbraHabra.NET.Infra.Persistent;
using Scalar.AspNetCore;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Host.AddWolverineConfig();

var config = builder.Configuration;
var env = builder.Environment;

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddProblemDetails();
builder.Services.AddFluentConfig();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddDatabaseConfig(config);
builder.Services.AddOpenIdDictConfig(config, env);
builder.Services.AddScalarConfig();
builder.Services.RegisterRepo();
builder.Services.RegisterServices();
builder.Services.AddIdentityConfig(config);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    await AppDbContextSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();