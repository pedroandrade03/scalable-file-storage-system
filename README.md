# scalable-file-storage-system

A scalable storage system that allows users to manage files and nested folders.

## Tech Stack

- **Frontend:** Angular (served via Nginx)
- **Backend:** .NET 10 Web API
- **Database:** PostgreSQL (for folder metadata and hierarchy)
- **Object Storage:** MinIO (S3-compatible)
- **Orchestration:** Docker Compose

## Choices

- **MinIO instead of AWS S3:** Used to replace cloud dependency with a robust local infrastructure for development and self-hosted environments.
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