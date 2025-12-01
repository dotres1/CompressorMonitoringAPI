using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Если уже есть данные, пропускаем инициализацию
            if (context.Equipment.Any() || context.Users.Any())
                return;

            // 1. Добавляем пользователей
            var users = new List<User>
            {
                new User { Username = "admin", PasswordHash = "admin123", Role = "Admin", FullName = "Иванов Алексей Сергеевич" },
                new User { Username = "engineer", PasswordHash = "engineer123", Role = "User", FullName = "Петров Игорь Васильевич" },
                new User { Username = "operator", PasswordHash = "operator123", Role = "User", FullName = "Сидорова Мария Петровна" },
                new User { Username = "viewer", PasswordHash = "viewer123", Role = "User", FullName = "Козлов Дмитрий Анатольевич" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // 2. Добавляем оборудование
            var equipment = new List<Equipment>
            {
                new Equipment
                {
                    Name = "КП-25-01",
                    Type = "Поршневой компрессор",
                    Location = "Цех 1, Линия А",
                    ProcessStage = "Сжатие",
                    InstallationDate = new DateTime(2020, 3, 15),
                    Status = "Operational"
                },
                new Equipment
                {
                    Name = "КП-25-02",
                    Type = "Поршневой компрессор",
                    Location = "Цех 1, Линия Б",
                    ProcessStage = "Сжатие",
                    InstallationDate = new DateTime(2020, 5, 20),
                    Status = "Warning"
                },
                new Equipment
                {
                    Name = "GA-90-01",
                    Type = "Винтовой компрессор",
                    Location = "Цех 2, КИПиА",
                    ProcessStage = "Вспомогательное",
                    InstallationDate = new DateTime(2021, 7, 20),
                    Status = "Operational"
                },
                new Equipment
                {
                    Name = "Турбокомпрессор T-60",
                    Type = "Турбокомпрессор",
                    Location = "Цех 1, Главная линия",
                    ProcessStage = "Бустерирование",
                    InstallationDate = new DateTime(2019, 11, 10),
                    Status = "Critical"
                },
                new Equipment
                {
                    Name = "Насос Н-12",
                    Type = "Насос",
                    Location = "Цех 3, Охлаждение",
                    ProcessStage = "Охлаждение",
                    InstallationDate = new DateTime(2022, 2, 28),
                    Status = "Operational"
                }
            };
            context.Equipment.AddRange(equipment);
            context.SaveChanges();

            // 3. Добавляем характеристики оборудования
            var specifications = new List<EquipmentSpecification>
            {
                // КП-25-01
                new EquipmentSpecification { EquipmentId = 1, ParameterName = "Производительность", ParameterValue = "25000", ParameterUnit = "м³/час" },
                new EquipmentSpecification { EquipmentId = 1, ParameterName = "Давление нагнетания", ParameterValue = "7.5", ParameterUnit = "МПа" },
                new EquipmentSpecification { EquipmentId = 1, ParameterName = "Мощность", ParameterValue = "2.5", ParameterUnit = "МВт" },
                new EquipmentSpecification { EquipmentId = 1, ParameterName = "Обороты", ParameterValue = "750", ParameterUnit = "об/мин" },
                
                // КП-25-02
                new EquipmentSpecification { EquipmentId = 2, ParameterName = "Производительность", ParameterValue = "28000", ParameterUnit = "м³/час" },
                new EquipmentSpecification { EquipmentId = 2, ParameterName = "Давление нагнетания", ParameterValue = "7.8", ParameterUnit = "МПа" },
                new EquipmentSpecification { EquipmentId = 2, ParameterName = "Мощность", ParameterValue = "2.8", ParameterUnit = "МВт" },
                
                // GA-90-01
                new EquipmentSpecification { EquipmentId = 3, ParameterName = "Производительность", ParameterValue = "12", ParameterUnit = "м³/мин" },
                new EquipmentSpecification { EquipmentId = 3, ParameterName = "Рабочее давление", ParameterValue = "0.8", ParameterUnit = "МПа" },
                new EquipmentSpecification { EquipmentId = 3, ParameterName = "Мощность", ParameterValue = "90", ParameterUnit = "кВт" },
                
                // Турбокомпрессор T-60
                new EquipmentSpecification { EquipmentId = 4, ParameterName = "Мощность", ParameterValue = "5.2", ParameterUnit = "МВт" },
                new EquipmentSpecification { EquipmentId = 4, ParameterName = "Обороты", ParameterValue = "15000", ParameterUnit = "об/мин" },
                new EquipmentSpecification { EquipmentId = 4, ParameterName = "Расход газа", ParameterValue = "80000", ParameterUnit = "м³/час" },
                
                // Насос Н-12
                new EquipmentSpecification { EquipmentId = 5, ParameterName = "Производительность", ParameterValue = "120", ParameterUnit = "м³/ч" },
                new EquipmentSpecification { EquipmentId = 5, ParameterName = "Напор", ParameterValue = "60", ParameterUnit = "м" },
                new EquipmentSpecification { EquipmentId = 5, ParameterName = "Мощность", ParameterValue = "45", ParameterUnit = "кВт" }
            };
            context.EquipmentSpecifications.AddRange(specifications);
            context.SaveChanges();

            // 4. Добавляем отчёты мониторинга (по 3-4 отчёта на каждое оборудование)
            var reports = new List<MonitoringReport>();
            var rand = new Random();
            
            // Для каждого оборудования создаём отчёты за последние 7 дней
            for (int equipmentId = 1; equipmentId <= 5; equipmentId++)
            {
                for (int i = 0; i < 4; i++)
                {
                    var reportDate = DateTime.Now.AddDays(-i).AddHours(-rand.Next(1, 24));
                    var healthScore = rand.Next(60, 100);
                    var healthStatus = healthScore >= 85 ? "Нормальное" : 
                                      healthScore >= 70 ? "Требует внимания" : "Критическое";
                    
                    var report = new MonitoringReport
                    {
                        EquipmentId = equipmentId,
                        ReportDate = reportDate,
                        OperatorName = users[rand.Next(1, 4)].FullName,
                        Shift = i % 3 == 0 ? "Утренняя" : i % 3 == 1 ? "Дневная" : "Ночная",
                        ReportType = healthStatus == "Критическое" ? "Аварийный" : "Оперативный",
                        Conclusions = GetRandomConclusion(healthStatus),
                        HealthScore = healthScore,
                        HealthStatus = healthStatus
                    };
                    
                    reports.Add(report);
                }
            }
            
            context.MonitoringReports.AddRange(reports);
            context.SaveChanges();

            // 5. Добавляем параметры для каждого отчёта
            var parameters = new List<ReportParameter>();
            
            foreach (var report in reports)
            {
                // Определяем критические лимиты в зависимости от типа оборудования
                var (pressureWarning, pressureCritical, tempWarning, tempCritical, vibWarning, vibCritical) = 
                    GetLimitsForEquipment(report.EquipmentId);
                
                // Генерируем случайные значения параметров
                var pressure = Math.Round(rand.NextDouble() * (pressureCritical - 5) + 5, 1);
                var temperature = Math.Round(rand.NextDouble() * (tempCritical - 30) + 30, 1);
                var vibration = Math.Round(rand.NextDouble() * (vibCritical - 1) + 1, 1);
                
                parameters.AddRange(new[]
                {
                    new ReportParameter
                    {
                        ReportId = report.Id,
                        Name = "Давление",
                        Value = pressure,
                        Unit = "МПа",
                        WarningLimit = pressureWarning,
                        CriticalLimit = pressureCritical
                    },
                    new ReportParameter
                    {
                        ReportId = report.Id,
                        Name = "Температура",
                        Value = temperature,
                        Unit = "°C",
                        WarningLimit = tempWarning,
                        CriticalLimit = tempCritical
                    },
                    new ReportParameter
                    {
                        ReportId = report.Id,
                        Name = "Вибрация",
                        Value = vibration,
                        Unit = "мм/с",
                        WarningLimit = vibWarning,
                        CriticalLimit = vibCritical
                    },
                    new ReportParameter
                    {
                        ReportId = report.Id,
                        Name = "Уровень масла",
                        Value = Math.Round(rand.NextDouble() * 30 + 70, 1), // 70-100%
                        Unit = "%",
                        WarningLimit = 75,
                        CriticalLimit = 70
                    }
                });
            }
            
            context.ReportParameters.AddRange(parameters);
            context.SaveChanges();

            Console.WriteLine("База данных успешно инициализирована с тестовыми данными!");
        }

        private static string GetRandomConclusion(string healthStatus)
        {
            var conclusions = new Dictionary<string, List<string>>
            {
                ["Нормальное"] = new List<string>
                {
                    "Все параметры в норме. Оборудование работает стабильно.",
                    "Работа в штатном режиме. Рекомендуется плановый осмотр.",
                    "Нормальные показатели. Производительность соответствует паспортной."
                },
                ["Требует внимания"] = new List<string>
                {
                    "Зафиксировано повышение температуры. Требуется дополнительный контроль.",
                    "Незначительное увеличение вибрации. Рекомендуется проверка креплений.",
                    "Снижение эффективности на 5%. Рекомендуется очистка фильтров."
                },
                ["Критическое"] = new List<string>
                {
                    "Критическое повышение давления! Требуется немедленная остановка.",
                    "Превышена максимальная температура. Автоматическое отключение сработало.",
                    "Вибрация превышает допустимые значения. Оборудование остановлено."
                }
            };

            var rand = new Random();
            var list = conclusions[healthStatus];
            return list[rand.Next(list.Count)];
        }

        private static (double pressureWarning, double pressureCritical, 
                        double tempWarning, double tempCritical, 
                        double vibWarning, double vibCritical) GetLimitsForEquipment(int equipmentId)
        {
            // Возвращаем разные лимиты для разных типов оборудования
            return equipmentId switch
            {
                1 or 2 => (7.0, 7.8, 85.0, 95.0, 3.5, 4.5),     // Поршневые компрессоры
                3 => (0.7, 0.85, 70.0, 80.0, 2.5, 3.5),         // Винтовой компрессор
                4 => (5.5, 6.0, 100.0, 110.0, 4.0, 5.0),        // Турбокомпрессор
                _ => (0.6, 0.8, 65.0, 75.0, 2.0, 3.0)           // Насосы и прочее
            };
        }
    }
}