namespace EquipmentRegistry.Legacy;

public sealed class EquipmentRepository
{
    private readonly List<Equipment> _items =
    [
        new() { Id = 101, Name = "Laptop Dell", PurchasePrice = 1200m, PurchaseDate = new(2022, 2, 15), Status = EquipmentStatus.InUse },
        new() { Id = 102, Name = "Monitor LG", PurchasePrice = 280m, PurchaseDate = new(2024, 1, 10), Status = EquipmentStatus.InUse },
        new() { Id = 103, Name = "Printer HP", PurchasePrice = 450m, PurchaseDate = new(2021, 6, 5), Status = EquipmentStatus.InRepair },
        new() { Id = 104, Name = "Laptop Lenovo", PurchasePrice = 980m, PurchaseDate = new(2023, 9, 20), Status = EquipmentStatus.WrittenOff },
        new() { Id = 105, Name = "Network switch", PurchasePrice = 160m, PurchaseDate = new(2025, 3, 1), Status = EquipmentStatus.InUse }
    ];

    public IReadOnlyList<Equipment> GetAll() => _items;

    public Equipment? FindById(int id) => _items.FirstOrDefault(item => item.Id == id);
}
