using Application.Services;
using Domain.Entities;
using Domain.Interface;
using Domain.Repositories;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization();  // Обязательно!

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Добавь это перед фильтрами
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebApiSSSS",  // Название API
        Version = "v1.0",          // Версия
        Description = "API для управления клиентами и аутентификацией. Используй Bearer токен для защищённых эндпоинтов.",  // Описание
    });
});

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IRecordRepository, RecordRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<ADatabaseConnection, SqliteConnection>();
builder.Services.AddScoped<MapperConfig, MapperConfig>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddScoped<ClientService, ClientService>();
builder.Services.AddScoped<RecordService, RecordService>();
builder.Services.AddScoped<EmployeeService, EmployeeService>();

builder.Services.AddScoped<Repository<Client>>();  // Сначала зарегистрируй реализацию
builder.Services.AddScoped<IRepository<Client>, Repository<Client>>();  // Затем интерфейс

builder.Services.AddScoped<Repository<Record>>();  // Сначала зарегистрируй реализацию
builder.Services.AddScoped<IRepository<Record>, Repository<Record>>();  // Затем интерфейс

builder.Services.AddScoped<Repository<Employee>>();  // Сначала зарегистрируй реализацию
builder.Services.AddScoped<IRepository<Employee>, Repository<Employee>>();  // Сначала зарегистрируй реализацию


builder.Services.AddDbContext<SqliteConnection>();

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

app.UseAuthentication();  // <-- Это должно быть ПЕРЕД UseAuthorization
app.UseAuthorization();   // <-- А это после

app.MapControllers();

app.Run();
