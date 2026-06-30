namespace StorageSystem.EndToEndTests.ApiModels;

public class TestApiResponse<TData>
{
    public TData Data { get; set; } = default!;
}

public class TestProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
}
