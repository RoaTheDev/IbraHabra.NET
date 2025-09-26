using dotenv.net;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Infra.Extension;
using Scalar.AspNetCore;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Host.AddWolverineConfig();

var config = builder.Configuration;
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddDatabaseConfig(config);
builder.Services.AddOpenIdDictConfig();
builder.Services.AddScalarConfig();
builder.Services.AddIdentityConfig(config);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("api/auth").MapIdentityApi<User>().WithTags("System Authentication");
app.MapControllers();

app.Run();