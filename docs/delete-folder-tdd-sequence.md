# DELETE /folders/{folderId} TDD Sequence

This document defines the implementation sequence for `DELETE /folders/{folderId}` using TDD.

The first implementation deliberately deletes only empty folders. Recursive folder deletion is a future evolution. This keeps the first version safe and reviewable because deleting folders with files requires coordinated database metadata deletion and MinIO object deletion.

The rule for this feature is the same as `DELETE /files/{fileId}`: write one failing test, make the smallest production change that passes it, then refactor. Do not implement the endpoint first and backfill tests later.

## Target Behavior

`DELETE /folders/{folderId}` deletes an empty folder owned by the authenticated user.

Expected responses:

- `204 No Content` when the authenticated user owns the folder and the folder is empty.
- `404 Not Found` when the folder does not exist.
- `404 Not Found` when the folder belongs to another user.
- `409 Conflict` when the folder contains files.
- `409 Conflict` when the folder contains subfolders.
- `401 Unauthorized` when the request has no valid token.

Initial scope:

- Delete only empty folders.
- Do not delete files from MinIO in this feature version.
- Do not delete subfolders recursively in this feature version.
- Do not rely on database cascade to delete files.
- Do not implement soft delete in this feature.
- Do not implement outbox/background cleanup in this feature.

Consistency rule:

- If the folder is not found, do not check folder contents and do not commit.
- If the folder contains files, do not delete the folder and do not commit.
- If the folder contains subfolders, do not delete the folder and do not commit.
- The empty-folder implementation should not call storage deletion.
- Recursive deletion must be designed separately because it needs storage deletion before metadata deletion for every contained file.

## Why Start With Empty Folder Delete

The current EF model has:

- files pointing to folders with cascade delete;
- subfolders pointing to parent folders with restrict delete.

Even though files can cascade at the database level, this API must not use that cascade for folder deletion because file objects also exist in MinIO. Deleting metadata without deleting MinIO objects would create orphaned storage objects.

Starting with empty folder delete makes the first endpoint correct and production-safe. A future recursive delete can be added with a separate TDD sequence that loads the folder tree, deletes each MinIO object, deletes metadata, and handles partial failure scenarios explicitly.

## Development Rule

For every behavior below:

1. Red: write or extend the smallest test and confirm it fails.
2. Green: write the minimum production code to pass.
3. Refactor: clean names, duplication, and placement while tests stay green.

If a production type does not exist yet, a compile failure is a valid red step.

## 1. Application Unit Tests

Start in:

```text
backend/tests/StorageSystem.UnitTests/Application/Folders/DeleteFolder/
```

Create:

```text
DeleteFolderCommandHandlerTest.cs
DeleteFolderCommandValidatorTest.cs
DeleteFolderTestFixture.cs
```

### 1.1 Red: deletes owned empty folder

Write a unit test that expects:

- handler receives `DeleteFolderCommand(folderId, userId)`;
- folder repository returns a folder owned by `userId`;
- file repository says the folder has no files;
- folder repository says the folder has no subfolders;
- handler calls `folderRepository.DeleteAsync(folder, cancellationToken)`;
- handler calls `unitOfWork.CommitAsync(cancellationToken)` once.

Expected initial failure:

- `DeleteFolderCommand` does not exist;
- `DeleteFolderCommandHandler` does not exist;
- repository ownership query does not exist;
- folder delete contract does not exist;
- folder content checks do not exist.

### 1.2 Green: minimal use case

Create:

```text
backend/src/StorageSystem.Application/UseCases/Folders/DeleteFolder/DeleteFolderCommand.cs
backend/src/StorageSystem.Application/UseCases/Folders/DeleteFolder/DeleteFolderCommandHandler.cs
```

Add the smallest repository contracts needed.

Suggested folder repository contracts:

```csharp
Task<Folder?> GetByIdAndUserIdAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken
);

Task<bool> HasSubFoldersAsync(
    Guid folderId,
    Guid userId,
    CancellationToken cancellationToken
);

Task DeleteAsync(Folder folder, CancellationToken cancellationToken);
```

Suggested file repository contract:

```csharp
Task<bool> ExistsInFolderAsync(
    Guid folderId,
    Guid userId,
    CancellationToken cancellationToken
);
```

Make the handler pass only the owned-empty-folder test.

The intended order is:

```text
load folder by folder id + user id
check if folder has files
check if folder has subfolders
delete folder metadata
commit database changes
```

### 1.3 Red: missing folder returns NotFound

Add a unit test where the folder repository returns `null`.

Assert:

- throws `NotFoundException`;
- does not check files;
- does not check subfolders;
- does not call `DeleteAsync`;
- does not call `CommitAsync`.

### 1.4 Green: NotFound branch

Add the `null` guard in the handler.

Use the same not-found behavior for missing folders and folders from another user. Ownership filtering should happen in the repository query, not after loading another user's folder.

### 1.5 Red: folder with files returns Conflict

Add a unit test where:

- folder repository returns an owned folder;
- file repository says the folder has files.

