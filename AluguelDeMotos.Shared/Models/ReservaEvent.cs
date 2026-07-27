namespace AluguelDeMotos.Shared.Models
{
  public class ReservaEvent
  {
    public int ReservaId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EventoEmitidoEm { get; set; }
    public Reserva reserva { get; set; }
  }
}
