namespace EquipmentRegistry.Legacy;

public sealed class ReportFormatter
{
    private readonly InventoryService _service;
    private readonly EquipmentRepository _repository;

    public ReportFormatter(InventoryService service, EquipmentRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    public string BuildStatusReport(EquipmentStatus status, DateOnly asOfDate)
    {
        var selected = _service.GetByStatus(status);
        var total = _service.CalculateTotalBookValue(_repository.GetAll(), asOfDate);

        var lines = selected.Select(item => $"{item.Id} | {item.Name} | {item.Status}").ToList();
        lines.Add($"Позиций: {selected.Count}");
        lines.Add($"Балансовая стоимость: {total:0.00}");
        return string.Join(Environment.NewLine, lines);
    }
}
