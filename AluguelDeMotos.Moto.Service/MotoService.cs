using AluguelDeMotos.Shared.Interfaces;

namespace AluguelDeMotos.Moto.Service
{
  public class MotoService : IMotoService
  {

    private readonly IMotoRepository _repo;
    private readonly IRedisCacheService _serviceRedis;
    private readonly IReservaRepository _reservaRepository;
    private readonly string cacheKeyMotosRedis = "allMotos";

    public MotoService(IMotoRepository repo, IReservaRepository reservaRepository, IRedisCacheService serviceRedis)
    {
      _repo = repo;
      _reservaRepository = reservaRepository;
      _serviceRedis = serviceRedis;
    }

    // Buscar motos disponíveis
    public async Task<IEnumerable<Shared.Models.Moto>> BuscarDisponiveis()
    {
      var motos = await _repo.ListaMotos();
      var reservas = await _reservaRepository.ConsultarReservas();

      return motos.Where(m => !reservas.Any(r => r.MotoId == m.Id));
    }

    public async Task<IEnumerable<Shared.Models.Moto>> BuscarDisponiveisRedis()
    {
      var motosCache = await _serviceRedis.GetAsync<IEnumerable<Shared.Models.Moto>>(cacheKeyMotosRedis);

      if (motosCache != null)
        return motosCache;

      return new List<Shared.Models.Moto>().ToArray();
    }
  }
}
