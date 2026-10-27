namespace EquipmentRegistry.Legacy;

public sealed class InventoryService
{
    private readonly EquipmentRepository _repository;
    public InventoryService(EquipmentRepository repository) => _repository = repository;

    public IReadOnlyList<Equipment> SearchByName(string query)
    {
        // TODO 1: Исследуйте поведение при пустом запросе и при разном регистре.
        // Не меняйте контракт метода, пока не выясните, как он используется.
        return _repository.GetAll()
            .Where(item => item.Name.Contains(query))
            .ToList();
    }

    public IReadOnlyList<Equipment> GetByStatus(EquipmentStatus status)
    {
        // TODO 2: Проверьте, соответствует ли фактический результат названию метода.
        // Найдите граничный сценарий, в котором результат особенно подозрителен.
        return _repository.GetAll()
            .Where(item => item.Status != status)
            .ToList();
    }

    public decimal CalculateBookValue(Equipment equipment, DateOnly asOfDate)
    {
        // Упрощённая политика: линейная амортизация 20% первоначальной стоимости в год,
        // пропорционально полным месяцам. Остаточная стоимость не может быть отрицательной.
        // TODO 3: Сопоставьте реализацию с политикой выше и придумайте тест на старое оборудование.
        var months = (asOfDate.Year - equipment.PurchaseDate.Year) * 12
                     + asOfDate.Month - equipment.PurchaseDate.Month;

        var depreciation = equipment.PurchasePrice * 0.20m * (months / 12);
        return equipment.PurchasePrice - depreciation;
    }

    public decimal CalculateTotalBookValue(IEnumerable<Equipment> items, DateOnly asOfDate)
        => items.Sum(item => CalculateBookValue(item, asOfDate));
}
