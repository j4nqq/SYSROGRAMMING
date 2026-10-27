using EquipmentRegistry.Legacy;

var repository = new EquipmentRepository();
var service = new InventoryService(repository);
var failures = new List<string>();

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    if (!condition) failures.Add(name);
}

// Эти проверки — отправная точка для расследования. Некоторые намеренно выявляют дефекты.
Check("поиск без учёта регистра находит Laptop Dell",
    service.SearchByName("laptop").Any(x => x.Id == 101));

Check("фильтр InRepair возвращает только записи InRepair",
    service.GetByStatus(EquipmentStatus.InRepair).All(x => x.Status == EquipmentStatus.InRepair));

var oldEquipment = new Equipment
{
    Id = 999, Name = "Old test device", PurchasePrice = 100m,
    PurchaseDate = new DateOnly(2010, 1, 1), Status = EquipmentStatus.InUse
};
var value = service.CalculateBookValue(oldEquipment, new DateOnly(2026, 10, 27));
Check("балансовая стоимость не отрицательна", value >= 0m);

Console.WriteLine();
Console.WriteLine($"Итого: {failures.Count} проверок не пройдено из 3.");
Console.WriteLine("Ожидаемо: сначала разберитесь, какие проверки падают и почему.");