Assert:

- throws `ConflictException`;
- does not check subfolders if files already block deletion;
- does not call `folderRepository.DeleteAsync`;
- does not call `CommitAsync`.

Suggested message:

```text
Folder '{folderId}' cannot be deleted because it contains files.
```

### 1.6 Green: file-content guard

Make the handler block deletion when the folder contains files.

Do not delete files, metadata, or storage objects in this version.

### 1.7 Red: folder with subfolders returns Conflict

Add a unit test where:

- folder repository returns an owned folder;
- file repository says the folder has no files;
- folder repository says the folder has subfolders.

Assert:

- throws `ConflictException`;
- does not call `folderRepository.DeleteAsync`;
- does not call `CommitAsync`.

Suggested message:

```text
Folder '{folderId}' cannot be deleted because it contains subfolders.
```

### 1.8 Green: subfolder guard

Make the handler block deletion when the folder contains subfolders.

Do not implement recursive deletion here.

### 1.9 Red: invalid command

Add validator tests:

- `FolderId == Guid.Empty` is invalid;
- `UserId == Guid.Empty` is invalid.

### 1.10 Green: command validator

Create:

```text
backend/src/StorageSystem.Application/UseCases/Folders/DeleteFolder/DeleteFolderCommandValidator.cs
```

Use FluentValidation and keep validation style consistent with existing command validators.

### 1.11 Verify unit tests

Run:

```bash
dotnet test tests/StorageSystem.UnitTests/StorageSystem.UnitTests.csproj --filter "FullyQualifiedName~Application.Folders.DeleteFolder"
```

Only move to repository work when these tests are green.

## 2. Repository Integration Tests

Work in:

```text
backend/tests/StorageSystem.IntegrationTests/Infrastructure/Repositories/FolderRepository/
backend/tests/StorageSystem.IntegrationTests/Infrastructure/Repositories/FileRepository/
```

### 2.1 Red: query folder by id and user

Add tests for `FolderRepository.GetByIdAndUserIdAsync`.

Cases:

- returns folder when `folderId` and `userId` match;
- returns `null` when folder exists but belongs to another user;
- returns `null` when folder does not exist.

### 2.2 Green: implement folder ownership query

Update:

```text
backend/src/StorageSystem.Infrastructure.Data.EF/Repositories/FolderRepository.cs
```

Implement the query using both `Id` and `UserId`.

### 2.3 Red: detects subfolders

Add tests for `FolderRepository.HasSubFoldersAsync`.

Cases:

- returns `true` when an owned child folder has `ParentFolderId == folderId`;
- returns `false` when the folder has no children;
- returns `false` when only another user's child folder points to a different owned parent.

### 2.4 Green: implement subfolder check

Implement the check using `ParentFolderId` and `UserId`.

### 2.5 Red: file repository detects files in folder

Add tests for `FileRepository.ExistsInFolderAsync`.

Cases:

- returns `true` when the owned folder has files;
- returns `false` when the folder has no files;
- returns `false` for files belonging to another user.

### 2.6 Green: implement file-in-folder check

Update:

```text
backend/src/StorageSystem.Infrastructure.Data.EF/Repositories/FileRepository.cs
```

Implement the check using both `FolderId` and `UserId`.

### 2.7 Red: delete removes folder after commit

Add a repository integration test:

- seed user and empty folder;
- call `FolderRepository.DeleteAsync(folder)`;
- call `EfUnitOfWork.CommitAsync`;
- assert the folder is no longer in the database.

### 2.8 Green: implement folder DeleteAsync

Implement:

```csharp
public Task DeleteAsync(Folder folder, CancellationToken cancellationToken)
{
    context.Folders.Remove(folder);
    return Task.CompletedTask;
}
```

### 2.9 Verify repository tests

Run:

```bash
dotnet test tests/StorageSystem.IntegrationTests/StorageSystem.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure.Repositories.FolderRepository"
dotnet test tests/StorageSystem.IntegrationTests/StorageSystem.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure.Repositories.FileRepository"
```

## 3. Application Integration Tests

Work in:

```text
backend/tests/StorageSystem.IntegrationTests/Application/UseCases/Folders/DeleteFolder/
```

### 3.1 Red: use case deletes owned empty folder

Create an integration test that uses real repositories and `EfUnitOfWork`:

- seed user and folder;
- execute `DeleteFolderCommand(folder.Id, user.Id)`;
- assert folder no longer exists.

### 3.2 Green: wire missing implementation details

Fix only what is needed for the integration test to pass.

### 3.3 Red: use case hides another user's folder

Add a test:

- seed folder for user A;
- execute delete as user B;
- expect `NotFoundException`;
- assert folder still exists.

### 3.4 Green: ensure ownership filter is used

The handler should call `GetByIdAndUserIdAsync`, not `GetByIdAsync` followed by an ownership check.

### 3.5 Red: folder with files is not deleted

Add a test:

- seed user, folder, and file;
- execute delete for the folder;
- expect `ConflictException`;
- assert folder still exists;
- assert file still exists.

