# DELETE /files/{fileId} TDD Sequence

This document defines the implementation sequence for `DELETE /files/{fileId}` using TDD.

The rule for this feature is simple: write one failing test, make the smallest production change that passes it, then refactor. Do not implement the endpoint first and backfill tests later.

## Target Behavior

`DELETE /files/{fileId}` deletes file metadata owned by the authenticated user and removes the stored object from MinIO.

Expected responses:

- `204 No Content` when the authenticated user owns the file.
- `404 Not Found` when the file does not exist.
- `404 Not Found` when the file belongs to another user.
- `401 Unauthorized` when the request has no valid token.

Initial scope:

- Delete the MinIO object synchronously through an application storage abstraction.
- Delete database metadata after storage deletion succeeds.
- Do not implement folder deletion in this feature.
- Do not implement soft delete in this feature.
- Do not implement outbox/background cleanup in this feature.

Consistency rule:

- If the file is not found, do not call storage.
- If storage deletion fails, do not delete metadata and do not commit.
- If database commit fails after storage deletion, the system may temporarily keep metadata for a missing object. A future outbox/reconciliation flow can handle that production-grade consistency gap.

## Development Rule

For every behavior below:

1. Red: write or extend the smallest test and confirm it fails.
2. Green: write the minimum production code to pass.
3. Refactor: clean names, duplication, and placement while tests stay green.

If a production type does not exist yet, a compile failure is a valid red step.

## 1. Application Unit Tests

Start in:

```text
backend/tests/StorageSystem.UnitTests/Application/Files/DeleteFile/
```

Create:

```text
DeleteFileCommandHandlerTest.cs
DeleteFileTestFixture.cs
```

### 1.1 Red: deletes owned file

Write a unit test that expects:

- handler receives `DeleteFileCommand(fileId, userId)`;
- repository returns a file owned by `userId`;
- handler calls `fileStorageRemover.DeleteAsync(file.StorageKey, cancellationToken)`;
- handler calls `fileRepository.DeleteAsync(file, cancellationToken)`;
- handler calls `unitOfWork.CommitAsync(cancellationToken)` once.

Expected initial failure:

- `DeleteFileCommand` does not exist;
- `DeleteFileCommandHandler` does not exist;
- storage deletion contract may not exist;
- repository delete contract may not exist.

### 1.2 Green: minimal use case

Create:

```text
backend/src/StorageSystem.Application/UseCases/Files/DeleteFile/DeleteFileCommand.cs
backend/src/StorageSystem.Application/UseCases/Files/DeleteFile/DeleteFileCommandHandler.cs
```

Add the smallest repository contract needed, preferably:

```csharp
Task<FileItem?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken
);

Task DeleteAsync(FileItem file, CancellationToken cancellationToken);
```

Add the smallest storage contract needed:

```csharp
Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
```

Suggested application interface:

```text
backend/src/StorageSystem.Application/Interfaces/IFileStorageRemover.cs
```

Make the handler pass only the owned-file test.

### 1.3 Red: missing file returns NotFound

Add a unit test where repository returns `null`.

Assert:

- throws `NotFoundException`;
- does not call storage deletion;
- does not call `DeleteAsync`;
- does not call `CommitAsync`.

### 1.4 Green: NotFound branch

Add the `null` guard in the handler.

Use the same not-found behavior for missing files and files from another user. Ownership filtering should happen in the repository query, not after loading another user's file.

### 1.5 Red: storage failure does not delete metadata

Add a unit test where:

- repository returns an owned file;
- `fileStorageRemover.DeleteAsync(file.StorageKey, cancellationToken)` throws.

Assert:

- exception is propagated;
- `fileRepository.DeleteAsync` is not called;
- `unitOfWork.CommitAsync` is not called.

### 1.6 Green: storage failure branch

Make the handler call storage deletion before repository deletion.

The intended order is:

```text
load file by file id + user id
delete object from storage
delete metadata from repository
commit database changes
```

Do not delete metadata first.

### 1.7 Red: invalid command

Add validator tests in:

