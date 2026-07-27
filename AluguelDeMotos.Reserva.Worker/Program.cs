using AluguelDeMotos.Moto.Service;
using AluguelDeMotos.Redis.Service;
using AluguelDeMotos.Reserva.Worker;
using AluguelDeMotos.Server;
using AluguelDeMotos.Service;
using AluguelDeMotos.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReservaMotos.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

var conexao = builder.Configuration.GetConnectionString("DefaultConnection") ?? "server=localhost;database=reservamotos;user=root;password=mrktplc";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conexao,
    new MySqlServerVersion(new Version(8, 0, 37))));

#region services
builder.Services.AddHostedService<ReservaWorker>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IMotoService, MotoService>();

#endregion

#region Repository

builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IMotoRepository, MotoRepository>();

#endregion



#region Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IRedisCacheService>(sp => new RedisCacheService(redisConnection));
#endregion

//RabbitMQ
var rabbitMQHost = builder.Configuration.GetValue<string>("RabbitMQ:Hostname") ?? "localhost";
var rabbitMQQueue = builder.Configuration.GetValue<string>("RabbitMQ:QueueName") ?? "reservas";
builder.Services.AddSingleton<IEventPublisher>(sp => new RabbitMQEventPublisher(rabbitMQHost, rabbitMQQueue));

var host = builder.Build();
host.Run();