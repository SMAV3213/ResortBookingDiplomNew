using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Data.Options;

/// <summary>
/// Опции приложения.
/// </summary>
public interface IAppOptions
{
    /// <summary>
    /// Путь до корневой папки.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Путь до папки с дополнительными файлами.
    /// </summary>
    public string AdditionalFilesDirectoryPath { get; }

    /// <summary>
    /// Имя папки с дополнительными файлами.
    /// </summary>
    public string AdditionalFilesDirectoryName { get; }

    /// <summary>
    /// Разрешенные источники запросов.
    /// </summary>
    public List<string> CorsOrigins { get; }

    /// <summary>
    /// Инициализировать опции приложения.
    /// </summary>
    /// <param name="rootPath">Путь до корневой папки.</param>
    public void InitalizeOptions(string rootPath);
}
