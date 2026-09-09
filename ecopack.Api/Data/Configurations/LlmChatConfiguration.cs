using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecopack.Api.Data.Configurations
{
    /// <summary>llm_chat_session — AI채팅세션기본</summary>
    public class LlmChatSessionConfiguration : IEntityTypeConfiguration<LlmChatSession>
    {
        public void Configure(EntityTypeBuilder<LlmChatSession> builder)
        {
            builder.ToTable("llm_chat_session");
            builder.HasKey(e => e.ChatSesId);

            builder.Property(e => e.ChatSesId).HasMaxLength(50).HasColumnName("chatSesId");
            builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repCustId").IsRequired();
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(10).HasColumnName("packLevel");
            builder.Property(e => e.ChatSesNm).HasMaxLength(200).HasColumnName("chatSesNm");
            builder.Property(e => e.ChatSesStatCd).HasMaxLength(20).HasColumnName("chatSesStatCd").IsRequired();
            builder.Property(e => e.ChatMsgCnt).HasColumnName("chatMsgCnt");
            builder.Property(e => e.LgnDtm).HasColumnType("datetime").HasColumnName("lgnDtm");
            builder.Property(e => e.LastMsgDtm).HasColumnType("datetime").HasColumnName("lastMsgDtm");
            builder.Property(e => e.FrstCrtDtm).HasColumnType("datetime").HasColumnName("frstCrtDtm");
            builder.Property(e => e.LastUpdDtm).HasColumnType("datetime").HasColumnName("lastUpdDtm");
        }
    }

    /// <summary>llm_chat_msg — AI채팅메시지기본</summary>
    public class LlmChatMsgConfiguration : IEntityTypeConfiguration<LlmChatMsg>
    {
        public void Configure(EntityTypeBuilder<LlmChatMsg> builder)
        {
            builder.ToTable("llm_chat_msg");
            builder.HasKey(e => e.ChatMsgId);

            builder.Property(e => e.ChatMsgId).HasColumnName("chatMsgId");
            builder.Property(e => e.ChatSesId).HasMaxLength(50).HasColumnName("chatSesId").IsRequired();
            builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repCustId").IsRequired();
            builder.Property(e => e.ChatMsgSeq).HasColumnName("chatMsgSeq");
            builder.Property(e => e.ChatRoleCd).HasMaxLength(20).HasColumnName("chatRoleCd").IsRequired();
            builder.Property(e => e.ChatMsgCntn).HasColumnType("mediumtext").HasColumnName("chatMsgCntn");
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(10).HasColumnName("packLevel");
            builder.Property(e => e.CurMenuId).HasMaxLength(50).HasColumnName("curMenuId");
            builder.Property(e => e.WrkStatSnpsCntn).HasColumnType("mediumtext").HasColumnName("wrkStatSnpsCntn");
            builder.Property(e => e.AiModelNm).HasMaxLength(60).HasColumnName("aiModelNm");
            builder.Property(e => e.InTknCnt).HasColumnName("inTknCnt");
            builder.Property(e => e.OutTknCnt).HasColumnName("outTknCnt");
            builder.Property(e => e.ElpsMsVal).HasColumnName("elpsMsVal");
            builder.Property(e => e.ErrCntn).HasColumnType("text").HasColumnName("errCntn");
            builder.Property(e => e.FrstCrtDtm).HasColumnType("datetime").HasColumnName("frstCrtDtm");
        }
    }

    /// <summary>llm_chat_session_arch — AI채팅세션백업</summary>
    public class LlmChatSessionArchConfiguration : IEntityTypeConfiguration<LlmChatSessionArch>
    {
        public void Configure(EntityTypeBuilder<LlmChatSessionArch> builder)
        {
            builder.ToTable("llm_chat_session_arch");
            builder.HasKey(e => e.ChatSesId);

            builder.Property(e => e.ChatSesId).HasMaxLength(50).HasColumnName("chatSesId");
            builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repCustId").IsRequired();
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(10).HasColumnName("packLevel");
            builder.Property(e => e.ChatSesNm).HasMaxLength(200).HasColumnName("chatSesNm");
            builder.Property(e => e.ChatSesStatCd).HasMaxLength(20).HasColumnName("chatSesStatCd").IsRequired();
            builder.Property(e => e.ChatMsgCnt).HasColumnName("chatMsgCnt");
            builder.Property(e => e.LgnDtm).HasColumnType("datetime").HasColumnName("lgnDtm");
            builder.Property(e => e.LastMsgDtm).HasColumnType("datetime").HasColumnName("lastMsgDtm");
            builder.Property(e => e.FrstCrtDtm).HasColumnType("datetime").HasColumnName("frstCrtDtm");
            builder.Property(e => e.LastUpdDtm).HasColumnType("datetime").HasColumnName("lastUpdDtm");
            builder.Property(e => e.ArchDtm).HasColumnType("datetime").HasColumnName("archDtm");
            builder.Property(e => e.ArchFileUrl).HasMaxLength(400).HasColumnName("archFileUrl");
        }
    }

    /// <summary>llm_chat_msg_arch — AI채팅메시지백업</summary>
    public class LlmChatMsgArchConfiguration : IEntityTypeConfiguration<LlmChatMsgArch>
    {
        public void Configure(EntityTypeBuilder<LlmChatMsgArch> builder)
        {
            builder.ToTable("llm_chat_msg_arch");
            builder.HasKey(e => e.ArchChatMsgId);

            builder.Property(e => e.ArchChatMsgId).HasColumnName("archChatMsgId");
            builder.Property(e => e.ChatMsgId).HasColumnName("chatMsgId");
            builder.Property(e => e.ChatSesId).HasMaxLength(50).HasColumnName("chatSesId").IsRequired();
            builder.Property(e => e.RepCustId).HasMaxLength(50).HasColumnName("repCustId").IsRequired();
            builder.Property(e => e.ChatMsgSeq).HasColumnName("chatMsgSeq");
            builder.Property(e => e.ChatRoleCd).HasMaxLength(20).HasColumnName("chatRoleCd").IsRequired();
            builder.Property(e => e.ChatMsgCntn).HasColumnType("mediumtext").HasColumnName("chatMsgCntn");
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjId");
            builder.Property(e => e.PackLevel).HasMaxLength(10).HasColumnName("packLevel");
            builder.Property(e => e.CurMenuId).HasMaxLength(50).HasColumnName("curMenuId");
            builder.Property(e => e.WrkStatSnpsCntn).HasColumnType("mediumtext").HasColumnName("wrkStatSnpsCntn");
            builder.Property(e => e.AiModelNm).HasMaxLength(60).HasColumnName("aiModelNm");
            builder.Property(e => e.InTknCnt).HasColumnName("inTknCnt");
            builder.Property(e => e.OutTknCnt).HasColumnName("outTknCnt");
            builder.Property(e => e.ElpsMsVal).HasColumnName("elpsMsVal");
            builder.Property(e => e.ErrCntn).HasColumnType("text").HasColumnName("errCntn");
            builder.Property(e => e.FrstCrtDtm).HasColumnType("datetime").HasColumnName("frstCrtDtm");
            builder.Property(e => e.ArchDtm).HasColumnType("datetime").HasColumnName("archDtm");
        }
    }
}
