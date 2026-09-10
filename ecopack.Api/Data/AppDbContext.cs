using Microsoft.EntityFrameworkCore;
using ecopack.Api.Dtos;

namespace ecopack.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ★ Product 테이블 통로 추가
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ecouser> Ecousers => Set<Ecouser>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<If001> If001 => Set<If001>();
        public DbSet<If002> If002 => Set<If002>();
        public DbSet<If002a> If002a => Set<If002a>();
        public DbSet<If003> If003 => Set<If003>();
        public DbSet<If003a> If003a => Set<If003a>();
        public DbSet<If004> If004 => Set<If004>();
        public DbSet<If005> If005 => Set<If005>();
        public DbSet<If100> If100 => Set<If100>();
        public DbSet<If200> If200s => Set<If200>();
        public DbSet<AiPkgEvalInfoBsc> AiPkgEvalInfoBscs => Set<AiPkgEvalInfoBsc>();

        public DbSet<PrimaryDoc> PrimaryDoc => Set<PrimaryDoc>();
        public DbSet<PrimaryPkg> PrimaryPkg => Set<PrimaryPkg>();
        public DbSet<PrimaryTd> PrimaryTd => Set<PrimaryTd>();

        // 2차 / 3차 포장 문서. 1차(primary_*)와 구조가 같고 문서 ID 컬럼만 차수별로 다르다.
        public DbSet<SecondaryTd> SecondaryTd => Set<SecondaryTd>();
        public DbSet<SecondaryDoc> SecondaryDoc => Set<SecondaryDoc>();
        public DbSet<TertiaryTd> TertiaryTd => Set<TertiaryTd>();
        public DbSet<TertiaryDoc> TertiaryDoc => Set<TertiaryDoc>();
        public DbSet<Project> Project => Set<Project>();
        public DbSet<ProjectDetail> ProjectDetail => Set<ProjectDetail>();
        public DbSet<ProjectDetailReport> ProjectDetailReport => Set<ProjectDetailReport>();
        public DbSet<ProjectAiImage> ProjectAiImages => Set<ProjectAiImage>();

        // 우측 LLM 채팅창(AI 어시스턴트). 세션/메시지와 2일 지난 대화의 백업본.
        public DbSet<LlmChatSession> LlmChatSession => Set<LlmChatSession>();
        public DbSet<LlmChatMsg> LlmChatMsg => Set<LlmChatMsg>();
        public DbSet<LlmChatSessionArch> LlmChatSessionArch => Set<LlmChatSessionArch>();
        public DbSet<LlmChatMsgArch> LlmChatMsgArch => Set<LlmChatMsgArch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");
            modelBuilder.UseCollation("utf8mb4_unicode_ci").HasCharSet("utf8mb4");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}