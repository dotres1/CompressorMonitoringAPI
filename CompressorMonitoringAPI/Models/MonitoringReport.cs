public class MonitoringReport
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }                     // Ссылка на оборудование
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public string OperatorName { get; set; } = string.Empty; // ФИО оператора
    public Dictionary<string, double> Parameters { get; set; } = new(); // Параметры: "Pressure" -> 10.5, "Temperature" -> 75.2
    public string Conclusions { get; set; } = string.Empty;  // Выводы и замечания
    
    // Бизнес-логика
    public bool IsReportValid()
    {
        return Parameters.Count > 0 && !string.IsNullOrEmpty(Conclusions);
    }
    
    public bool HasCriticalParameters()
    {
        // Проверяем есть ли критические значения параметров
        return Parameters.Values.Any(value => value > 100); // Пример: значения выше 100 - критические
    }
}