using AluguelDeMotos.Service;
using AluguelDeMotos.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;


namespace API.Controllers
{

  [ApiController]
  [Route("api/[controller]")]
  public class MotosController : ControllerBase
  {
    private readonly IMotoService _servico;

    public MotosController(IMotoService service)
    {
      _servico = service;
    }

    // GET api/motos
    [HttpGet]
    public async Task<IActionResult> GetDisponiveis()
    {
      var motos = await _servico.BuscarDisponiveis();
      return Ok(motos);
    }

    [HttpGet("redis")]
    public async Task<IActionResult> GetDisponiveisRedis()
    {
      try
      {
        var motos = await _servico.BuscarDisponiveisRedis();
        return Ok(motos);
      }
      catch (RedisConnectionException ex)
      {
        return StatusCode(500, $"Erro de conexão com Redis: {ex.Message}");
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Ocorreu um erro ao buscar as motos disponíveis no Redis.");
      }
    }
  }  
}
