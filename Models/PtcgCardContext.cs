using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PtcgSearch.Models;

public partial class PtcgCardContext : DbContext
{
    public PtcgCardContext(DbContextOptions<PtcgCardContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CardOfficialInfo> CardOfficialInfo { get; set; }

    public virtual DbSet<Rarity> Rarity { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardOfficialInfo>(entity =>
        {
            entity.HasKey(e => e.CardId);

            entity.Property(e => e.CardId).ValueGeneratedNever();

            entity.HasOne(d => d.Rarity).WithMany(p => p.CardOfficialInfo)
                .HasForeignKey(d => d.RarityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CardOfficialInfo_Rarity");
        });

        modelBuilder.Entity<Rarity>(entity =>
        {
            entity.Property(e => e.RarityId).ValueGeneratedNever();
            entity.Property(e => e.RarityName)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
