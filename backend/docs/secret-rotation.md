# Secret Rotation Checklist

## Rotate Externally

- MongoDB Atlas user password.
- Gmail app password.
- Neon Postgres password.
- Ministry API key.
- JWT secret before any non-local deployment.

## Update Local After Rotation

```powershell
dotnet user-secrets set "ConnectionStrings:Postgres" "<new-postgres-connection>" --project src\EduHub.WebApi
dotnet user-secrets set "ConnectionStrings:Mongo" "<new-mongo-connection>" --project src\EduHub.WebApi
dotnet user-secrets set "Email:Smtp:Password" "<new-gmail-app-password>" --project src\EduHub.WebApi
dotnet user-secrets set "MinistryApi:ApiKey" "<new-ministry-api-key>" --project src\EduHub.WebApi
dotnet user-secrets set "Auth:Jwt:Secret" "<new-32-plus-char-secret>" --project src\EduHub.WebApi
```

## Update Docker After Rotation

- Update local `.env`.
- Tạo `REDIS_PASSWORD` riêng, không dùng chung PostgreSQL password.
- Never commit `.env`.
- Keep `.env.example` placeholder-only.

## Current Risk

- Secrets were pasted into chat.
- Treat them as exposed.
- Rotate before sharing repo, deploying, or using real production data.
- Rotation chỉ hoàn tất sau khi revoke credential cũ và smoke-test credential mới; sửa file local không tự rotate credential phía provider.
