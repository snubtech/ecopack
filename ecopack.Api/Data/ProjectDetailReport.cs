using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecopack.Api.Data
{
    [Table("project_detail_report")]
    public partial class ProjectDetailReport
    {
        /// <summary>
        /// 고유 일련번호 (PK)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 프로젝트 고유 ID
        /// </summary>
        [Column("prjId")]
        [StringLength(50)]
        public string? PrjId { get; set; }

        /// <summary>
        /// 포장차수
        /// </summary>
        [Column("packLevel")]
        [StringLength(50)]
        public string? PackLevel { get; set; }

        [Column("prjuserid")]
        [StringLength(50)]
        public string? Prjuserid { get; set; }

        /// <summary>
        /// 포장차수명
        /// </summary>
        [Column("packLevelNm")]
        [StringLength(100)]
        public string? PackLevelNm { get; set; }

        /// <summary>
        /// 적용소재
        /// </summary>
        [Column("appliedMaterial")]
        [StringLength(50)]
        public string? AppliedMaterial { get; set; }

        /// <summary>
        /// 적용소재명
        /// </summary>
        [Column("appliedMaterialNm")]
        [StringLength(100)]
        public string? AppliedMaterialNm { get; set; }

        /// <summary>
        /// 수출국
        /// </summary>
        [Column("prdExpCntry")]
        [StringLength(50)]
        public string? PrdExpCntry { get; set; }

        /// <summary>
        /// 수출국 명
        /// </summary>
        [Column("prdExpCntryNm")]
        [StringLength(50)]
        public string? PrdExpCntryNm { get; set; }

        /// <summary>
        /// 포장재 구분
        /// </summary>
        [Column("matType")]
        [StringLength(50)]
        public string? MatType { get; set; }

        /// <summary>
        /// 포장재 구분명
        /// </summary>
        [Column("matTypeNm")]
        [StringLength(100)]
        public string? MatTypeNm { get; set; }

        /// <summary>
        /// 소재의 구성
        /// </summary>
        [Column("matForm")]
        [StringLength(50)]
        public string? MatForm { get; set; }

        /// <summary>
        /// 소재의 구성명
        /// </summary>
        [Column("matFormNm")]
        [StringLength(100)]
        public string? MatFormNm { get; set; }

        /// <summary>
        /// 성능항목
        /// </summary>
        [Column("item")]
        [StringLength(50)]
        public string? Item { get; set; }

        /// <summary>
        /// 성능항목명
        /// </summary>
        [Column("itemNm")]
        [StringLength(100)]
        public string? ItemNm { get; set; }

        /// <summary>
        /// 단위
        /// </summary>
        [Column("unit")]
        [StringLength(50)]
        public string? Unit { get; set; }

        /// <summary>
        /// 단위명
        /// </summary>
        [Column("unitNm")]
        [StringLength(50)]
        public string? UnitNm { get; set; }

        /// <summary>
        /// 기준값 범위
        /// </summary>
        [Column("acceptableRange", TypeName = "text")]
        public string? AcceptableRange { get; set; }

        /// <summary>
        /// 관련규정
        /// </summary>
        [Column("relatedReg")]
        [StringLength(255)]
        public string? RelatedReg { get; set; }

        /// <summary>
        /// 규제항목
        /// </summary>
        [Column("regItem", TypeName = "text")]
        public string? RegItem { get; set; }

        /// <summary>
        /// 규제내용
        /// </summary>
        [Column("dtlCont", TypeName = "text")]
        public string? DtlCont { get; set; }

        /// <summary>
        /// 디자인소재
        /// </summary>
        [Column("matComp")]
        [StringLength(50)]
        public string? MatComp { get; set; }

        /// <summary>
        /// 디자인소재명
        /// </summary>
        [Column("matCompNm")]
        [StringLength(100)]
        public string? MatCompNm { get; set; }

        /// <summary>
        /// 설명이미지 (memoImg)
        /// </summary>
        [Column("memoImg", TypeName = "longtext")]
        public string? MemoImg { get; set; }

        /// <summary>
        /// 파일데이터 (fileData)
        /// </summary>
        [Column("fileData", TypeName = "longtext")]
        public string? FileData { get; set; }

        /// <summary>
        /// 중량 당 탄소배출량(소재)
        /// </summary>
        [Column("massCo2Mat")]
        [StringLength(50)]
        public string? MassCo2Mat { get; set; }

        /// <summary>
        /// 중량 당 탄소배출량(공정)
        /// </summary>
        [Column("massCo2Proc")]
        [StringLength(50)]
        public string? MassCo2Proc { get; set; }

        /// <summary>
        /// 중량 당 탄소배출량(스크랩)
        /// </summary>
        [Column("massCo2Scrap")]
        [StringLength(50)]
        public string? MassCo2Scrap { get; set; }

        /// <summary>
        /// 중량 당 탄소배출량(합계)
        /// </summary>
        [Column("massCo2Sum")]
        [StringLength(50)]
        public string? MassCo2Sum { get; set; }

        /// <summary>
        /// 단위당 탄소배출량(소재)
        /// </summary>
        [Column("unitCo2Mat")]
        [StringLength(50)]
        public string? UnitCo2Mat { get; set; }

        /// <summary>
        /// 단위당 탄소배출량(공정)
        /// </summary>
        [Column("unitCo2Proc")]
        [StringLength(50)]
        public string? UnitCo2Proc { get; set; }

        /// <summary>
        /// 단위당 탄소배출량(스크랩)
        /// </summary>
        [Column("unitCo2Scrap")]
        [StringLength(50)]
        public string? UnitCo2Scrap { get; set; }

        /// <summary>
        /// 단위당 탄소배출량(합계)
        /// </summary>
        [Column("unitCo2Sum")]
        [StringLength(50)]
        public string? UnitCo2Sum { get; set; }

        /// <summary>
        /// 저장수정시간
        /// </summary>
        [Column("updatedate", TypeName = "datetime")]
        public DateTime? Updatedate { get; set; }
    }
}