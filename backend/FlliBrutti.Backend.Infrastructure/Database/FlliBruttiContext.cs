using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FlliBrutti.Backend.Infrastructure.Database;

public class FlliBruttiContext : DbContext, IFlliBruttiContext
{
    public FlliBruttiContext(DbContextOptions<FlliBruttiContext> options)
        : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserNotAuthenticated> UsersNotAuthenticated { get; set; }
    public DbSet<Firma> Firme { get; set; }
    public DbSet<FormulaPreventivo> FormulaPreventivo { get; set; }
    public DbSet<PreventivoNCC> PreventiviNCC { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Firma>(entity =>
        {
            entity.HasKey(e => e.Idfirma).HasName("PRIMARY");

            entity.ToTable("firme");

            entity.HasIndex(e => e.IdUser, "idUser_idx");

            entity.Property(e => e.Idfirma).HasColumnName("idfirme");
            entity.Property(e => e.Entrata)
                .HasColumnType("datetime")
                .HasColumnName("entrata");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.Uscita)
                .HasColumnType("datetime")
                .HasColumnName("uscita");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Firme)
                .HasPrincipalKey(p => p.IdPerson)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUser");
        });

        modelBuilder.Entity<FormulaPreventivo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("formulapreventivo");

            entity.Property(e => e.CostoKm).HasColumnName("costo_km");
            entity.Property(e => e.PrimoAutista).HasColumnName("primo_autista");
            entity.Property(e => e.SecontoAutista).HasColumnName("seconto_autista");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.IdPerson).HasName("PRIMARY");

            entity.ToTable("people");

            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.PhoneNumber)
            .HasMaxLength(20)
            .HasColumnName("phoneNumber");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<PreventivoExtra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("preventiviextra");

            entity.HasIndex(e => e.IdPreventivo, "idPreventivoRef_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Costo).HasColumnName("costo");
            entity.Property(e => e.Description)
                .HasColumnType("mediumtext")
                .HasColumnName("description");
            entity.Property(e => e.IdPreventivo).HasColumnName("idPreventivo");

            entity.HasOne(d => d.IdPreventivoNavigation).WithMany(p => p.PreventivoExtra)
                .HasForeignKey(d => d.IdPreventivo)
                .HasConstraintName("idPreventivoRef");
        });

        modelBuilder.Entity<PreventivoNCC>(entity =>
        {
            entity.HasKey(e => e.IdPreventivo).HasName("PRIMARY");

            entity.ToTable("preventivincc");

            entity.HasIndex(e => e.IdUserNonAutenticato, "idUserNonAutenticato_idx");

            entity.HasIndex(e => e.IdUser, "idUser_idx");

            entity.Property(e => e.IdPreventivo).HasColumnName("idPreventivo");
            entity.Property(e => e.Arrivo)
                .HasMaxLength(100)
                .HasColumnName("arrivo");
            entity.Property(e => e.Costo).HasColumnName("costo");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.IdUserNonAutenticato).HasColumnName("idUserNonAutenticato");
            entity.Property(e => e.IsTodo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_todo");
            entity.Property(e => e.Partenza)
                .HasMaxLength(100)
                .HasColumnName("partenza");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.PreventiviNcc)
                .HasPrincipalKey(p => p.IdPerson)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUserRef");

            entity.HasOne(d => d.IdUserNonAutenticatoNavigation).WithMany(p => p.PreventiviNcc)
                .HasPrincipalKey(p => p.IdPerson)
                .HasForeignKey(d => d.IdUserNonAutenticato)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUserNonAutenticatoRef");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdPerson, "idPerson_UNIQUE").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.IdPersonNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.IdPerson)
                .HasConstraintName("idPersonRef");
        });

        modelBuilder.Entity<UserNotAuthenticated>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PRIMARY");

            entity.ToTable("usersnotauthenticated");

            entity.HasIndex(e => e.IdPerson, "ipPerson_UNIQUE").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasColumnName("ip");

            entity.HasOne(d => d.IdPersonNavigation).WithOne(p => p.UserNotAuthenticated)
                .HasForeignKey<UserNotAuthenticated>(d => d.IdPerson)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("idPersonNotAuthenticatedRef");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refreshtokens");

            entity.HasIndex(e => e.Token, "token_idx").IsUnique();
            entity.HasIndex(e => e.UserId, "userId_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expiresAt");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsRevoked).HasColumnName("isRevoked");
            entity.Property(e => e.RevokedByIp)
                .HasMaxLength(50)
                .HasColumnName("revokedByIp");
            entity.Property(e => e.RevokedAt)
                .HasColumnType("datetime")
                .HasColumnName("revokedAt");

            entity.HasOne(d => d.User).WithMany()
                .HasPrincipalKey(p => p.IdPerson)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("userId");
        });
    }

}