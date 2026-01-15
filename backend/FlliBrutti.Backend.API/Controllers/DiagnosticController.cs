using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly ILogger<DiagnosticController> _logger;
        private readonly IFlliBruttiContext _context;
        private readonly FlliBruttiContext _concreteContext; // Per accedere a ChangeTracker

        public DiagnosticController(
            ILogger<DiagnosticController> logger,
            IFlliBruttiContext context)
        {
            _logger = logger;
            _context = context;
            _concreteContext = (FlliBruttiContext)context; // Cast per ChangeTracker
        }

        [HttpGet("memory")]
        public IActionResult GetMemoryInfo()
        {
            var process = Process.GetCurrentProcess();

            // Forza un GC leggero per vedere la memoria reale
            var beforeGC = GC.GetTotalMemory(false);
            GC.Collect(0, GCCollectionMode.Forced, false);
            var afterGC = GC.GetTotalMemory(false);

            var memoryInfo = new
            {
                ProcessMemory = new
                {
                    WorkingSet = $"{process.WorkingSet64 / 1024 / 1024} MB",
                    PrivateMemory = $"{process.PrivateMemorySize64 / 1024 / 1024} MB"
                },
                GCMemory = new
                {
                    TotalMemoryBeforeGC = $"{beforeGC / 1024 / 1024} MB",
                    TotalMemoryAfterGC = $"{afterGC / 1024 / 1024} MB",
                    FreedByGC = $"{(beforeGC - afterGC) / 1024 / 1024} MB"
                },
                GCStats = new
                {
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2)
                }
            };

            _logger.LogInformation("Memory diagnostic: {@MemoryInfo}", memoryInfo);
            return Ok(memoryInfo);
        }

        [HttpGet("tokens")]
        public async Task<IActionResult> GetTokenStats()
        {
            try
            {
                var tokenStats = await _context.RefreshTokens
                    .AsNoTracking()
                    .GroupBy(rt => rt.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalTokens = g.Count(),
                        ActiveTokens = g.Count(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow),
                        RevokedTokens = g.Count(t => t.IsRevoked),
                        ExpiredTokens = g.Count(t => t.ExpiresAt <= DateTime.UtcNow && !t.IsRevoked)
                    })
                    .ToListAsync();

                var totalStats = new
                {
                    TotalUsers = tokenStats.Count,
                    TotalTokens = await _context.RefreshTokens.CountAsync(),
                    TotalActive = await _context.RefreshTokens
                        .CountAsync(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow),
                    TotalRevoked = await _context.RefreshTokens.CountAsync(t => t.IsRevoked),
                    TotalExpired = await _context.RefreshTokens
                        .CountAsync(t => t.ExpiresAt <= DateTime.UtcNow && !t.IsRevoked),
                    PerUser = tokenStats
                };

                _logger.LogInformation("Token stats: {@TokenStats}", totalStats);
                return Ok(totalStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token stats");
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpGet("dbcontext")]
        public IActionResult GetDbContextInfo()
        {
            try
            {
                var trackedCount = _concreteContext.ChangeTracker.Entries().Count();

                var contextInfo = new
                {
                    ContextType = _context.GetType().Name,
                    IsTracking = trackedCount > 0,
                    TrackedEntitiesCount = trackedCount,
                    TrackedEntities = _concreteContext.ChangeTracker.Entries()
                        .Select(e => new
                        {
                            EntityType = e.Entity.GetType().Name,
                            State = e.State.ToString()
                        })
                        .ToList()
                };

                _logger.LogInformation("DbContext info: {@ContextInfo}", contextInfo);
                return Ok(contextInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DbContext info");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("gc/full")]
        public IActionResult ForceFullGarbageCollection()
        {
            var beforeMemory = GC.GetTotalMemory(false) / 1024 / 1024;

            _logger.LogInformation("🗑️ Forcing FULL garbage collection... Memory before: {BeforeMemory} MB", beforeMemory);

            // GC completo su tutte le generazioni
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);

            var afterMemory = GC.GetTotalMemory(true) / 1024 / 1024;

            var result = new
            {
                BeforeGC = $"{beforeMemory} MB",
                AfterGC = $"{afterMemory} MB",
                Freed = $"{beforeMemory - afterMemory} MB",
                FreedPercentage = beforeMemory > 0 ? $"{((beforeMemory - afterMemory) / beforeMemory * 100):F1}%" : "0%"
            };

            _logger.LogInformation("✅ Garbage collection completed: {@Result}", result);
            return Ok(result);
        }

        [HttpDelete("tokens/cleanup")]
        public async Task<IActionResult> CleanupOldTokens()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-1);

                var tokensToRemove = await _context.RefreshTokens
                    .Where(rt => (rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow)
                              && rt.CreatedAt < cutoffDate)
                    .ToListAsync();

                if (tokensToRemove.Any())
                {
                    _context.RefreshTokens.RemoveRange(tokensToRemove);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} old tokens", tokensToRemove.Count);
                    return Ok(new { message = $"Removed {tokensToRemove.Count} tokens" });
                }

                return Ok(new { message = "No tokens to cleanup" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up tokens");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("leak-test")]
        public async Task<IActionResult> LeakTest()
        {
            try
            {
                var beforeMemory = GC.GetTotalMemory(false) / 1024 / 1024;
                var beforeTracked = _concreteContext.ChangeTracker.Entries().Count();

                // Simula query che potrebbero causare leak
                var users = await _context.Users
                    .Include(u => u.IdPersonNavigation)
                    .Take(10)
                    .ToListAsync();

                var afterMemory = GC.GetTotalMemory(false) / 1024 / 1024;
                var afterTracked = _concreteContext.ChangeTracker.Entries().Count();

                return Ok(new
                {
                    MemoryBefore = $"{beforeMemory} MB",
                    MemoryAfter = $"{afterMemory} MB",
                    MemoryIncrease = $"{afterMemory - beforeMemory} MB",
                    TrackedEntitiesBefore = beforeTracked,
                    TrackedEntitiesAfter = afterTracked,
                    NewTrackedEntities = afterTracked - beforeTracked,
                    Warning = afterTracked > beforeTracked
                        ? "⚠️ ENTITÀ TRACCIATE! Manca AsNoTracking()"
                        : "✅ OK - Nessuna entità tracciata"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in leak test");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("login-simulation")]
        public async Task<IActionResult> SimulateLoginMemoryUsage()
        {
            try
            {
                _logger.LogInformation("🧪 Starting login memory simulation");

                var beforeMemory = GC.GetTotalMemory(false) / 1024 / 1024;
                var beforeTracked = _concreteContext.ChangeTracker.Entries().Count();

                // Simula esattamente cosa fa il login
                var testEmail = "test@example.com";

                // 1. LoginService: verifica credenziali
                var userForValidation = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == testEmail);

                // 2. UserService: ottieni dati utente
                var userForResponse = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.IdPersonNavigation)
                    .FirstOrDefaultAsync(u => u.Email == testEmail);

                // 3. JwtService: controlla token esistenti
                var existingTokens = await _context.RefreshTokens
                    .AsNoTracking()
                    .Where(rt => rt.UserId == (userForResponse != null ? userForResponse.IdPerson : 0))
                    .ToListAsync();

                var afterMemory = GC.GetTotalMemory(false) / 1024 / 1024;
                var afterTracked = _concreteContext.ChangeTracker.Entries().Count();

                return Ok(new
                {
                    MemoryBefore = $"{beforeMemory} MB",
                    MemoryAfter = $"{afterMemory} MB",
                    MemoryIncrease = $"{afterMemory - beforeMemory} MB",
                    TrackedEntitiesBefore = beforeTracked,
                    TrackedEntitiesAfter = afterTracked,
                    ExistingTokensFound = existingTokens.Count,
                    UserFound = userForResponse != null,
                    Warning = afterTracked > beforeTracked
                        ? $"⚠️ {afterTracked - beforeTracked} ENTITÀ TRACCIATE dopo login simulation!"
                        : "✅ OK - Nessuna entità tracciata"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login simulation");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}