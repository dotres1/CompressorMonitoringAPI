using System;
using System.Collections.Generic;

namespace CompressorMonitoringAPI.Models
{
    public static class DataContext
    {
        public static List<Equipment> Equipment { get; set; } = new List<Equipment>
        {
            new Equipment 
            { 
                Id = 1, 
                Name = "КП-25-01", 
                Type = "Поршневой компрессор", 
                Location = "Цех 1, Линия А", 
                ProcessStage = "Сжатие",
                InstallationDate = new DateTime(2020, 3, 15), 
                Status = "Operational",
                Specifications = new Dictionary<string, string>
                {
                    { "Производительность", "25000 м³/час" },
                    { "Давление нагнетания", "7.5 МПа" },
                    { "Мощность", "2.5 МВт" }
                }
            },
            new Equipment 
            { 
                Id = 2, 
                Name = "GA-90-01", 
                Type = "Винтовой компрессор", 
                Location = "Цех 2, КИПиА", 
                ProcessStage = "Вспомогательное",
                InstallationDate = new DateTime(2021, 7, 20), 
                Status = "Warning",
                Specifications = new Dictionary<string, string>
                {
                    { "Производительность", "12 м³/мин" },
                    { "Рабочее давление", "0.8 МПа" },
                    { "Мощность", "90 кВт" }
                }
            },
            new Equipment 
            { 
                Id = 3, 
                Name = "Taurus-60-01", 
                Type = "Турбокомпрессор", 
                Location = "Цех 1, Главная линия", 
                ProcessStage = "Бустерирование",
                InstallationDate = new DateTime(2019, 11, 10), 
                Status = "Critical",
                Specifications = new Dictionary<string, string>
                {
                    { "Мощность", "5.2 МВт" },
                    { "Обороты", "15000 об/мин" },
                    { "Расход газа", "80000 м³/час" }
                }
            }
        };

        public static List<MonitoringReport> Reports { get; set; } = new List<MonitoringReport>
        {
            new MonitoringReport
            {
                Id = 1,
                EquipmentId = 1,
                ReportDate = DateTime.Now.AddHours(-2),
                OperatorName = "Иванов А.С.",
                Shift = "Утренняя",
                Parameters = new Dictionary<string, double>
                {
                    { "ДавлениеВсасывания", 0.8 },
                    { "ДавлениеНагнетания", 7.2 },
                    { "ТемператураГазов", 85.5 },
                    { "ТемператураМасла", 65.0 },
                    { "ВибрацияРадиальная", 2.1 }
                },
                Conclusions = "Норма",
                ReportType = "Оперативный"
            },
            new MonitoringReport
            {
                Id = 2,
                EquipmentId = 2,
                ReportDate = DateTime.Now.AddHours(-1),
                OperatorName = "Петров И.В.",
                Shift = "Утренняя",
                Parameters = new Dictionary<string, double>
                {
                    { "ДавлениеВходное", 0.1 },
                    { "ДавлениеВыходное", 0.75 },
                    { "ТемператураМасла", 68.0 },
                    { "ВлажностьВоздуха", 15.0 }
                },
                Conclusions = "Повышенная температура масла",
                ReportType = "Оперативный"
            },
            new MonitoringReport
            {
                Id = 3,
                EquipmentId = 3,
                ReportDate = DateTime.Now.AddHours(-3),
                OperatorName = "Сидоров В.П.",
                Shift = "Ночная",
                Parameters = new Dictionary<string, double>
                {
                    { "ОборотыРотора", 14850 },
                    { "ТемператураПодшипников", 95.0 },
                    { "ВибрацияОсевая", 4.5 },
                    { "ВибрацияРадиальная", 3.8 },
                    { "ДавлениеМасла", 0.25 }
                },
                Conclusions = "Критическая вибрация и температура",
                ReportType = "Аварийный"
            }
        };
    }
}