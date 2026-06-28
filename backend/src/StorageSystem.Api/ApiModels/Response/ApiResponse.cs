namespace StorageSystem.Api.ApiModels.Response;

public sealed class ApiResponse<TData>(TData data)
{
    public TData Data { get; } = data;
}
