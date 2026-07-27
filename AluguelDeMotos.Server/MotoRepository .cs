using AluguelDeMotos.Shared.Interfaces;
using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Server
{
  public class MotoRepository : IMotoRepository
  {
    private readonly AppDbContext _context;

    public MotoRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<Moto>> ListaMotos()
    {
      return _context.Motos.ToList();
    }

    public Moto BuscarPorId(int id)
    {
      return _context.Motos.FirstOrDefault(m => m.Id == id);
    }
  }
}
