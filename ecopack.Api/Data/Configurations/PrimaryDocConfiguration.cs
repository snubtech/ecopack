/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrimaryDocConfiguration (primary_doc 테이블 매핑)
 * ==============================================================================
 * 
 * 1. 하는 일
 *    - 엔티티 PrimaryDoc 을 실제 테이블 primary_doc 에 이어 줍니다.
 *    - 실제 DB의 SHOW CREATE TABLE 결과를 그대로 옮겨 적었습니다(2026-09).
 * 
 * 2. 처음 만들어졌을 때 잘못돼 있던 점
 *    - primary_td 와 같은 문제였습니다. 테이블명이 primarydoc 이고 컬럼명이 전부 소문자였습니다.
 * 
 * 3. 맞춘 내용
 *    - 테이블명 primary_doc, 컬럼명은 실제와 같은 대소문자(pkg1DocId, soCHvyMetLmtCmplCntn1 …)
 *    - 문자열은 varchar 이며 길이를 실제 컬럼과 맞췄습니다.
 *      길이 제한이 있으므로 화면에서도 같은 길이로 입력을 제한합니다.
 *    - sbstTot 는 DB 기본값 '총합', lastWrtDt 는 date 타입에 기본값 (curdate()) 을 따릅니다.
 * 
 * 4. 참고
 *    - 컬럼별 한글 설명은 엔티티 PrimaryDoc.cs 의 주석에 적혀 있습니다.
 * ==============================================================================
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    /// <summary>
    /// primary_doc (1차포장적합성선언서기본) 테이블 매핑.
    /// 실제 DB의 SHOW CREATE TABLE 결과에 맞춰 작성됨 (2026-09):
    ///  - 테이블명: primary_doc (언더스코어)
    ///  - PK: pkg1DocId varchar(50)
    ///  - 문자열 컬럼은 전부 varchar(길이 지정)
    ///  - sbstTot 은 DB DEFAULT '총합'
    ///  - lastWrtDt 는 date 타입, DB DEFAULT (curdate())
    /// 컬럼 정의(한글 설명)는 엔티티 PrimaryDoc.cs 의 XML 주석 참고.
    /// </summary>
    public class PrimaryDocConfiguration : IEntityTypeConfiguration<PrimaryDoc>
    {
        public void Configure(EntityTypeBuilder<PrimaryDoc> builder)
        {
            builder.ToTable("primary_doc");
            builder.HasKey(e => e.Pkg1DocId);

            builder.Property(e => e.Pkg1DocId).HasColumnName("pkg1DocId").HasMaxLength(50);
            builder.Property(e => e.BizNm).HasColumnName("bizNm").HasMaxLength(100);
            builder.Property(e => e.RepNm).HasColumnName("repNm").HasMaxLength(50);
            builder.Property(e => e.RoleNm).HasColumnName("roleNm").HasMaxLength(30);
            builder.Property(e => e.EmlAddr).HasColumnName("emlAddr").HasMaxLength(100);
            builder.Property(e => e.MbTelNo).HasColumnName("mbTelNo").HasMaxLength(20);
            builder.Property(e => e.PrjfNm).HasColumnName("prjfNm").HasMaxLength(100);
            builder.Property(e => e.PrjId).HasColumnName("prjId").HasMaxLength(50);
            builder.Property(e => e.Pkg1TechDocId).HasColumnName("pkg1TechDocId").HasMaxLength(50);
            builder.Property(e => e.RevNo).HasColumnName("revNo").HasMaxLength(14);
            builder.Property(e => e.CntryNm).HasColumnName("cntryNm").HasMaxLength(50);
            builder.Property(e => e.DsgnTypeNm).HasColumnName("dsgnTypeNm").HasMaxLength(300);
            builder.Property(e => e.DocPhrsCntn).HasColumnName("docPhrsCntn").HasMaxLength(500);
            builder.Property(e => e.ReuseReqCmplCntn).HasColumnName("reuseReqCmplCntn").HasMaxLength(300);
            builder.Property(e => e.DsgnTmplMstrPrdExpl).HasColumnName("dsgnTmplMstrPrdExpl").HasMaxLength(300);
            builder.Property(e => e.RcycReqCmplCntn1).HasColumnName("rcycReqCmplCntn1").HasMaxLength(100);
            builder.Property(e => e.RcycMainFeatCntn).HasColumnName("rcycMainFeatCntn").HasMaxLength(200);
            builder.Property(e => e.RcycReqCmplCntn2).HasColumnName("rcycReqCmplCntn2").HasMaxLength(100);
            builder.Property(e => e.SoCHvyMetLmtCmplCntn1).HasColumnName("soCHvyMetLmtCmplCntn1").HasMaxLength(100);
            builder.Property(e => e.SoCHvyMetLmtCmplCntn2).HasColumnName("soCHvyMetLmtCmplCntn2").HasMaxLength(300);
            builder.Property(e => e.Sbst1).HasColumnName("sbst1").HasMaxLength(50);
            builder.Property(e => e.Sbst2).HasColumnName("sbst2").HasMaxLength(50);
            builder.Property(e => e.Sbst3).HasColumnName("sbst3").HasMaxLength(50);
            builder.Property(e => e.Sbst4).HasColumnName("sbst4").HasMaxLength(50);
            builder.Property(e => e.SbstTot).HasColumnName("sbstTot").HasMaxLength(50)
                   .HasDefaultValue("총합");
            builder.Property(e => e.TestRslt1).HasColumnName("testRslt1").HasMaxLength(50);
            builder.Property(e => e.TestRslt2).HasColumnName("testRslt2").HasMaxLength(50);
            builder.Property(e => e.TestRslt3).HasColumnName("testRslt3").HasMaxLength(50);
            builder.Property(e => e.TestRslt4).HasColumnName("testRslt4").HasMaxLength(50);
            builder.Property(e => e.TestRsltTot).HasColumnName("testRsltTot").HasMaxLength(50);
            builder.Property(e => e.Compltem1).HasColumnName("compltem1").HasMaxLength(50);
            builder.Property(e => e.Compltem2).HasColumnName("compltem2").HasMaxLength(50);
            builder.Property(e => e.Compltem3).HasColumnName("compltem3").HasMaxLength(50);
            builder.Property(e => e.Mat1).HasColumnName("mat1").HasMaxLength(50);
            builder.Property(e => e.Mat2).HasColumnName("mat2").HasMaxLength(50);
            builder.Property(e => e.Mat3).HasColumnName("mat3").HasMaxLength(50);
            builder.Property(e => e.MatInfoTotWtVal).HasColumnName("matInfoTotWtVal").HasMaxLength(50);
            builder.Property(e => e.MatInfoCntn).HasColumnName("matInfoCntn").HasMaxLength(100);
            builder.Property(e => e.EvdDocCntn).HasColumnName("evdDocCntn").HasMaxLength(100);
            builder.Property(e => e.EvdDocUrl1).HasColumnName("evdDocUrl1").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl2).HasColumnName("evdDocUrl2").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl3).HasColumnName("evdDocUrl3").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl4).HasColumnName("evdDocUrl4").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl5).HasColumnName("evdDocUrl5").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl6).HasColumnName("evdDocUrl6").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl7).HasColumnName("evdDocUrl7").HasMaxLength(500);
            builder.Property(e => e.EvdDocUrl8).HasColumnName("evdDocUrl8").HasMaxLength(500);
            builder.Property(e => e.EvdDocNm1).HasColumnName("evdDocNm1").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm2).HasColumnName("evdDocNm2").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm3).HasColumnName("evdDocNm3").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm4).HasColumnName("evdDocNm4").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm5).HasColumnName("evdDocNm5").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm6).HasColumnName("evdDocNm6").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm7).HasColumnName("evdDocNm7").HasMaxLength(300);
            builder.Property(e => e.EvdDocNm8).HasColumnName("evdDocNm8").HasMaxLength(300);
            builder.Property(e => e.ApplRuleStdCntn).HasColumnName("applRuleStdCntn").HasMaxLength(300);
            builder.Property(e => e.LastDclCntn).HasColumnName("lastDclCntn").HasMaxLength(200);
            builder.Property(e => e.LastWrtDt).HasColumnName("lastWrtDt").HasColumnType("date")
                   .HasDefaultValueSql("(curdate())").ValueGeneratedOnAdd();
            builder.Property(e => e.BizNm2).HasColumnName("bizNm2").HasMaxLength(100);
            builder.Property(e => e.RepNm2).HasColumnName("repNm2").HasMaxLength(50);
            builder.Property(e => e.RoleNm2).HasColumnName("roleNm2").HasMaxLength(30);
        }
    }
}
