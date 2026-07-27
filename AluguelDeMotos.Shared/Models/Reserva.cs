using System;
using System.Collections.Generic;
using System.Text;

namespace AluguelDeMotos.Shared.Models
{
  public class Reserva
  {
    public int Id { get; set; }
    public int MotoId { get; set; }
    public string Status { get; set; } // "Reservada", "Concluída", "Cancelada"
  }
}
