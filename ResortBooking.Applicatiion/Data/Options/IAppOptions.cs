using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Data.Options;

/// <summary>
/// Интерфейс для опций приложения. Содержит все необходимые конфигурационные пути и настройки.
/// Реализуется в AppOptions (Infrastructure слой) и внедряется через DI контейнер.
/// 
/// Концепция: Dependency Inversion - зависимости внедряются через интерфейс, 
/// а не через конкретную реализацию. Это позволяет легко менять реализацию без изменения кода.
/// </summary>
public interface IAppOptions
{
    /// <summary>
    /// Путь до корневой папки (wwwroot).
    /// Используется для сохранения загруженных файлов и доступа к статическим ресурсам.
    /// Пример: C:\Project\ResortBooking.API\wwwroot
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Полный путь до папки с дополнительными файлами.
    /// Обычно это подпапка wwwroot, используется для фото номеров, документов и т.д.
    /// Пример: C:\Project\ResortBooking.API\wwwroot\additional-files
    /// </summary>
    public string AdditionalFilesDirectoryPath { get; }

    /// <summary>
    /// Имя папки для дополнительных файлов. Берется из appsettings.json
    /// Пример значения: "additional-files", "uploads", "media" и т.д.
    /// </summary>
    public string AdditionalFilesDirectoryName { get; }

    /// <summary>
    /// Список разрешенных источников для CORS (Cross-Origin Resource Sharing).
    /// CORS защищает от нежелательных запросов с других доменов.
    /// 
    /// Примеры значений:
    /// - ["https://localhost:3000"] - разработка
    /// - ["https://example.com", "https://www.example.com"] - production
    /// - ["*"] - разрешить всем (НЕ безопасно для production!)
    /// </summary>
    public List<string> CorsOrigins { get; }

    /// <summary>
    /// Инициализировать опции приложения при старте.
    /// 
    /// Этот метод должен быть вызван один раз в Program.cs:
    /// var appOptions = new AppOptions();
    /// appOptions.InitalizeOptions(app.Environment.ContentRootPath);
    /// 
    /// После инициализации все пути автоматически вычисляются на основе ContentRootPath.
    /// </summary>
    /// <param name="rootPath">Путь до корневой папки приложения (обычно ContentRootPath)</param>
    public void InitalizeOptions(string rootPath);
}
