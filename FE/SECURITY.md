# Security Policy

- Do not commit `.env`, access tokens, API credentials, cookies, or user data.
- Keep authentication tokens in the server-managed secure cookie flow; do not move them to `localStorage`.
- Report authentication, authorization, XSS, CSRF, or data-exposure findings privately to the repository owner.
- Protect `main`, require pull requests and successful CI, and disable force pushes.
- Treat all `NEXT_PUBLIC_*` values as public browser configuration.
