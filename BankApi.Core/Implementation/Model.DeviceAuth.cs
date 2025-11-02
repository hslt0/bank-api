public class DeviceAuthorization
{
    public int Id { get; set; }
    public string DeviceCode { get; set; } = default!;
    public string UserCode { get; set; } = default!;
    public string? Username { get; set; }
    public bool IsApproved { get; set; }
    public DateTime ExpiresAt { get; set; }
}