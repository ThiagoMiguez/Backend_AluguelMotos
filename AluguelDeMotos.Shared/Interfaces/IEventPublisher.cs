using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Shared.Interfaces
{
  public interface IEventPublisher
  {
    Task PublishReservaEventAsync(ReservaEvent reservaEvent);
  }
}