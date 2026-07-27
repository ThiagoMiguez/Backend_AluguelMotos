using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Shared.Interfaces
{
  public interface IMotoRepository
  {
    Task<IEnumerable<Moto>> ListaMotos();
    Moto BuscarPorId(int id);
  }
}
