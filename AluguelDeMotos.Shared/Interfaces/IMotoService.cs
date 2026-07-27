using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Shared.Interfaces
{
  public interface IMotoService
  {
    Task<IEnumerable<Moto>> BuscarDisponiveis();
    Task<IEnumerable<Moto>> BuscarDisponiveisRedis();
  }
}
