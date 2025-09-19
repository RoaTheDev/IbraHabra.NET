using dotenv.net;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Infra.Extension;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
builder.Services.AddDatabaseConfig(config);
builder.Host.AddWolverineConfig();

builder.Services.AddOpenApi();
builder.Services.AddIdentityConfig(config);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("api/auth").MapIdentityApi<User>().WithTags("System Authentication");
app.MapControllers();

app.Run();