```text
backend/tests/StorageSystem.UnitTests/Application/Files/DeleteFile/DeleteFileCommandValidatorTest.cs
```

Test:

- `FileId == Guid.Empty` is invalid;
- `UserId == Guid.Empty` is invalid.

### 1.8 Green: command validator

Create:

```text
backend/src/StorageSystem.Application/UseCases/Files/DeleteFile/DeleteFileCommandValidator.cs
```

Use FluentValidation and keep validation messages consistent with existing command validators.

### 1.9 Verify unit tests

Run:

```bash
dotnet test tests/StorageSystem.UnitTests/StorageSystem.UnitTests.csproj --filter "FullyQualifiedName~Application.Files.DeleteFile"
```

Only move to repository work when these tests are green.

## 2. Repository Integration Tests

Work in:

```text
backend/tests/StorageSystem.IntegrationTests/Infrastructure/Repositories/FileRepository/
```

### 2.1 Red: query by file and user

Add a test for `GetByIdAndUserIdAsync`.

Cases:

- returns file when `fileId` and `userId` match;
- returns `null` when file exists but belongs to another user;
- returns `null` when file does not exist.

### 2.2 Green: implement repository query

Update:

```text
backend/src/StorageSystem.Infrastructure.Data.EF/Repositories/FileRepository.cs
```

Implement the query using both `Id` and `UserId`.

### 2.3 Red: delete removes metadata after commit

Add a repository integration test:

- seed user, folder, and file;
- call `DeleteAsync(file)`;
- call `EfUnitOfWork.CommitAsync`;
- assert the file is no longer in the database.

### 2.4 Green: implement DeleteAsync

Implement:

```csharp
public Task DeleteAsync(FileItem file, CancellationToken cancellationToken)
{
    context.Files.Remove(file);
    return Task.CompletedTask;
}
```

### 2.5 Verify repository tests

Run:

```bash
dotnet test tests/StorageSystem.IntegrationTests/StorageSystem.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure.Repositories.FileRepository"
```

## 3. Application Integration Tests

Work in:

```text
backend/tests/StorageSystem.IntegrationTests/Application/UseCases/Files/DeleteFile/
```

### 3.1 Red: use case deletes owned file

Create an integration test that uses real repositories and `EfUnitOfWork`:

- seed user, folder, file;
- use a fake `IFileStorageRemover`;
- execute `DeleteFileCommand(file.Id, user.Id)`;
- assert the fake storage remover received `file.StorageKey`;
- assert file no longer exists.

### 3.2 Green: wire missing implementation details

Fix only what is needed for the integration test to pass.

### 3.3 Red: use case hides other user's file

Add a test:

- seed file for user A;
- execute delete as user B;
- expect `NotFoundException`;
- assert fake storage remover was not called;
- assert file still exists.

### 3.4 Green: ensure ownership filter is used

The handler should call `GetByIdAndUserIdAsync`, not `GetByIdAsync` followed by an ownership check.

### 3.5 Red: storage failure keeps metadata

Add an integration test with a fake storage remover that throws.

Assert:

- handler throws;
- file metadata still exists after the handler fails.

### 3.6 Green: keep storage-before-metadata order

Do not call `fileRepository.DeleteAsync` before `IFileStorageRemover.DeleteAsync`.

### 3.7 Verify application integration tests

Run:

```bash
dotnet test tests/StorageSystem.IntegrationTests/StorageSystem.IntegrationTests.csproj --filter "FullyQualifiedName~Application.UseCases.Files.DeleteFile"
```

## 4. Storage Adapter Implementation

Only implement the MinIO deletion adapter after application tests define the storage contract.

### 4.1 Red: fake storage provider supports delete in E2E

Update:

```text
backend/tests/StorageSystem.EndToEndTests/Common/FakeStorageProviders.cs
```

Make the fake provider implement the new storage deletion interface.

Add a simple way to assert deleted keys, for example an in-memory collection reset between tests.

### 4.2 Green: implement fake delete

Implement fake deletion with no external dependency.

