using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Shared.Interfaces
{ 
  public interface IReservaRepository
  {
    Task<Reserva> CriarReserva(int motoId);
    Task ExcluirReserva(int idReserva);
    Task<IEnumerable<Reserva>> ConsultarReservas();
    Task<IEnumerable<Reserva>> ConsultarReservasRedis();
    Task<Reserva> ConsultarReserva(int idReserva);
    Task<Reserva> AtualizarReserva(Reserva reserva);
  }
}
