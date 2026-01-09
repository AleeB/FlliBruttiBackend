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
            .ToListAsync();
    }
}
