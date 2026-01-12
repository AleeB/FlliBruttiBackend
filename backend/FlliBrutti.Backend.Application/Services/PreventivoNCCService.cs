using System;
using FlliBrutti.Backend.Application.Extensions;
using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
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
            var userExists = await _context.UsersNotAuthenticated
                .AnyAsync(u => u.IdPerson == preventivo.IdUserNonAutenticato);

            if (!userExists)
            {
                _logger.LogWarning($"UserNotAuthenticated with Id: {preventivo.IdUserNonAutenticato} not found");
                return (false, $"User non autenticato con Id: {preventivo.IdUserNonAutenticato} non trovato");
            }

            var newPreventivo = new PreventivoNCC
            {
                Description = preventivo.Description,
                IsTodo = true,
                Partenza = preventivo.Partenza,
                Arrivo = preventivo.Arrivo,
                IdUserNonAutenticato = preventivo.IdUserNonAutenticato
            };

            await _context.PreventiviNCC.AddAsync(newPreventivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Preventivo created successfully with Id: {newPreventivo.IdPreventivo}");
            return (true, "Preventivo creato con successo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding Preventivo");
            return (false, "Errore durante la creazione del preventivo");
        }
    }

    public async Task<IEnumerable<PreventivoNCCResponseDTO>> GetPreventiviToExamineAsync(bool isTodo)
    {
        try
        {
            var preventivi = await _context.PreventiviNCC
                .Include(p => p.IdUserNavigation)
                    .ThenInclude(u => u.IdPersonNavigation)
                .Include(p => p.IdUserNonAutenticatoNavigation)
                    .ThenInclude(u => u.IdPersonNavigation)
                .Include(p => p.PreventivoExtra)
                .Where(p => p.IsTodo == isTodo)
                .ToListAsync();

            return preventivi.Select(p => p.ToResponseDTO());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Preventivi to examine");
            return Enumerable.Empty<PreventivoNCCResponseDTO>();
        }
    }

    public async Task<PreventivoNCCResponseDTO> GetPreventivoByIdAsync(long id)
    {
        try
        {
            var preventivo = await _context.PreventiviNCC
                .Include(p => p.IdUserNavigation)
                    .ThenInclude(u => u.IdPersonNavigation)
                .Include(p => p.IdUserNonAutenticatoNavigation)
                    .ThenInclude(u => u.IdPersonNavigation)
                .Include(p => p.PreventivoExtra)
                .FirstOrDefaultAsync(p => p.IdPreventivo == id);

            if (preventivo == null)
            {
                _logger.LogWarning($"Preventivo with Id: {id} not found");
                return null;
            }

            return preventivo.ToResponseDTO();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving Preventivo with Id: {id}");
            return null;
        }
    }

    public async Task<bool> UpdatePreventivoNCCAsync(long id, PreventivoUpdateNCCDTO preventivo)
    {
        try
        {
            var existingPreventivo = await _context.PreventiviNCC
                .Include(p => p.PreventivoExtra)
                .FirstOrDefaultAsync(p => p.IdPreventivo == id);

            if (existingPreventivo == null)
            {
                _logger.LogWarning($"Preventivo with Id: {id} not found");
                return false;
            }

            // Aggiorna i campi
            existingPreventivo.Description = preventivo.Description;
            existingPreventivo.IsTodo = false;
            existingPreventivo.IdUser = preventivo.IdUser;
            existingPreventivo.Partenza = preventivo.Partenza;
            existingPreventivo.Arrivo = preventivo.Arrivo;
            existingPreventivo.Costo = preventivo.Costo;

            // Rimuovi gli extra esistenti
            _context.PreventiviNCC.Entry(existingPreventivo)
                .Collection(p => p.PreventivoExtra)
                .Load();

            existingPreventivo.PreventivoExtra.Clear();

            // Aggiungi i nuovi extra
            foreach (var extraDto in preventivo.Extra)
            {
                existingPreventivo.PreventivoExtra.Add(extraDto.ToModel());
            }

            _context.PreventiviNCC.Update(existingPreventivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Preventivo with Id: {id} updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating Preventivo with Id: {id}");
            return false;
        }
    }

}
