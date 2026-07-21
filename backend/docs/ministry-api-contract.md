# Ministry API Contract

## Status

- Real sandbox URL: waiting for provider.
- Auth scheme in code: `X-Api-Key`.
- Idempotency scheme in code: `Idempotency-Key`.
- Current test contract: fake Ministry server in `EduHub.IntegrationTests`.

## Gradebook Sync

```http
POST /api/v1/gradebooks
X-Api-Key: <api-key>
Idempotency-Key: <aggregate-type>:<aggregate-id>:<version>
Content-Type: application/json
```

Request:

```json
{
  "contractVersion": "ministry-gradebook-v1",
  "assignmentId": "00000000-0000-0000-0000-000000000000",
  "publicationVersion": 1,
  "grades": [
    {
      "studentId": "00000000-0000-0000-0000-000000000000",
      "componentId": "00000000-0000-0000-0000-000000000000",
      "score": 8.5
    }
  ]
}
```

Response:

```json
{
  "externalId": "external-gradebook-id",
  "externalVersion": "v1"
}
```

## Required From Ministry Provider

- Sandbox base URL.
- Production base URL.
- Exact auth scheme: API key, bearer token, mTLS, OAuth2, or signature.
- Error response contract.
- Rate limit and retry-after behavior.
- Idempotency retention window.
- Health endpoint path, if different from `/health`.