### 4.3 Red: MinIO provider lacks delete contract

Make the production MinIO provider implement the new storage deletion interface.

Expected initial failure:

- `MinioFileUploadUrlProvider` does not implement `IFileStorageRemover`.

### 4.4 Green: implement MinIO object delete

Update:

```text
backend/src/StorageSystem.Infrastructure.Storage/Minio/MinioFileUploadUrlProvider.cs
```

Use MinIO remove-object support to delete by storage key.

The implementation should:

- validate options;
- ensure the bucket exists or tolerate missing bucket according to MinIO behavior;
- remove the object by `storageKey`.

Refactor note:

- If the provider name becomes misleading, rename it to something like `MinioFileStorageProvider` after tests are green.

## 5. API End-to-End Tests

Only add the controller endpoint after the application behavior is green.

Work in:

```text
backend/tests/StorageSystem.EndToEndTests/Api/Files/DeleteFile/
```

### 5.1 Red: DELETE returns 204

Create an E2E test:

- create a folder through the API;
- create a file through the API;
- call `DELETE /files/{fileId}`;
- assert `204 No Content`.
- assert the fake storage provider recorded the deleted `StorageKey`.

Expected initial failure:

- route does not exist, likely `405` or `404`.

### 5.2 Green: add controller endpoint

Update:

```text
backend/src/StorageSystem.Api/Controllers/FilesController.cs
```

Add:

```csharp
[HttpDelete("{fileId:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(
    Guid fileId,
    CancellationToken cancellationToken
)
{
    var userId = await currentUser.GetUserIdAsync(cancellationToken);
    await mediator.Send(new DeleteFileCommand(fileId, userId), cancellationToken);
    return NoContent();
}
```

### 5.3 Red: deleted file cannot be downloaded

Extend the E2E test:

- after delete, call `GET /files/{fileId}/download`;
- assert `404 Not Found`.

### 5.4 Green: fix any missing flow

Make only the smallest changes needed.

### 5.5 Red: missing file returns 404

Add E2E test:

- call `DELETE /files/{Guid.NewGuid()}`;
- assert `404 Not Found`.
- assert fake storage deletion was not called.

### 5.6 Verify E2E tests

Run:

```bash
dotnet test tests/StorageSystem.EndToEndTests/StorageSystem.EndToEndTests.csproj --filter "FullyQualifiedName~Api.Files.DeleteFile"
```

## 6. Full Automated Verification

After all focused tests pass, run the full suite:

```bash
dotnet build StorageSystem.slnx
dotnet test StorageSystem.slnx --no-build
docker compose -f ..\docker-compose.yml build backend
git diff --check
```

Do not do manual testing until automated verification is green.

## 7. Manual Test With Docker Compose

Start or rebuild the stack:

```bash
docker compose -f ..\docker-compose.yml up -d --build
```

Open:

```text
http://localhost:8080/swagger
```

Authorize with Keycloak:

```text
username: demo
password: demo
client_id: storage-swagger
```

Manual flow:

1. `POST /folders`
   - create a folder.
2. `POST /files`
   - create file metadata using the folder id.
   - copy the returned file id.
   - copy the returned upload URL.
3. Upload content to MinIO using the presigned upload URL.
   - use `curl -X PUT` or another HTTP client.
4. `GET /files/{fileId}/download`
   - copy the returned download URL.
5. Request the download URL directly.
   - confirm it returns the uploaded content before deletion.
6. `DELETE /files/{fileId}`
   - expect `204 No Content`.
7. `GET /files/{fileId}/download`
   - expect `404 Not Found`.
8. Request the old download URL directly before it expires.
   - expect MinIO to return an object-not-found response.
9. `DELETE /files/{sameFileId}`
   - expect `404 Not Found`.

## 8. Commit Shape

Preferred commit:

```text
feat: add file deletion endpoint
```

Commit only after:

- focused unit tests pass;
- focused integration tests pass;
- focused E2E tests pass;
- full test suite passes;
- Docker backend build passes;
- manual Swagger flow is verified.
