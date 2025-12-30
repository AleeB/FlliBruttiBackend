using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace FlliBrutti.Backend.Core.Models;

public partial class FlliBruttiContext : DbContext
{
    public FlliBruttiContext()
    {
    }

    public FlliBruttiContext(DbContextOptions<FlliBruttiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Firma> Firmes { get; set; }

    public virtual DbSet<FormulaPreventivo> FormulaPreventivo { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Preventivo> Preventivi { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserNotAuthenticated> UsersNotAuthenticated { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=root2002;database=flliBrutti", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.7-mysql"));

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

            entity.Property(e => e.Idfirma).HasColumnName("idfirma");
            entity.Property(e => e.Entrata)
                .HasColumnType("datetime")
                .HasColumnName("entrata");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.Uscita)
                .HasColumnType("datetime")
                .HasColumnName("uscita");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Firme)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUser");
        });

        modelBuilder.Entity<FormulaPreventivo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("formulaPreventivo");

            entity.Property(e => e.CostoKm).HasColumnName("costo_km");
            entity.Property(e => e.PrimoAutista).HasColumnName("primo_autista");
            entity.Property(e => e.SecontoAutista).HasColumnName("seconto_autista");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.IdPerson).HasName("PRIMARY");

            entity.ToTable("people");

            entity.Property(e => e.IdPerson).HasColumnName("idPerson");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Preventivo>(entity =>
        {
            entity.HasKey(e => e.IdPreventivo).HasName("PRIMARY");

            entity.ToTable("preventivi");

            entity.HasIndex(e => e.IdUserNonAutenticato, "idUserNonAutenticato_idx");

            entity.HasIndex(e => e.IdUser, "idUser_idx");

            entity.Property(e => e.IdPreventivo).HasColumnName("idPreventivo");
            entity.Property(e => e.Costo).HasColumnName("costo");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdUser).HasColumnName("idUser");
            entity.Property(e => e.IdUserNonAutenticato).HasColumnName("idUserNonAutenticato");
            entity.Property(e => e.IsTodo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_todo");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Preventivi)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUserRef");

            entity.HasOne(d => d.IdUserNonAutenticatoNavigation).WithMany(p => p.Preventivi)
                .HasForeignKey(d => d.IdUserNonAutenticato)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("idUserNonAutenticatoRef");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdPerson).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdPerson, "idPerson_UNIQUE").IsUnique();

            entity.Property(e => e.IdPerson)
                .ValueGeneratedNever()
                .HasColumnName("idPerson");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Type)
                .HasMaxLength(15)
                .HasColumnName("type");

            entity.HasOne(d => d.IdPersonNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.IdPerson)
                .HasConstraintName("idPersonRef");
        });

        modelBuilder.Entity<UserNotAuthenticated>(entity =>
        {
            entity.HasKey(e => e.IdPerson).HasName("PRIMARY");

            entity.ToTable("usersNotAuthenticated");

            entity.HasIndex(e => e.IdPerson, "ipPerson_UNIQUE").IsUnique();

            entity.Property(e => e.IdPerson)
                .ValueGeneratedNever()
                .HasColumnName("idPerson");
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasColumnName("ip");

            entity.HasOne(d => d.IdPersonNavigation).WithOne(p => p.UserNotAuthenticated)
                .HasForeignKey<UserNotAuthenticated>(d => d.IdPerson)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("idPersonNotAuthenticatedRef");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
