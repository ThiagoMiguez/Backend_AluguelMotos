using AluguelDeMotos.Shared.Models;
using AluguelDeMotos.Shared.Interfaces;

namespace AluguelDeMotos.Service
{
  public class ReservaService: IReservaService
  {
    private readonly IReservaRepository _reservaRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IRedisCacheService _serviceRedis;
    private readonly IMotoRepository _repoMoto;
    
    private readonly string cacheKeyMotosRedis = "allMotos";
    private readonly string cacheKeyReservasRedis = "allReservas";

    public ReservaService( IReservaRepository reservaRepository, IRedisCacheService serviceRedis, IMotoRepository repoMoto, IEventPublisher eventPublisher)
    {
      _reservaRepository = reservaRepository;
      _serviceRedis = serviceRedis;
      _eventPublisher = eventPublisher;
      _repoMoto = repoMoto;
    }

    // Criar reserva
    public async Task<Reserva> ReservarMoto(int motoId)
    {
      var reservas = await ConsultarReservas();
      var resrvasExistentes = reservas.Where(r => r.MotoId == motoId);
      
      if (resrvasExistentes.Any())
      {
        throw new Exception("Moto já reservada.");
      }

      var reserva =  await _reservaRepository.CriarReserva(motoId);

      if (reserva != null)
      {
        await this.PublicarReservaRabbitMQ(reserva);
      }

      await this.AtualizarCacheReservas();


      return reserva;
    }

    //metodo que será consumido pelo RabbitMQ para confirmar a reserva
    public async Task<Reserva> AtualizarReserva(Reserva reserva)
    {

      var reservaConfirmada = await _reservaRepository.AtualizarReserva(reserva);

      /*
       * ConfirmarReserva
    public async Task<LancamentoModel> UpdateAsync(LancamentoModel model)
    {
      var entity = ModelToEntity(model);
      _set.Update(entity);
      await _context.SaveChangesAsync();
      return EntityToModel(entity);
    }        
       */
      /*if (reserva != null)
      {
        reserva.
      }*/

      return reservaConfirmada;
    }

    //conferir.. rabbitmq vai receber a fila com todos os itens e vai buscar 1 a 1.
    public async Task<IEnumerable<Reserva>> ConsultarReservaEmProcessamento()
    {
      var reservasEmProcessamento = await ConsultarReservasRedis();
      if (reservasEmProcessamento != null && reservasEmProcessamento.Any())
      {
        reservasEmProcessamento.Where(r => r.Status == "EmProcessamento");
        return reservasEmProcessamento;
      }
      
      reservasEmProcessamento = await ConsultarReservasRedis();
      reservasEmProcessamento.Where(r => r.Status == "EmProcessamento");
      return reservasEmProcessamento;
    }

    public async Task<bool> ExcluirReserva(int idReserva)
    {
      var reservas = await ConsultarReservas();
      var resrvasExistentes = reservas.Where(r => r.Id == idReserva);

      if (!resrvasExistentes.Any())
      {
        throw new Exception("Reserva não encontrada.");
      }
      await _reservaRepository.ExcluirReserva(idReserva);

      await this.AtualizarCacheReservas();

      return true;
    }

    // Consultar reserva
    public async Task<IEnumerable<Reserva>> ConsultarReservas()
    {
      var reservas = await _reservaRepository.ConsultarReservas();
      return reservas;
    }

    public async Task<IEnumerable<Reserva>> ConsultarReservasRedis()
    {
      var reservasCache = await _serviceRedis.GetAsync<IEnumerable<Reserva>>(cacheKeyReservasRedis);

      if (reservasCache != null)
        return reservasCache;

      return new List<Reserva>().ToArray();      
    }

    public async Task<bool> RabbitMQTeste()
    {
      var reservaEvent = new ReservaEvent();

      for (int i = 0; i < 10; i++)
      {
        reservaEvent.ReservaId = 0;
        reservaEvent.EventoEmitidoEm = DateTime.Now;
        await _eventPublisher.PublishReservaEventAsync(reservaEvent);
      }
      //await _eventPublisher.PublishReservaEventAsync(reservaEvent);
      //await this.AtualizarCacheReservas();
      return true;
    }

    private async Task PublicarReservaRabbitMQ(Reserva reserva)
    {
      var reservaEvent = new ReservaEvent()
      {
        EventoEmitidoEm = DateTime.Now,
        ReservaId = reserva.Id,
        reserva = reserva
      };

      await _eventPublisher.PublishReservaEventAsync(reservaEvent);
    }

    public async Task AtualizarCacheReservas()
    {
      var reservas = await _reservaRepository.ConsultarReservas();
      var motosDisponiveis = await _repoMoto.ListaMotos();
      var disponiveis = motosDisponiveis.Where(m => !reservas.Any(r => r.MotoId == m.Id));

      await _serviceRedis.SetAsync(cacheKeyReservasRedis, reservas, TimeSpan.FromSeconds(5));
      await _serviceRedis.SetAsync(cacheKeyMotosRedis, disponiveis, TimeSpan.FromSeconds(5));
    }
  }
}
