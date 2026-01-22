using ResortBooking.Application.Data.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Data.Options;

/// <summary>
/// Опции приложения.
/// </summary>
public class AppOptions : IAppOptions
{
    /// <summary>
    /// Путь до корневой папки.
    /// </summary>
    public string RootPath { get; private set; } = default!;

    /// <summary>
    /// Путь до папки с дополнительными файлами.
    /// </summary>
    public string AdditionalFilesDirectoryPath { get; private set; } = default!;

    /// <summary>
    /// Имя папки с дополнительными файлами.
    /// </summary>
    public string AdditionalFilesDirectoryName { get; set; } = default!;

    /// <summary>
    /// Разрешенные источники запросов.
    /// </summary>
    public List<string> CorsOrigins { get; set; } = [];

    /// <summary>
    /// Инициализировать опции приложения.
    /// </summary>
    /// <param name="rootPath">Путь до корневой папки.</param>
    public void InitalizeOptions(string rootPath)
    {
        RootPath = Path.Combine(rootPath, "wwwroot");
        AdditionalFilesDirectoryPath = Path.Combine(RootPath, AdditionalFilesDirectoryName);
    }
}
