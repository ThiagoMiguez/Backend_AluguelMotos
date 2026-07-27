using AluguelDeMotos.Shared.Models;
using AluguelDeMotos.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AluguelDeMotos.Server
{
  public class ReservaRepository : IReservaRepository
  {
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<Reserva> CriarReserva(int motoId)
    {
      var reserva = new Reserva
      {
        MotoId = motoId,
        Status = "EmProcessamento"
      };

      _context.Reservas.Add(reserva);
      await _context.SaveChangesAsync();

      return reserva;
    }

    public async Task<Reserva> AtualizarReserva(Reserva reserva)
    {
      var reservaExistente = await _context.Reservas.FindAsync(reserva.Id);
      if (reservaExistente == null)
      {
        throw new Exception("Reserva não encontrada.");
      }

      reservaExistente.Status = reserva.Status;
      reservaExistente.MotoId = reserva.MotoId;

      await _context.SaveChangesAsync();

      return reservaExistente;
    }

    public async Task ExcluirReserva(int idReserva)
    {
      var reserva = await _context.Reservas.FindAsync(idReserva);
      if (reserva == null)
      {
        throw new Exception("Reserva não encontrada.");
      }

      _context.Reservas.Remove(reserva);
      await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<Reserva>> ConsultarReservas()
    {
      return _context.Reservas.ToList();
    }

    public async Task<Reserva> ConsultarReserva(int idReserva)
    {
      return _context.Reservas.FirstOrDefault(r => r.Id == idReserva);
    }

    public async Task<IEnumerable<Reserva>> ConsultarReservasRedis()
    {
      var reservas = await _context.Reservas.ToListAsync();
      return reservas;
    }
  }
}
