using ResortBooking.Application.Data.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Data.Options;

/// <summary>
/// Опции приложения. Хранит конфигурацию путей и CORS настроек.
/// Инициализируется при запуске приложения в Program.cs
/// </summary>
public class AppOptions : IAppOptions
{
    /// <summary>
    /// Путь до корневой папки (wwwroot).
    /// Вычисляется в InitalizeOptions как: {ContentRootPath}/wwwroot
    /// </summary>
    public string RootPath { get; private set; } = default!;

    /// <summary>
    /// Полный путь до папки с дополнительными файлами (фото, документы и т.д.).
    /// Вычисляется как: {RootPath}/{AdditionalFilesDirectoryName}
    /// Примеры: wwwroot/additional-files/ или wwwroot/uploads/
    /// </summary>
    public string AdditionalFilesDirectoryPath { get; private set; } = default!;

    /// <summary>
    /// Имя папки с дополнительными файлами (берется из appsettings.json).
    /// По умолчанию: "additional-files"
    /// </summary>
    public string AdditionalFilesDirectoryName { get; set; } = default!;

    /// <summary>
    /// Разрешенные источники запросов для CORS.
    /// Примеры: ["https://localhost:3000", "https://example.com"]
    /// Если пусто, CORS отключен.
    /// </summary>
    public List<string> CorsOrigins { get; set; } = [];

    /// <summary>
    /// Инициализировать опции приложения при старте.
    /// Этот метод вызывается в Program.cs и вычисляет все необходимые пути.
    /// </summary>
    /// <param name="rootPath">Путь до корневой папки приложения (ContentRootPath)</param>
    public void InitalizeOptions(string rootPath)
    {
        // Вычисляем путь до wwwroot папки
        // wwwroot - папка для статических файлов (CSS, JS, фото и т.д.)
        RootPath = Path.Combine(rootPath, "wwwroot");

        // Вычисляем полный путь до папки с дополнительными файлами
        // Наприклад: C:\Project\wwwroot\additional-files
        AdditionalFilesDirectoryPath = Path.Combine(RootPath, AdditionalFilesDirectoryName);
    }
}
