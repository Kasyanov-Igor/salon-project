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

builder.Services.AddAuthorization();  // �����������!

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // ������ ��� ����� ���������
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebApiSSSS",  // �������� API
        Version = "v1.0",          // ������
        Description = "API ��� ���������� ��������� � ���������������. ��������� Bearer ����� ��� ���������� ����������.",  // ��������
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

builder.Services.AddScoped<Repository<Client>>();  // ������� ������������� ����������
builder.Services.AddScoped<IRepository<Client>, Repository<Client>>();  // ����� ���������

builder.Services.AddScoped<Repository<Record>>();  // ������� ������������� ����������
builder.Services.AddScoped<IRepository<Record>, Repository<Record>>();  // ����� ���������

builder.Services.AddScoped<Repository<Employee>>();  // ������� ������������� ����������
builder.Services.AddScoped<IRepository<Employee>, Repository<Employee>>();  // ������� ������������� ����������


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

app.UseAuthentication();  // <-- ��� ������ ���� ����� UseAuthorization
app.UseAuthorization();   // <-- � ��� �����

app.MapControllers();

app.Run();
