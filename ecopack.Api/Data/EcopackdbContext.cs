using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ecopack.Api.Data;

public partial class EcopackdbContext : DbContext
{
    public EcopackdbContext()
    {
    }

    public EcopackdbContext(DbContextOptions<EcopackdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ProjectDetail> ProjectDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=192.168.1.222;database=ecopackdb;uid=root;pwd=snub3120", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.46-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ProjectDetail>(entity =>
        {
            entity.HasKey(e => new { e.PrjId, e.PackLevel })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("project_detail");

            entity.HasIndex(e => new { e.PrjId, e.PackLevel }, "project_detail_prjId_IDX");

            entity.Property(e => e.PrjId)
                .HasMaxLength(50)
                .HasComment("프로젝트 고유 ID")
                .HasColumnName("prjId");
            entity.Property(e => e.PackLevel)
                .HasMaxLength(50)
                .HasComment("포장차수")
                .HasColumnName("packLevel")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.AppliedMaterial)
                .HasMaxLength(50)
                .HasComment("적용소재")
                .HasColumnName("appliedMaterial")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.AppliedMaterialNm)
                .HasMaxLength(100)
                .HasComment("적용소재명")
                .HasColumnName("appliedMaterialNm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatForm)
                .HasMaxLength(50)
                .HasComment("소재의 구성")
                .HasColumnName("matForm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatFormNm)
                .HasMaxLength(100)
                .HasComment("소재의 구성명")
                .HasColumnName("matFormNm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatType)
                .HasMaxLength(50)
                .HasComment("포장재 구분")
                .HasColumnName("matType")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatTypeNm)
                .HasMaxLength(100)
                .HasComment("포장재 구분명")
                .HasColumnName("matTypeNm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatUse)
                .HasMaxLength(50)
                .HasComment("사용환경")
                .HasColumnName("matUse")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.MatUseNm)
                .HasMaxLength(100)
                .HasComment("사용환경명")
                .HasColumnName("matUseNm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.PackDsgnTplId)
                .HasMaxLength(50)
                .HasComment("패키징디자인템플릿 ID")
                .HasColumnName("packDsgnTplId");
            entity.Property(e => e.PackLevelNm)
                .HasMaxLength(100)
                .HasComment("포장차수명")
                .HasColumnName("packLevelNm")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.PrdExpCntry)
                .HasMaxLength(50)
                .HasColumnName("prdExpCntry");
            entity.Property(e => e.PrdExpCntryNm)
                .HasMaxLength(50)
                .HasColumnName("prdExpCntryNm");
            entity.Property(e => e.PrjRevNo).HasMaxLength(10);
            entity.Property(e => e.Projstatus)
                .HasMaxLength(30)
                .HasComment("프로젝트 진행단계")
                .HasColumnName("projstatus")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Updatedate)
                .HasComment("저장수정시간")
                .HasColumnType("datetime")
                .HasColumnName("updatedate");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
