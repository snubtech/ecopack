using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class ProjectDetailReportConfiguration : IEntityTypeConfiguration<ProjectDetailReport>
    {
        public void Configure(EntityTypeBuilder<ProjectDetailReport> builder)
        {
            builder.ToTable("project_detail_report");

            // ★ 단일 기본키(id) 설정 및 자동 증가(Auto Increment) 지정
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packLevel");
            builder.Property(e => e.Prjuserid).HasMaxLength(50).HasColumnName("prjuserid");
            builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packLevelNm");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedMaterial");
            builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedMaterialNm");
            builder.Property(e => e.PrdExpCntry).HasMaxLength(50).HasColumnName("prdExpCntry");
            builder.Property(e => e.PrdExpCntryNm).HasMaxLength(50).HasColumnName("prdExpCntryNm");
            builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("matType");
            builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("matTypeNm");
            builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matForm");
            builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matFormNm");
            builder.Property(e => e.Item).HasMaxLength(50).HasColumnName("item");
            builder.Property(e => e.ItemNm).HasMaxLength(100).HasColumnName("itemNm");
            builder.Property(e => e.Unit).HasMaxLength(50).HasColumnName("unit");
            builder.Property(e => e.UnitNm).HasMaxLength(50).HasColumnName("unitNm");
            builder.Property(e => e.AcceptableRange).HasColumnType("text").HasColumnName("acceptableRange");
            builder.Property(e => e.RelatedReg).HasMaxLength(255).HasColumnName("relatedReg");
            builder.Property(e => e.RegItem).HasColumnType("text").HasColumnName("regItem");
            builder.Property(e => e.DtlCont).HasColumnType("text").HasColumnName("dtlCont");
            builder.Property(e => e.MatComp).HasMaxLength(50).HasColumnName("matComp");
            builder.Property(e => e.MatCompNm).HasMaxLength(100).HasColumnName("matCompNm");
            builder.Property(e => e.MemoImg).HasColumnType("longtext").HasColumnName("memoImg");
            builder.Property(e => e.FileData).HasColumnType("longtext").HasColumnName("fileData");
            builder.Property(e => e.MassCo2Mat).HasMaxLength(50).HasColumnName("massCo2Mat");
            builder.Property(e => e.MassCo2Proc).HasMaxLength(50).HasColumnName("massCo2Proc");
            builder.Property(e => e.MassCo2Scrap).HasMaxLength(50).HasColumnName("massCo2Scrap");
            builder.Property(e => e.MassCo2Sum).HasMaxLength(50).HasColumnName("massCo2Sum");
            builder.Property(e => e.UnitCo2Mat).HasMaxLength(50).HasColumnName("unitCo2Mat");
            builder.Property(e => e.UnitCo2Proc).HasMaxLength(50).HasColumnName("unitCo2Proc");
            builder.Property(e => e.UnitCo2Scrap).HasMaxLength(50).HasColumnName("unitCo2Scrap");
            builder.Property(e => e.UnitCo2Sum).HasMaxLength(50).HasColumnName("unitCo2Sum");
            builder.Property(e => e.Updatedate).HasColumnType("datetime").HasColumnName("updatedate");
        }
    }
}