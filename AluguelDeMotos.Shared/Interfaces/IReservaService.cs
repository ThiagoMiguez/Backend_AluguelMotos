using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Shared.Interfaces
{
  public interface IReservaService
  {
    Task<Reserva> ReservarMoto(int motoId);
    Task<IEnumerable<Reserva>> ConsultarReservas();
    Task<IEnumerable<Reserva>> ConsultarReservasRedis();
    Task<bool> ExcluirReserva(int idReserva);
    Task<Reserva> AtualizarReserva(Reserva reserva);
    Task<bool> RabbitMQTeste();
    Task AtualizarCacheReservas();
  }
}
