# EcoPack

친환경 포장(EcoPack) 연구·개발을 위한 풀스택 기본 프레임입니다.

## 구조

```
ecopack/
├── ecopack.Api/     ASP.NET Core 10 Web API
└── ecopack.Web/     React + Vite 프론트엔드
```

### Backend (`ecopack.Api`)

| 경로 | 설명 |
|------|------|
| `Endpoints/` | Minimal API 엔드포인트 |
| `Extensions/` | DI·CORS 등 설정 확장 |
| `Models/` | 공통 DTO·응답 모델 |

- API prefix: `/api/v1`
- Health check: `GET /api/v1/health`

### Frontend (`ecopack.Web`)

| 경로 | 설명 |
|------|------|
| `src/api/` | API 클라이언트 |
| `src/components/` | 재사용 UI 컴포넌트 |
| `src/pages/` | 페이지 단위 화면 |
| `src/styles/` | 전역 스타일 |

개발 시 Vite가 `/api` 요청을 `http://localhost:5260`으로 프록시합니다.

## 실행 방법

### 1. API

```bash
cd ecopack.Api
dotnet run
```

기본 주소: `http://localhost:5260`

### 2. Web

```bash
cd ecopack.Web
npm install
npm run dev
```

기본 주소: `http://localhost:5173`

## 다음 단계

- 도메인 모델·DB 연동 (`ecopack.Api/Services`, EF Core 등)
- 인증/권한
- 페이지 라우팅 (`react-router-dom`)
- 패키징·LCA 관련 API/화면 추가
