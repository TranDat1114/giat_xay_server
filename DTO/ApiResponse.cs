namespace giat_xay_server;
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Result { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}