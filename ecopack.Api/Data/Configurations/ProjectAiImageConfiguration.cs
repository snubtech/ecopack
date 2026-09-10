using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class ProjectAiImageConfiguration : IEntityTypeConfiguration<ProjectAiImage>
    {
        public void Configure(EntityTypeBuilder<ProjectAiImage> builder)
        {
            builder.ToTable("project_aiimage");

            // 복합 기본키 설정 (prjId + packLevel)
            builder.HasKey(e => new { e.PrjId, e.PackLevel });

            // 프로젝트 및 사용자 식별 정보
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packLevel");
            builder.Property(e => e.Prjuserid).HasMaxLength(50).HasColumnName("prjuserid");

            // 기본 식별자 (2D 작업 기준)
            builder.Property(e => e.JobId).HasMaxLength(100).HasColumnName("job_id");
            builder.Property(e => e.RequestId).HasMaxLength(100).HasColumnName("request_id");

            // 1. 2D 이미지 요청 파라미터
            builder.Property(e => e.Prompt).HasColumnType("TEXT").HasColumnName("prompt");
            builder.Property(e => e.EcoFix).HasColumnType("TEXT").HasColumnName("eco_fix");
            builder.Property(e => e.Material).HasMaxLength(50).HasColumnName("material");
            builder.Property(e => e.InputImage).HasColumnType("LONGTEXT").HasColumnName("input_image");

            // 2. 2D 작업 상태 및 진행 관리
            builder.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("QUEUED").HasColumnName("status");
            builder.Property(e => e.Progress).HasDefaultValue(0).HasColumnName("progress");
            builder.Property(e => e.Success).HasDefaultValue(true).HasColumnName("success");
            builder.Property(e => e.StatusMessage).HasColumnType("TEXT").HasColumnName("status_message");
            builder.Property(e => e.ErrorMessage).HasColumnType("TEXT").HasColumnName("error_message");

            // 3. 2D 결과 이미지 데이터 영구 보관 (BASE64)
            builder.Property(e => e.ResultDataOriginal).HasColumnType("LONGTEXT").HasColumnName("result_data_original");
            builder.Property(e => e.ResultDataModerate).HasColumnType("LONGTEXT").HasColumnName("result_data_moderate");
            builder.Property(e => e.ResultDataRedesign).HasColumnType("LONGTEXT").HasColumnName("result_data_redesign");

            // 4. 3D GLB 작업 요청 및 폴링 상태 관리
            builder.Property(e => e.GlbJobId).HasMaxLength(100).HasColumnName("glb_job_id");
            builder.Property(e => e.RequestIdGlb).HasMaxLength(100).HasColumnName("request_id_glb");
            builder.Property(e => e.SourceImageId).HasMaxLength(100).HasColumnName("source_image_id");
            builder.Property(e => e.Status3d).HasMaxLength(30).HasDefaultValue("NONE").HasColumnName("status_3d");
            builder.Property(e => e.Progress3d).HasDefaultValue(0).HasColumnName("progress_3d");
            builder.Property(e => e.Success3d).HasDefaultValue(true).HasColumnName("success_3d");
            builder.Property(e => e.StatusMessage3d).HasColumnType("TEXT").HasColumnName("status_message_3d");
            builder.Property(e => e.ErrorMessage3d).HasColumnType("TEXT").HasColumnName("error_message_3d");

            // 5. 3D GLB 결과 바이너리 파일 데이터 영구 보관 (BASE64)
            builder.Property(e => e.ResultDataGlbOriginal).HasColumnType("LONGTEXT").HasColumnName("result_data_glb_original");
            builder.Property(e => e.ResultDataGlbModerate).HasColumnType("LONGTEXT").HasColumnName("result_data_glb_moderate");
            builder.Property(e => e.ResultDataGlbRedesign).HasColumnType("LONGTEXT").HasColumnName("result_data_glb_redesign");

            // 타임스탬프
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        }
    }
}