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
    //private readonly IServiceProvider _serviceProvider;
    private readonly IReservaService _reservaService;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public ReservaWorker(
        ILogger<ReservaWorker> logger,
        //IServiceProvider serviceProvider,
        IReservaService reservaService,
        IConfiguration configuration)
    {
      _logger = logger;
      //_serviceProvider = serviceProvider;
      _reservaService = reservaService;

      var hostname = configuration.GetValue<string>("RabbitMQ:Hostname") ?? "localhost";
      _queueName = configuration.GetValue<string>("RabbitMQ:QueueName") ?? "reservas";

      //var factory = new ConnectionFactory() { HostName = hostname };
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
      //using var scope = _serviceProvider.CreateScope();

      /*
      var reservaRepository = scope.ServiceProvider.GetRequiredService<IReservaRepository>();
      var motoRepository = scope.ServiceProvider.GetRequiredService<IMotoRepository>();
      var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

      _logger.LogInformation("Processando reserva {ReservaId} para moto {MotoId}",
          evento.ReservaId, evento.MotoId);*/

      try
      {
        //var reserva = await reservaRepository.GetByIdAsync(evento.ReservaId);
        /*var reserva = await reservaRepository.GetByIdAsync(evento.ReservaId);
        _reservaService.ConfirmarReserva
        if (reserva == null)
        {
          _logger.LogWarning("Reserva {ReservaId} não encontrada", evento.ReservaId);
          return;
        }*/

        /*
        var moto = await motoRepository.GetByIdAsync(evento.MotoId);
        if (moto == null)
        {
          _logger.LogWarning("Moto {MotoId} não encontrada", evento.MotoId);
          reserva.Status = StatusReserva.Erro;
          reserva.MensagemErro = "Moto não encontrada";
          await reservaRepository.UpdateAsync(reserva);
          return;
        }

        if (!moto.Disponivel)
        {
          _logger.LogInformation("Moto {MotoId} não está disponível", evento.MotoId);
          reserva.Status = StatusReserva.Cancelado;
          reserva.MensagemErro = "Moto não disponível";
          await reservaRepository.UpdateAsync(reserva);
          await AtualizarCache(reserva, moto, cacheService);
          return;
        }

        moto.Disponivel = false;
        await motoRepository.UpdateAsync(moto);

        reserva.Status = StatusReserva.Confirmado;
        await reservaRepository.UpdateAsync(reserva);

        await AtualizarCache(reserva, moto, cacheService);
        */
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
          }
        }


      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao processar reserva {ReservaId}", evento.ReservaId);
        /*
        var reserva = await reservaRepository.GetByIdAsync(evento.ReservaId);
        if (reserva != null)
        {
          reserva.Status = StatusReserva.Erro;
          reserva.MensagemErro = ex.Message;
          await reservaRepository.UpdateAsync(reserva);
        }*/
      }
    }

    /*private async Task AtualizarCache(Reserva reserva, Moto moto, ICacheService cacheService)
    {
      var statusReserva = new
      {
        ReservaId = reserva.Id,
        Status = reserva.Status.ToString(),
        MotoDisponivel = moto.Disponivel,
        MensagemErro = reserva.MensagemErro,
        UltimaAtualizacao = reserva.AtualizadoEm ?? reserva.CriadoEm
      };

      await cacheService.SetAsync($"reserva:{reserva.Id}:status", statusReserva, TimeSpan.FromMinutes(10));
      await cacheService.SetAsync($"moto:{moto.Id}:disponivel", moto.Disponivel, TimeSpan.FromMinutes(10));

      _logger.LogInformation("Cache atualizado para reserva {ReservaId}", reserva.Id);
    }*/

    public override void Dispose()
    {
      _channel?.Dispose();
      _connection?.Dispose();
      base.Dispose();
    }
  }
}