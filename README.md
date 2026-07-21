# EduHub

EduHub là monorepo gồm Backend ASP.NET Core 10 và Frontend Next.js.

```text
BE/  API, Domain, Application, Infrastructure, migrations, tests, Docker databases
FE/  Public Site, role-based Portal, shared frontend packages, Docker images
```

## Docker local

Backend và local databases:

```powershell
cd .\BE
Copy-Item .env.example .env
docker compose up -d --build --wait
docker compose --profile tools run --rm eduhub-seed
```

Frontend:

```powershell
cd ..\FE
Copy-Item .env.example .env
docker compose up -d --build --wait
```

```text
Site:    http://localhost:3000
Portal:  http://localhost:3001
Swagger: http://localhost:8080/swagger
```

Chi tiết: [Backend](BE/README.md) | [Frontend](FE/README.md)

Không commit `.env`, token, connection string, database dump hoặc dữ liệu người dùng.
