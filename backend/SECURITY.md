# Security Policy

## Reporting

Do not open a public issue for authentication, authorization, data exposure, or secret-leak findings. Report them privately to the repository owner.

## Repository rules

- Never commit `.env`, credentials, access tokens, connection strings, database dumps, evidence files, or report files.
- Use `.env.example` only for local-development placeholders.
- Rotate a credential immediately if it appears in Git history, chat, logs, screenshots, or CI output.
- Protect `main`, require pull requests and successful CI, and disable force pushes.
- Store deployment secrets in the deployment platform or GitHub Environment secrets.

## Local Docker boundary

The default Compose stack binds published ports to `127.0.0.1` and uses local-only credentials. It is not a production deployment configuration.
