using AluguelDeMotos.Shared.Interfaces;
using AluguelDeMotos.Shared.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ReservaMotos.Infrastructure.Messaging;

public class RabbitMQEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly string _queueName;
    private readonly ConnectionFactory _connectionFactory;

  public RabbitMQEventPublisher(string hostname, string queueName)
    {
    //var factory = new ConnectionFactory() { HostName = hostname };
    _connectionFactory = new ConnectionFactory()
    {
      HostName = hostname,
      Port = 5672,
      UserName = "guest",
      Password = "guest"
    };
    _queueName = queueName;
    /*
    _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    
    _queueName = queueName;
    _hostname = hostname;
    */
  }

  public async Task PublishReservaEventAsync(ReservaEvent reservaEvent)
    {/* var factory = new ConnectionFactory()
        {
          HostName = _hostname,
          Port = 5672,
          UserName = "guest",
          Password = "guest"
        };
    */
    await using var connection = _connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();    
    await using var channel = await connection.CreateChannelAsync();
      //await using var channel = await _connection.CreateChannelAsync();

      await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = JsonSerializer.Serialize(reservaEvent);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
