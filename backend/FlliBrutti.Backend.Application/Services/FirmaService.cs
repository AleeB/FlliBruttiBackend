using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.Services
{
    public class FirmaService: IFirmaService
    {

        private readonly IFlliBruttiContext _context;
        private readonly ILogger _logger;

        public FirmaService(IFlliBruttiContext context, ILogger<FirmaService> logger) 
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool, string)> CreateFirma(long idUser)
        {
            
            if(await checkEntryOpens(idUser))
            {
                _logger.LogWarning($"User with Id: {idUser} already has an open Firma entry.");
                return (false, $"User: {idUser} already has an open Firma entry.");
            }

            if(await _context.Users.AnyAsync(u => u.IdPerson == idUser) == false)
            {
                _logger.LogWarning($"Attempted to create Firma for non-existent user with Id: {idUser}");
                return (false, $"User: {idUser} does not exist.");
            }
            await _context.Firme.AddAsync(new Firma
            {
                IdUser = idUser,
                Entrata = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return (true, "Firma created successfully.");
        }

        private async Task<bool> checkEntryOpens(long idUser)
        {
            return await _context.Firme.AnyAsync(f => f.IdUser == idUser && f.Uscita == null);
        }

        public async Task<(bool, string)> ExitFirma(long idUser, DateOnly date)
        {
            var res = await _context.Firme.Where(x => x.IdUser == idUser && x.Entrata.Value.Date.Equals(date)).FirstOrDefaultAsync();
            if (res == null)
            {
                return (false, $"No open Firma entry found for the specified user: {idUser} and date.");
            }
            if(res.Uscita != null)
            {
                return (false, $"Firma entry for user: {idUser} on date: {date} is already closed.");
            }

            _context.Firme.Update(new Firma
            {
                Idfirma = res.Idfirma,
                Uscita = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return (true, "Firma exit recorded successfully.");
        }

        public async Task<IEnumerable<Firma>> GetFirmaByIdUserAsync(long idUser)
        {
            try
            {
                var firme = await _context.Firme.Where(f => f.IdUser == idUser).ToListAsync();
                if(firme == null || firme.Count == 0)
                {
                    _logger.LogWarning($"No Sign records found for IdUser: {idUser}");
                    return Enumerable.Empty<Firma>();
                }
                _logger.LogInformation($"Retrieved {firme.Count} Sign records for IdUser: {idUser}");
                return firme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving Signs records for IdUser: {idUser}. \n Exception: {ex.Message}");
                return Enumerable.Empty<Firma>();
            }
        }

    }
}
