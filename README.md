# scalable-file-storage-system

A backend storage system for managing files and nested folders with relational metadata, S3-compatible object storage, and OIDC authentication.

This project is designed as a portfolio-grade backend: it keeps infrastructure local and reproducible while applying production-oriented boundaries such as domain entities, application use cases, persistence repositories, JWT authentication, external identity mapping, and automated tests across multiple layers.

## Features

- JWT authentication through Keycloak and OpenID Connect.
- Internal domain user provisioning from authenticated tokens.
- Stable user mapping by `ExternalProvider + ExternalSubject`, instead of relying only on email.
- Folder creation with nested folder support.
- File metadata creation linked to users and folders.
- Presigned upload and download URLs backed by MinIO.
- PostgreSQL persistence with EF Core migrations.
- Swagger UI with OAuth2 Authorization Code + PKCE flow.
- Docker Compose setup for API, PostgreSQL, MinIO, and Keycloak.
- Unit, integration, and end-to-end test projects.

## Architecture

```text
Client / Swagger UI
        |
        v
   .NET Web API
        |
        +--> PostgreSQL
        |    Stores users, folders, file metadata, and hierarchy.
        |
        +--> MinIO
        |    Stores file objects through S3-compatible presigned URLs.
        |
        +--> Keycloak
             Issues OIDC/JWT tokens and manages authentication.
```

## Tech Stack

- **Backend:** .NET 10 Web API
- **Application Flow:** MediatR + FluentValidation
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Object Storage:** MinIO
- **Authentication:** Keycloak with JWT Bearer validation
- **API Docs:** Swagger / OpenAPI with OAuth2
- **Tests:** xUnit, FluentAssertions, Moq, EF Core InMemory
- **Orchestration:** Docker Compose

## Design Decisions

- **Keycloak as IAM:** Authentication is delegated to a dedicated identity provider instead of implementing custom login and password handling inside the API.
- **Internal domain user:** The API still keeps its own `User` entity because folders and files need stable domain relationships. Keycloak identifies the person; the application owns its business data.
- **External identity mapping:** Users are linked by `ExternalProvider + ExternalSubject`, where `ExternalSubject` comes from the OIDC `sub` claim. This is more stable than email, which can change.
- **PostgreSQL for hierarchy:** Object storage is flat, so folder nesting and ownership live in a relational database.
- **MinIO for local S3 compatibility:** The project can run locally without cloud dependencies while preserving an S3-like storage model.
- **Presigned URLs:** File upload and download can happen directly against object storage, avoiding unnecessary file traffic through the API.
- **Versioned migrations:** Database schema changes are explicit and reproducible with EF Core migrations.
- **Layered tests:** The test suite separates domain/application behavior, persistence integration, and HTTP end-to-end flows.

## Getting Started

### Prerequisites

- Docker
- Docker Compose
- .NET 10 SDK, only required if you want to run tests or the API outside Docker

### 1. Environment Variables

Create a `.env` file in the repository root:

```env
MINIO_ROOT_USER=admin
MINIO_ROOT_PASSWORD=supersecret

POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=storage_db

KEYCLOAK_USER=admin
KEYCLOAK_PASSWORD=admin
KEYCLOAK_AUTHORITY=http://localhost:8081/realms/StorageSystemRealm
KEYCLOAK_CLIENT_ID=storage-swagger
KEYCLOAK_METADATA_ADDRESS=http://host.docker.internal:8081/realms/StorageSystemRealm/.well-known/openid-configuration
KEYCLOAK_VALID_ISSUER=http://localhost:8081/realms/StorageSystemRealm
```

### 2. Start the Stack

From the repository root:

```bash
docker compose up -d --build
```

The API applies EF Core migrations on startup when `ApplyMigrations=true` is set in Docker Compose.

### 3. Access Local Services

- **API Swagger:** `http://localhost:8080/swagger`
- **Keycloak:** `http://localhost:8081`
- **MinIO Console:** `http://localhost:9001`
- **PostgreSQL:** `localhost:5432`

### 4. Demo Credentials

Keycloak admin:

```text
username: admin
password: admin
```

Demo application user:

```text
username: demo
password: demo
```

Swagger OAuth client:

```text
client_id: storage-swagger
flow: Authorization Code with PKCE
```

## API Endpoints

All storage endpoints require authentication.

```http
POST /folders
POST /files
GET  /files/{fileId}/download
```

Current behavior:

- `POST /folders` creates a folder for the authenticated user.
- `POST /files` creates file metadata and returns a presigned upload URL.
- `GET /files/{fileId}/download` returns a presigned download URL.

## Authentication Model

The access token is validated by the API using the Keycloak realm metadata.

When an authenticated request arrives, the API reads the token claims through `ICurrentUserAccessor` and resolves the internal user by:

```text
ExternalProvider = keycloak
ExternalSubject  = token sub claim
```

If the internal user does not exist yet, it is provisioned automatically using token profile claims such as email and name. This keeps authentication external while preserving a clean domain model for ownership, relationships, and future permissions.

## Running Tests

From the `backend` directory:

```bash
dotnet test StorageSystem.slnx
```

Test projects:

- `StorageSystem.UnitTests`: domain and application behavior.
- `StorageSystem.IntegrationTests`: repositories, persistence, and use cases with test infrastructure.
- `StorageSystem.EndToEndTests`: HTTP API flow using an in-memory test host and fake storage providers.

## Roadmap

- List folders and files.
- Rename and delete folders/files.
- User-level storage quotas.
- File sharing and permission policies.
- Background cleanup for orphaned objects.
- Observability with structured logs, metrics, and tracing.
- CI pipeline for build, test, and Docker image validation.
