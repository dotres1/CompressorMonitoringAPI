public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;          // Название
    public string Type { get; set; } = string.Empty;          // Тип оборудования
    public string Location { get; set; } = string.Empty;      // Местоположение в цехе
    public DateTime InstallationDate { get; set; }           // Дата установки
    public string Status { get; set; } = "Operational";      // Статус: Operational, Warning, Critical, Maintenance
    
    // Бизнес-логика
    public bool RequiresMaintenance()
    {
        return Status == "Warning" || Status == "Critical";
    }
    
    public int GetEquipmentAgeInYears()
    {
        return DateTime.Now.Year - InstallationDate.Year;
    }
}