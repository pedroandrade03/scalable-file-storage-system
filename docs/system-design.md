# System Design

This document describes the product scope, functional requirements, non-functional requirements, and current boundaries of the scalable file storage backend.

## Purpose

The system provides authenticated file and folder management with relational metadata, nested folder hierarchy, and S3-compatible object storage. The API owns business rules and metadata; file bytes are stored externally in MinIO through presigned multipart upload URLs.

## Current Scope

- Authenticated API access through Keycloak-issued JWT tokens.
- Internal user provisioning from external identity claims.
- Nested folder creation and listing.
- File metadata creation linked to users and folders.
- Multipart upload planning and completion.
- Presigned download URL generation.
- File deletion and empty-folder deletion.
- Local infrastructure through Docker Compose.
- Automated backend restore, build, and test validation through GitHub Actions.

## Actors

- **Authenticated user:** Creates folders, uploads files, downloads files, and deletes owned resources.
- **API client:** Calls the HTTP API using a Bearer token. Swagger UI is the current local exploration client.
- **Identity provider:** Keycloak authenticates users and issues JWT access tokens.
- **Object storage:** MinIO stores file bytes using an S3-compatible API.

## Functional Requirements

| ID | Requirement |
| --- | --- |
| RF01 | The API must require authentication for all storage endpoints. |
| RF02 | The API must validate JWT access tokens using Keycloak realm metadata. |
| RF03 | The API must resolve or provision an internal domain user from authenticated token claims. |
| RF04 | The API must create folders owned by the authenticated user. |
| RF05 | The API must support nested folders through an optional parent folder relationship. |
| RF06 | The API must list child folders and files for the authenticated user at root or inside a parent folder. |
| RF07 | The API must create file metadata linked to an authenticated user and optional folder. |
| RF08 | The API must return a multipart upload plan when file metadata is created. |
| RF09 | The client must be able to upload file chunks directly to object storage using presigned URLs. |
| RF10 | The API must complete multipart uploads after receiving the storage `uploadId` and uploaded part `ETag` values. |
| RF11 | The API must mark files as available only after multipart completion succeeds. |
| RF12 | The API must generate presigned download URLs for existing files. |
| RF13 | The API must delete file metadata and the related storage object when a file is deleted. |
| RF14 | The API must delete folders only when the folder can be safely removed. |

## Non-Functional Requirements

| ID | Requirement |
| --- | --- |
| RNF01 | The system must run locally with reproducible dependencies through Docker Compose. |
| RNF02 | Authentication must remain external to the API through OIDC/JWT instead of custom password handling. |
| RNF03 | User ownership, folder hierarchy, and file metadata must be persisted in PostgreSQL. |
| RNF04 | File bytes must be stored outside the relational database in S3-compatible object storage. |
| RNF05 | Large file uploads must avoid proxying file bytes through the API after presigned URL generation. |
| RNF06 | Database schema changes must be versioned with EF Core migrations. |
| RNF07 | API errors must be returned consistently through `ProblemDetails`. |
| RNF08 | The backend must be independent from any frontend client. |
| RNF09 | The project must include automated tests across domain/application, persistence, and HTTP API flows. |
| RNF10 | The main branch must be validated by CI with restore, build, and test steps. |

## Out of Scope

- Frontend application.
- Public file sharing.
- User-level storage quotas.
- Folder or file rename operations.
- Background cleanup for orphaned objects.
- Production cloud deployment.
- Release automation and Docker image publishing.

## Acceptance Criteria

- A local developer can start API, PostgreSQL, MinIO, and Keycloak with Docker Compose.
- A user can authenticate through Keycloak and call protected API endpoints.
- A user can create folders, list folder contents, create file metadata, upload file chunks to MinIO, complete the upload, and download through a presigned URL.
- Automated tests and GitHub Actions validate the backend solution.
