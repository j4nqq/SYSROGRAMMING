using EquipmentRegistry.Legacy;

var repository = new EquipmentRepository();
var service = new InventoryService(repository);
var report = new ReportFormatter(service, repository);
var today = new DateOnly(2026, 10, 27);

Console.WriteLine("=== Equipment Registry / legacy build ===");
Console.WriteLine("Поиск по слову 'laptop':");
foreach (var item in service.SearchByName("laptop"))
    Console.WriteLine($"  {item.Id}: {item.Name}");

Console.WriteLine();
Console.WriteLine("Отчёт по оборудованию в ремонте:");
Console.WriteLine(report.BuildStatusReport(EquipmentStatus.InRepair, today));
Console.WriteLine();
Console.WriteLine("Подсказка: исследуйте методы InventoryService и ReportFormatter.");
