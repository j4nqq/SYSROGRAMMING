namespace EquipmentRegistry.Legacy;

public sealed class InventoryService
{
    private readonly EquipmentRepository _repository;
    public InventoryService(EquipmentRepository repository) => _repository = repository;

    public IReadOnlyList<Equipment> SearchByName(string query)
    {
        // [1] Поиск без учёта регистра. Пустой запрос по-прежнему возвращает все записи.
        return _repository.GetAll()
            .Where(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<Equipment> GetByStatus(EquipmentStatus status)
    {
        // [2] Было != (все записи, кроме нужного статуса). Теперь ==.
        return _repository.GetAll()
            .Where(item => item.Status == status)
            .ToList();
    }

    public decimal CalculateBookValue(Equipment equipment, DateOnly asOfDate)
    {
        // Политика: линейная амортизация 20% первоначальной стоимости в год,
        // пропорционально полным месяцам. Остаточная стоимость не может быть отрицательной.
        var months = (asOfDate.Year - equipment.PurchaseDate.Year) * 12
                     + asOfDate.Month - equipment.PurchaseDate.Month;

        // [3] Неполный месяц не считается: 15 фев -> 10 мар это ещё 0 полных месяцев.
        if (asOfDate.Day < equipment.PurchaseDate.Day)
            months--;

        // [3] Дата раньше покупки: амортизации нет, а не "отрицательная амортизация".
        if (months < 0)
            months = 0;

        // [3] Было months / 12 (целочисленное деление, скачки раз в год). Теперь по месяцам.
        var depreciation = equipment.PurchasePrice * 0.20m * months / 12m;

        // [3] Остаточная стоимость не отрицательна.
        return Math.Max(0m, equipment.PurchasePrice - depreciation);
    }

    public decimal CalculateTotalBookValue(IEnumerable<Equipment> items, DateOnly asOfDate)
        => items.Sum(item => CalculateBookValue(item, asOfDate));
}