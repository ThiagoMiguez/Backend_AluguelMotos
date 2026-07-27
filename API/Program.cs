using AluguelDeMotos.Moto.Service;
using AluguelDeMotos.Redis.Service;
using AluguelDeMotos.Server;
using AluguelDeMotos.Service;
using AluguelDeMotos.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using ReservaMotos.Infrastructure.Messaging;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 37))));


#region  Registrar repositórios

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMotoRepository, MotoRepository>();

#endregion

#region  Registrar services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();


//services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMotoService, MotoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();

// Cache Service

var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IRedisCacheService>(sp => new RedisCacheService(redisConnection));


//RabbitMQ
var rabbitMQHost = builder.Configuration.GetValue<string>("RabbitMQ:Hostname") ?? "localhost";
var rabbitMQQueue = builder.Configuration.GetValue<string>("RabbitMQ:QueueName") ?? "reservas";
builder.Services.AddSingleton<IEventPublisher>(sp => new RabbitMQEventPublisher(rabbitMQHost, rabbitMQQueue));

#endregion



//builder.Services.AddSingleton<MotoService>();
//builder.Services.AddSingleton<ReservaService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar suporte a controllers
builder.Services.AddControllers();



var app = builder.Build();

// Habilitar CORS
app.UseCors(options => options
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod());

// Habilitar CORS
app.UseCors("AllowAngular");

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