### 3.6 Green: block folders with files

Keep this version empty-folder-only.

### 3.7 Red: folder with subfolders is not deleted

Add a test:

- seed user, parent folder, and child folder;
- execute delete for the parent folder;
- expect `ConflictException`;
- assert parent folder still exists;
- assert child folder still exists.

### 3.8 Green: block folders with subfolders

Keep this version non-recursive.

### 3.9 Verify application integration tests

Run:

```bash
dotnet test tests/StorageSystem.IntegrationTests/StorageSystem.IntegrationTests.csproj --filter "FullyQualifiedName~Application.UseCases.Folders.DeleteFolder"
```

## 4. API End-to-End Tests

Only add the controller endpoint after the application behavior is green.

Work in:

```text
backend/tests/StorageSystem.EndToEndTests/Api/Folders/DeleteFolder/
```

### 4.1 Red: DELETE returns 204 for empty folder

Create an E2E test:

- create a folder through the API;
- call `DELETE /folders/{folderId}`;
- assert `204 No Content`.

Expected initial failure:

- route does not exist, likely `405` or `404`.

### 4.2 Green: add controller endpoint

Update:

```text
backend/src/StorageSystem.Api/Controllers/FoldersController.cs
```

Add:

```csharp
[HttpDelete("{folderId:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> Delete(
    Guid folderId,
    CancellationToken cancellationToken
)
{
    var userId = await currentUser.GetUserIdAsync(cancellationToken);
    await mediator.Send(new DeleteFolderCommand(folderId, userId), cancellationToken);
    return NoContent();
}
```

### 4.3 Red: missing folder returns 404

Add E2E test:

- call `DELETE /folders/{Guid.NewGuid()}`;
- assert `404 Not Found`.

### 4.4 Green: fix missing flow

Make only the smallest changes needed.

### 4.5 Red: folder with files returns 409

Add E2E test:

- create folder through the API;
- create file metadata in that folder through the API;
- call `DELETE /folders/{folderId}`;
- assert `409 Conflict`;
- call `GET /files/{fileId}/download`;
- assert the file metadata still exists by receiving `200 OK`.

Do not assert storage deletion because this version must not call storage.

### 4.6 Green: expose conflict behavior

Ensure `ConflictException` is mapped to `409 Conflict` by the existing exception handling.

### 4.7 Red: folder with subfolders returns 409

Add E2E test:

- create parent folder through the API;
- create child folder through the API using `parentFolderId`;
- call `DELETE /folders/{parentFolderId}`;
- assert `409 Conflict`.

### 4.8 Verify E2E tests

Run:

```bash
dotnet test tests/StorageSystem.EndToEndTests/StorageSystem.EndToEndTests.csproj --filter "FullyQualifiedName~Api.Folders.DeleteFolder"
```

## 5. Full Automated Verification

After all focused tests pass, run the full suite:

```bash
dotnet build StorageSystem.slnx
dotnet test StorageSystem.slnx --no-build
docker compose -f ..\docker-compose.yml build backend
git diff --check
```

Do not do manual testing until automated verification is green.

## 6. Manual Test With Docker Compose

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
   - create an empty root folder.
2. `DELETE /folders/{folderId}`
   - expect `204 No Content`.
3. `DELETE /folders/{sameFolderId}`
   - expect `404 Not Found`.
4. `POST /folders`
   - create another parent folder.
5. `POST /files`
   - create file metadata inside that parent folder.
6. `DELETE /folders/{parentFolderId}`
   - expect `409 Conflict`.
7. `GET /files/{fileId}/download`
   - expect `200 OK`, proving the file metadata was not deleted.
8. `POST /folders`
   - create a child folder using the parent folder id.
9. `DELETE /folders/{parentFolderId}`
   - expect `409 Conflict` because the parent has a subfolder.

## 7. Future Recursive Delete

Recursive deletion should be implemented as a separate feature, not as hidden scope inside the first folder delete endpoint.

Future behavior:

- delete all descendant files from MinIO;
- delete all descendant file metadata;
- delete all descendant folders;
- delete the requested folder;
- return `204 No Content` when the whole tree is deleted.

Future TDD sequence should include:

- repository query that loads the full folder tree owned by the user;
- repository query that lists all files inside the folder tree;
- application tests proving storage deletion happens before metadata deletion;
- failure test proving metadata is not deleted if any storage deletion fails;
- integration tests for nested folders and multiple files;
- E2E test proving deleted files cannot be downloaded;
- manual MinIO verification using old presigned URLs.

Production consistency note:

- Recursive delete has the same database-after-storage consistency gap as file deletion.
- If storage deletion succeeds and database commit fails, the system may keep metadata for missing objects.
- A future outbox/reconciliation flow is the correct production-grade solution for that gap.

## 8. Commit Shape

Preferred commit:

```text
feat: add empty folder deletion endpoint
```

Commit only after:

- focused unit tests pass;
- focused integration tests pass;
- focused E2E tests pass;
- full test suite passes;
- Docker backend build passes;
- manual Swagger flow is verified.
