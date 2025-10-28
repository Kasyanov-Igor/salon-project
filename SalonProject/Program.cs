using Application.Services;
using Domain.Entities;
using Domain.Interface;
using Domain.Repositories;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ADatabaseConnection, SqliteConnection>();
builder.Services.AddScoped<MapperConfig, MapperConfig>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddScoped<ClientService, ClientService>();

builder.Services.AddScoped<Repository<Client>>();  // Сначала зарегистрируй реализацию
builder.Services.AddScoped<IRepository<Client>, Repository<Client>>();  // Затем интерфейс


var startup = new WebApplication1.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
