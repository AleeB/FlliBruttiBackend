using System;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlliBrutti.Backend.Application.Services;

public class PreventivoNCCService : IPreventivoNCCService
{

    private readonly IFlliBruttiContext _context;
    private readonly ILogger _logger;
    public PreventivoNCCService(IFlliBruttiContext context, ILogger<PreventivoNCCService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool, string)> AddPreventivoNCCAsync(PreventivoNCCDTO preventivo)
    {
        try
        {
            var newPreventivo = new PreventivoNCC
            {
                Description = preventivo.Description,
                Partenza = preventivo.Partenza,
                Arrivo = preventivo.Arrivo,
                IdUserNonAutenticato = preventivo.IdUserNonAutenticato
            };

            await _context.PreventiviNCC.AddAsync(newPreventivo);
            await _context.SaveChangesAsync();

            return (true, "PreventivoNCC added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding PreventivoNCC");
            return (false, $"Error adding PreventivoNCC: {ex.Message}");
        }
    }

    public async Task<IEnumerable<PreventivoNCC>> GetPreventiviToExamineAsync(bool isTodo)
    {
        return await _context.PreventiviNCC
            .Where(p => p.IsTodo.Equals(isTodo))
            .Include(p => p.PreventivoExtra)
            .ToListAsync();
    }
    //TODO: Fare le response con DTO, così da evitare select cicliche
    public async Task<PreventivoNCC> GetPreventivoByIdAsync(long id)
    {
        var res = await _context.PreventiviNCC
            .Where(p => p.IdPreventivo == id)
            .Include(p => p.PreventivoExtra)
            .
            .FirstOrDefaultAsync();
        if (res == default)
        {
            return null!;
        }
        return res;
    }

    public async Task<bool> UpdatePreventivoNCCAsync(long id, PreventivoUpdateNCCDTO preventivo)
    {
        var existingPreventivo = await _context.PreventiviNCC
            .FirstOrDefaultAsync(p => p.IdPreventivo == id);
        if (existingPreventivo == default)
        {
            return false;
        }
        existingPreventivo.Description = preventivo.Description;
        existingPreventivo.IsTodo = preventivo.IsTodo;
        existingPreventivo.Partenza = preventivo.Partenza;
        existingPreventivo.Arrivo = preventivo.Arrivo;
        existingPreventivo.IdUser = preventivo.IdUser;
        existingPreventivo.Costo = preventivo.Costo;
        var preventivoExtraConverted = preventivo.Extra.Select(e => new PreventivoExtra
        {
            Description = e.Description,
            Costo = e.Costo,
            IdPreventivo = existingPreventivo.IdPreventivo

        }).ToList();
        foreach (var extra in preventivoExtraConverted)
        {
            existingPreventivo.PreventivoExtra.Add(extra);
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
