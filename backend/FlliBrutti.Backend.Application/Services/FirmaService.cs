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

        public async Task<bool> CreateFirma(long idUser)
        {
            if(await _context.Users.FindAsync(idUser) == null)
            {
                _logger.LogWarning($"Attempted to create Firma for non-existent user with Id: {idUser}");
                return false;
            }
            await _context.Firme.AddAsync(new Firma
            {
                IdUser = idUser,
                Entrata = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExitFirma(long idUser, DateOnly date)
        {
            var res = await _context.Firme.Where(x => x.IdUser == idUser && x.Entrata.Value.Date.Equals(date)).FirstOrDefaultAsync();
            if (res == null)
            {
                return false;
            }
            _context.Firme.Update(new Firma
            {
                Idfirma = res.Idfirma,
                Uscita = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
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
