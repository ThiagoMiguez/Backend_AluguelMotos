using Microsoft.EntityFrameworkCore;
using AluguelDeMotos.Shared.Models;

namespace AluguelDeMotos.Server
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Moto> Motos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
  }
}
