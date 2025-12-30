using System;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FlliBrutti.Backend.Application.IContext;

public interface IFlliBruttiContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserNotAuthenticated> UsersNotAuthenticated { get; set; }
    public DbSet<Firma> Firme { get; set; }
    public DbSet<FormulaPreventivo> FormulaPreventivo { get; set; }
    public DbSet<Preventivo> Preventivi { get; set; }
    public DbSet<Person> People { get; set; }
}
