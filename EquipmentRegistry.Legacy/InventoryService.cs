// InventoryService.cs
namespace EquipmentRegistry.Legacy;

public sealed class InventoryService
{
    private readonly EquipmentRepository _repository;
    public InventoryService(EquipmentRepository repository) => _repository = repository;

    public IReadOnlyList<Equipment> SearchByName(string query)
    {
        // [1] ИСПРАВЛЕНО: раньше было Contains(query)
        return _repository.GetAll()
            .Where(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<Equipment> GetByStatus(EquipmentStatus status)
    {
        // [2] НЕ ИСПРАВЛЕНО: стоит != вместо ==.
        //     Метод возвращает все записи, КРОМЕ нужного статуса.
        return _repository.GetAll()
            .Where(item => item.Status != status)
            .ToList();
    }

    public decimal CalculateBookValue(Equipment equipment, DateOnly asOfDate)
    {
        var months = (asOfDate.Year - equipment.PurchaseDate.Year) * 12
                     + asOfDate.Month - equipment.PurchaseDate.Month;
        // [3] НЕ ИСПРАВЛЕНО: day месяца не учитывается (15 фев -> 10 мар считается как 1 полный месяц).
        //     Если asOfDate раньше даты покупки, months < 0 и стоимость получается выше цены покупки.

        // [4] НЕ ИСПРАВЛЕНО: months / 12 - целочисленное деление, амортизация растёт
        //     скачками раз в год, а не по полным месяцам (политика: 20% в год по месяцам).
        var depreciation = equipment.PurchasePrice * 0.20m * (months / 12);

        // [5] НЕ ИСПРАВЛЕНО: нет нижней границы. Для устройства 2010 года за 100 :
        //     стоимость = 100 - 300 = -200, а по политике должно быть 0 (Math.Max(0, ...)).
        return equipment.PurchasePrice - depreciation;
    }

    public decimal CalculateTotalBookValue(IEnumerable<Equipment> items, DateOnly asOfDate)
        => items.Sum(item => CalculateBookValue(item, asOfDate));
}