# scalable-file-storage-system

A scalable storage system that allows users to manage files and nested folders.

## Tech Stack

- **Backend:** .NET 10 Web API
- **Database:** PostgreSQL (for folder metadata and hierarchy)
- **Object Storage:** MinIO (S3-compatible)
- **Orchestration:** Docker Compose

## Choices

- **MinIO instead of AWS S3:** Used to replace cloud dependency with a robust local infrastructure for development and self-hosted environments.
- **Keycloak for Auth:** Delegating Identity and Access Management (IAM) to a dedicated, battle-tested server rather than rolling custom authentication. This decouples security from the core business logic and provides out-of-the-box features like JWT validation and social login.
- **Relational DB for Nested Folders:** Since object storage is flat, PostgreSQL is used to handle the hierarchical mapping of virtual nested folders and file metadata.
- **Multi-stage Docker Builds:** Used for both Angular and .NET to keep final production images lightweight.

## Getting Started

### Prerequisites

Ensure you have [Docker](https://www.docker.com/) and Docker Compose installed on your machine.

### 1. Environment Variables

Create a `.env` file in the root directory matching the variables required by the services:

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
KEYCLOAK_METADATA_ADDRESS=http://keycloak:8080/realms/StorageSystemRealm/.well-known/openid-configuration
KEYCLOAK_VALID_ISSUER=http://localhost:8081/realms/StorageSystemRealm
```