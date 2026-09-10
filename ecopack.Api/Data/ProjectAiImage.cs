using System;
using System.Collections.Generic;

namespace ecopack.Api.Data
{
    /// <summary>
    /// 프로젝트별 2D AI 생성 및 3D GLB 변환 라이프사이클 통합 관리
    /// </summary>
    public partial class ProjectAiImage
    {
        /// <summary>
        /// 프로젝트 고유 ID (복합 기본키 구성 요소)
        /// </summary>
        public string PrjId { get; set; } = null!;

        /// <summary>
        /// 포장차수 (복합 기본키 구성 요소)
        /// </summary>
        public string PackLevel { get; set; } = null!;

        /// <summary>
        /// 프로젝트 유저 ID
        /// </summary>
        public string? Prjuserid { get; set; }

        /// <summary>
        /// 2D 작업 고유 ID (예: img-job-044)
        /// </summary>
        public string? JobId { get; set; }

        /// <summary>
        /// 클라이언트 2D 요청 추적 ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 디자인 서술 프롬프트
        /// </summary>
        public string? Prompt { get; set; }

        /// <summary>
        /// 규제 수정 지시 사항
        /// </summary>
        public string? EcoFix { get; set; }

        /// <summary>
        /// 소재 종류
        /// </summary>
        public string? Material { get; set; }

        /// <summary>
        /// 입력된 원본 이미지 데이터(BASE64)
        /// </summary>
        public string? InputImage { get; set; }

        /// <summary>
        /// 2D 작업 상태 (QUEUED, PROCESSING, COMPLETE, FAILED)
        /// </summary>
        public string Status { get; set; } = "QUEUED";

        /// <summary>
        /// 2D 작업 진행률 (0 ~ 100)
        /// </summary>
        public int Progress { get; set; } = 0;

        /// <summary>
        /// 2D 성공 여부
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 2D 상태 설명 메시지
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// 2D 실패 시 에러 메시지
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 원본 중심 2D 결과 이미지(BASE64)
        /// </summary>
        public string? ResultDataOriginal { get; set; }

        /// <summary>
        /// 중간 변형 2D 결과 이미지(BASE64)
        /// </summary>
        public string? ResultDataModerate { get; set; }

        /// <summary>
        /// 자유 리디자인 2D 결과 이미지(BASE64)
        /// </summary>
        public string? ResultDataRedesign { get; set; }

        /// <summary>
        /// 3D GLB 변환 작업 고유 ID (예: glb-job-069)
        /// </summary>
        public string? GlbJobId { get; set; }

        /// <summary>
        /// 클라이언트 3D 요청 추적 ID
        /// </summary>
        public string? RequestIdGlb { get; set; }

        /// <summary>
        /// 3D 변환에 사용된 소스 이미지 식별자 (예: img-job-044#0)
        /// </summary>
        public string? SourceImageId { get; set; }

        /// <summary>
        /// 3D 변환 상태 (NONE, QUEUED, PROCESSING, COMPLETE, FAILED)
        /// </summary>
        public string? Status3d { get; set; } = "NONE";

        /// <summary>
        /// 3D 변환 진행률 (0 ~ 100)
        /// </summary>
        public int Progress3d { get; set; } = 0;

        /// <summary>
        /// 3D 성공 여부
        /// </summary>
        public bool Success3d { get; set; } = true;

        /// <summary>
        /// 3D 상태 설명 메시지
        /// </summary>
        public string? StatusMessage3d { get; set; }

        /// <summary>
        /// 3D 변환 실패 시 에러 메시지
        /// </summary>
        public string? ErrorMessage3d { get; set; }

        /// <summary>
        /// 원본 중심 3D GLB 파일 데이터(BASE64)
        /// </summary>
        public string? ResultDataGlbOriginal { get; set; }

        /// <summary>
        /// 중간 변형 3D GLB 파일 데이터(BASE64)
        /// </summary>
        public string? ResultDataGlbModerate { get; set; }

        /// <summary>
        /// 자유 리디자인 3D GLB 파일 데이터(BASE64)
        /// </summary>
        public string? ResultDataGlbRedesign { get; set; }

        /// <summary>
        /// 작업 생성 일시
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 상태 갱신 일시
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}