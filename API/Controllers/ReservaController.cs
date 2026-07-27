using AluguelDeMotos.Service;
using AluguelDeMotos.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ReservasController : ControllerBase
  {
    private readonly IReservaService _servico;

    public ReservasController(IReservaService service)
    {
      _servico = service;
    }


    // POST api/reservas/{motoId}
    [HttpPost("{motoId}")]
    public async Task<IActionResult> CriarReserva(int motoId)
    {
      try
      {
        var reserva = await _servico.ReservarMoto(motoId);
        return Ok(reserva);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    // Delete api/reservas/{motoId}
    [HttpDelete("{idReserva}")]
    public async Task<IActionResult> Excluir(int idReserva)
    {
      try
      {
        var reserva = await _servico.ExcluirReserva(idReserva);
        return Ok(reserva);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    // GET api/reservas/
    [HttpGet]
    public async Task<IActionResult> ConsultarReservas()
    {
      try
      {
        var reservas = await _servico.ConsultarReservas();
        if (reservas == null)
          return NotFound();

        return Ok(reservas);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("redis")]
    public async Task<IActionResult> GetDisponiveisRedis()
    {
      try
      {
        var motos = await _servico.ConsultarReservasRedis();
        return Ok(motos);
      }
      catch (RedisConnectionException ex)
      {
        return StatusCode(500, $"Erro de conexão com Redis: {ex.Message}");
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Ocorreu um erro ao buscar as reservas disponíveis no Redis.");
      }
    }

    [HttpPost("RabbitMQ_Teste")]
    public async Task<IActionResult> RabbitMQTeste()
    {
      try
      {
        var reserva = await _servico.RabbitMQTeste();
        return Ok(reserva);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
