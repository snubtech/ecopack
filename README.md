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

개발 시 Vite가 `/api` 요청을 `http://localhost:5258`로 프록시합니다.

## 실행 방법

### 1. API

```bash
cd ecopack.Api
dotnet run
```

기본 주소: `http://localhost:5258`

### 2. Web

```bash
cd ecopack.Web
npm install
npm run dev
```

기본 주소: `http://localhost:5173`

## 로그인·보안 설정

모든 API는 로그인 토큰이 있어야 부를 수 있습니다. 로그인하면 서버가 토큰을 발급하고, 화면이 이후 요청마다 자동으로 실어 보냅니다. 설정은 `ecopack.Api/appsettings.json`의 `Jwt`, `Admin`, `LoginGuard`, `RateLimit` 구역에 있습니다.

### 서명 키 (`ECOPACK_JWT_KEY`)

토큰에 서명하는 키입니다. **설정하지 않아도 서버는 켜집니다.** 그때는 서버가 켜질 때마다 임시 키를 새로 만들어 메모리에만 두고 씁니다.

임시 키로 운영하면 키를 관리할 필요가 없는 대신 다음을 감수해야 합니다.

- 서버를 다시 켜면(배포, 재부팅 포함) 모든 사용자가 다시 로그인해야 합니다.
- 서버를 여러 대로 늘리면 서버마다 키가 달라 로그인이 유지되지 않습니다.

위 두 가지를 피하려면 환경변수 `ECOPACK_JWT_KEY`에 고정 키를 넣습니다. 이 키를 아는 사람은 누구의 토큰이든 만들 수 있으므로 **저장소에 올리지 않습니다.** `appsettings.json`에 적어도 읽지 않습니다.

키는 32자 이상의 임의 문자열입니다. 아래 명령으로 만들 수 있습니다.

```bash
openssl rand -base64 48
```

만든 값을 서버를 실행하는 계정의 환경변수로 넣습니다.

```bash
# macOS / Linux
export ECOPACK_JWT_KEY='만든 값'
```

```powershell
# Windows PowerShell
$env:ECOPACK_JWT_KEY = '만든 값'
```

키를 바꾸면 그 전에 발급된 토큰은 모두 못 쓰게 됩니다(전원 다시 로그인).

### 관리자

`Admin:RepCustIds`에 적힌 아이디만 관리용 API(대화 수동 백업 `POST /api/LlmChat/Archive`, IF005 수동 수집 `POST /api/If005Sync/Run`)를 부를 수 있습니다. 목록을 바꾼 뒤에는 서버를 다시 켜야 반영됩니다. 자동 백업과 자동 수집은 이 설정과 무관하게 돕니다.

### 배포할 때

- **서버(`ecopack.Api`)와 화면(`ecopack.Web`)을 반드시 함께 배포합니다.** 서버만 올리면 화면의 모든 요청이 401로 막히고, 화면만 올리면 로그인이 되지 않습니다.
- 배포 후에는 모든 사용자가 다시 로그인해야 합니다.
- `ECOPACK_JWT_KEY`를 설정하지 않았다면 서버를 다시 켤 때마다 모든 사용자가 다시 로그인해야 합니다.
- 서버를 프록시(nginx 등) 뒤에 두면 접속 IP가 모두 프록시 주소로 보입니다. 로그인 요청 제한이 IP별로 동작하게 하려면 전달 헤더(ForwardedHeaders) 설정이 필요합니다.

## 다음 단계

- 도메인 모델·DB 연동 (`ecopack.Api/Services`, EF Core 등)
- 인증/권한
- 페이지 라우팅 (`react-router-dom`)
- 패키징·LCA 관련 API/화면 추가
