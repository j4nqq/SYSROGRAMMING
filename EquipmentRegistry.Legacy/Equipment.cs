namespace EquipmentRegistry.Legacy;

public enum EquipmentStatus
{
    InUse,
    InRepair,
    WrittenOff
}

public sealed class Equipment
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public decimal PurchasePrice { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public EquipmentStatus Status { get; init; }
}
