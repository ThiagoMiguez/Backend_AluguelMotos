using AluguelDeMotos.Shared.Interfaces;
using AluguelDeMotos.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace AluguelDeMotos.Reserva.Worker
{

  public class ReservaWorker : BackgroundService
  {
    private readonly ILogger<ReservaWorker> _logger;
    private readonly IReservaService _reservaService;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public ReservaWorker(
        ILogger<ReservaWorker> logger,
        IReservaService reservaService,
        IConfiguration configuration)
    {
      _logger = logger;
      _reservaService = reservaService;

      var hostname = configuration.GetValue<string>("RabbitMQ:Hostname") ?? "localhost";
      _queueName = configuration.GetValue<string>("RabbitMQ:QueueName") ?? "reservas";

      var factory = new ConnectionFactory()
      {
        HostName = hostname,
        Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
        UserName = configuration.GetValue<string>("RabbitMQ:UserName", "guest"),
        Password = configuration.GetValue<string>("RabbitMQ:Password", "guest")
      };
      _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
      _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

      _channel.QueueDeclareAsync(
          queue: _queueName,
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null).GetAwaiter().GetResult();

      _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false).GetAwaiter().GetResult();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Worker iniciado e aguardando mensagens");

      var consumer = new AsyncEventingBasicConsumer(_channel);

      consumer.ReceivedAsync += async (sender, ea) =>
      {
        try
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          var evento = JsonSerializer.Deserialize<ReservaEvent>(message);

          if (evento != null)
          {
            await ProcessarReserva(evento);
          }

          await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Erro ao processar mensagem");
          await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        }
      };

      await _channel.BasicConsumeAsync(
          queue: _queueName,
          autoAck: false,
          consumer: consumer,
          cancellationToken: stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
      }
    }

    private async Task ProcessarReserva(ReservaEvent evento)
    {

      try
      {
        _logger.LogInformation("Inicio RabbitMQ", evento.ReservaId);

        // Simula o processamento da mensagem
        await Task.Delay(3000);

        if (evento.ReservaId.Equals(0))
        {
          _logger.LogInformation("RabbitMT teste demonstrativo de fila");
        }
        else
        {
          if (evento.reserva == null)
          {
            _logger.LogWarning("Reserva {ReservaId} não encontrada", evento.ReservaId);
          }
          else
          {
            evento.reserva.Status = "Confirmada";
            await _reservaService.AtualizarReserva(evento.reserva);
            await _reservaService.AtualizarCacheReservas();
            _logger.LogInformation("Reserva {ReservaId} confirmada com sucesso", evento.reserva.Id);
            _logger.LogInformation("-----------------------------------------------------------------");
          }
        }


      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao processar reserva {ReservaId}", evento.ReservaId);
      }
    }

    public override void Dispose()
    {
      _channel?.Dispose();
      _connection?.Dispose();
      base.Dispose();
    }
  }
}