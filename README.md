# Jubo_Backend_Homework

Patient / Order 管理系統後端。.NET 8、Clean Architecture（Domain / Application / Infrastructure / WebApi）+ CQRS + MediatR，PostgreSQL + EF Core，Redis 存 refresh token。詳細規格見 [SPEC.md](./SPEC.md)。

## 啟動方式（Docker）

```bash
docker compose up -d --build
```

啟動後：

- Backend: http://localhost:8000
- Swagger（Development 環境）: http://localhost:8000/swagger

首次啟動會自動跑 EF Core migration 並種入種子資料（5 位 patient + 1 個 demo 使用者）。

停止服務：

```bash
docker compose down
```

> 若本機 8000 port 已被其他服務占用，`docker compose up` 會回報 port 已被占用；可先釋放該 port，或暫時調整 `docker-compose.yml` 的 port mapping。

## Demo 帳密

```
帳號：demo
密碼：demo
```

## 本機開發（不用 Docker）

需要本機有 PostgreSQL、Redis，並依 `src/WebApi/appsettings.Development.json` / 環境變數調整連線字串。

```bash
dotnet restore
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
dotnet run --project src/WebApi
```

## 測試

```bash
# 單元測試
dotnet test tests/Application.UnitTests

# 整合測試（需要本機 Docker daemon，會自動起臨時 Postgres + Redis container）
dotnet test tests/Infrastructure.IntegrationTests
```

## API 端點

```
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout

GET    /api/patients
GET    /api/patients/{id}
POST   /api/patients
PUT    /api/patients/{id}
DELETE /api/patients/{id}

GET    /api/patients/{id}/orders
POST   /api/patients/{id}/orders
PUT    /api/orders/{id}
DELETE /api/orders/{id}
```

除 `/api/auth/*` 外皆需要 `Authorization: Bearer <accessToken>`。
