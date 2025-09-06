namespace DeviceArchiving.Data.Dto.Operations;

public class CreateOperation 
{
    public int DeviceId { get; set; }
    public string OperationName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Comment { get; set; }
}
