using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class PrimaryPkgConfiguration : IEntityTypeConfiguration<PrimaryPkg>
    {
        public void Configure(EntityTypeBuilder<PrimaryPkg> builder)
        {
            builder.ToTable("primarypkg");
            builder.HasKey(e => e.SubPrjId);

            builder.Property(e => e.SubPrjId).HasMaxLength(50).HasColumnName("subprjid");
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjid");
            builder.Property(e => e.ApplMatNm).HasMaxLength(100).HasColumnName("applmatnm");
            builder.Property(e => e.UseEnvCntn).HasColumnType("text").HasColumnName("useenvcntn");
            builder.Property(e => e.PkgMatTypeNm).HasMaxLength(100).HasColumnName("pkgmattypenm");
        }
    }
}