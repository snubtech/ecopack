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

        // ─────────────────────────────────────────────────────────────
        // 저장 전 길이 검사
        // 글자 수가 컬럼 길이를 넘으면 DB 가 오류를 내거나(엄격 모드) 말없이 잘라 저장한다(비엄격 모드).
        // 어느 쪽이든 원인을 알기 어려우므로, 저장하기 전에 여기서 걸러 "어느 항목이 몇 자를 넘었는지" 알려 준다.
        // 기준은 코드에 적힌 길이가 아니라 실제 DB 컬럼 길이다(둘이 다른 컬럼이 있어서 DB 에서 직접 읽어 온다).
        // ─────────────────────────────────────────────────────────────

        /// <summary>"테이블|컬럼" → (최대 길이, 바이트 기준 여부). 처음 저장할 때 한 번 읽어 서버가 떠 있는 동안 쓴다.</summary>
        private static volatile Dictionary<string, (long Max, bool InBytes)>? _columnLimits;

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            _columnLimits ??= LoadColumnLimitsAsync(sync: true, CancellationToken.None).GetAwaiter().GetResult();
            ValidateLengths();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            _columnLimits ??= await LoadColumnLimitsAsync(sync: false, cancellationToken);
            ValidateLengths();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ValidateLengths()
        {
            var limits = _columnLimits;
            if (limits == null || limits.Count == 0)
            {
                return; // 컬럼 길이를 읽지 못했으면 검사하지 않고 DB 에 맡긴다
            }

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                {
                    continue;
                }

                var table = entry.Metadata.GetTableName();
                if (table == null)
                {
                    continue;
                }
                var storeObject = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(table, entry.Metadata.GetSchema());

                foreach (var prop in entry.Properties)
                {
                    if (prop.CurrentValue is not string value || value.Length == 0)
                    {
                        continue;
                    }
                    if (entry.State == EntityState.Modified && !prop.IsModified)
                    {
                        continue; // 이번에 바꾸지 않은 값은 보지 않는다(예전에 들어간 값 때문에 저장이 막히지 않게)
                    }

                    var column = prop.Metadata.GetColumnName(storeObject);
                    if (column == null || !limits.TryGetValue(LimitKey(table, column), out var limit))
                    {
                        continue;
                    }

                    if (limit.InBytes)
                    {
                        // text 계열은 바이트 기준이다. 한글은 글자당 3바이트.
                        if ((long)value.Length * 4 > limit.Max && System.Text.Encoding.UTF8.GetByteCount(value) > limit.Max)
                        {
                            throw new Support.InputValidationException($"입력한 내용이 너무 깁니다. ({column})");
                        }
                    }
                    else if (value.Length > limit.Max)
                    {
                        throw new Support.InputValidationException($"입력한 값이 너무 깁니다. {column} 항목은 최대 {limit.Max}자입니다.");
                    }
                }
            }
        }

        private static string LimitKey(string table, string column) => $"{table}|{column}".ToLowerInvariant();

        /// <summary>실제 DB 에서 글자 컬럼의 최대 길이를 읽어 온다. 읽지 못하면 null(다음 저장 때 다시 시도).</summary>
        private async Task<Dictionary<string, (long Max, bool InBytes)>?> LoadColumnLimitsAsync(bool sync, CancellationToken ct)
        {
            try
            {
                var conn = Database.GetDbConnection();
                var opened = false;
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    if (sync) Database.OpenConnection(); else await Database.OpenConnectionAsync(ct);
                    opened = true;
                }

                try
                {
                    using var cmd = conn.CreateCommand();
                    if (Database.CurrentTransaction != null)
                    {
                        // 트랜잭션 안에서 불렸으면 같은 트랜잭션으로 읽어야 한다
                        cmd.Transaction = Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction(Database.CurrentTransaction);
                    }
                    cmd.CommandText =
                        "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                        "FROM information_schema.COLUMNS " +
                        "WHERE TABLE_SCHEMA = DATABASE() AND CHARACTER_MAXIMUM_LENGTH IS NOT NULL";

                    var result = new Dictionary<string, (long, bool)>();
                    using var reader = sync ? cmd.ExecuteReader() : await cmd.ExecuteReaderAsync(ct);
                    while (sync ? reader.Read() : await reader.ReadAsync(ct))
                    {
                        var type = reader.GetString(2).ToLowerInvariant();
                        var inBytes = type.EndsWith("text") || type.EndsWith("blob");
                        result[LimitKey(reader.GetString(0), reader.GetString(1))] = (Convert.ToInt64(reader.GetValue(3)), inBytes);
                    }
                    return result;
                }
                finally
                {
                    if (opened)
                    {
                        if (sync) Database.CloseConnection(); else await Database.CloseConnectionAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");
            modelBuilder.UseCollation("utf8mb4_unicode_ci").HasCharSet("utf8mb4");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}