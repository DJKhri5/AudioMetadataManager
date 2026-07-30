using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Configuration;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Dtos;
using AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Models;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Providers.Chromaprint.Execution;

/// <summary>
/// Invoca el ejecutable externo fpcalc para generar la huella
/// acústica de un archivo local.
///
/// Solo genera la huella. La consulta a la API pública de
/// AcoustID se implementará en una fase posterior.
/// </summary>
public sealed class ChromaprintFingerprintExecutor
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

    private readonly ChromaprintOptions
        _options;

    public ChromaprintFingerprintExecutor(
        ChromaprintOptions options)
    {
        _options =
            options ??
            throw new ArgumentNullException(
                nameof(options));
    }

    /// <summary>
    /// Genera la huella acústica del archivo indicado.
    /// </summary>
    public async Task<ChromaprintFingerprintResult> ExecuteAsync(
        ChromaprintFingerprintRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        if (!request.HasFilePath)
        {
            return ChromaprintFingerprintResult.InvalidRequest(
                request.FilePath,
                "La solicitud no contiene una ruta de archivo.");
        }

        if (!File.Exists(
                request.FilePath))
        {
            return ChromaprintFingerprintResult.InvalidRequest(
                request.FilePath,
                $"El archivo '{request.FilePath}' no existe.");
        }

        if (!_options.IsValid)
        {
            return ChromaprintFingerprintResult.InvalidConfiguration(
                request.FilePath,
                "La configuración de Chromaprint no es válida.");
        }

        ProcessStartInfo startInfo =
            new(_options.ExecutablePath)
            {
                UseShellExecute =
                    false,

                CreateNoWindow =
                    true,

                RedirectStandardOutput =
                    true,

                RedirectStandardError =
                    true
            };

        startInfo.ArgumentList.Add(
            "-json");

        startInfo.ArgumentList.Add(
            request.FilePath);

        using Process process =
            new()
            {
                StartInfo =
                    startInfo
            };

        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return ChromaprintFingerprintResult.ExecutableNotFound(
                request.FilePath,
                _options.ExecutablePath);
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                or PlatformNotSupportedException)
        {
            return ChromaprintFingerprintResult.ExecutableNotFound(
                request.FilePath,
                _options.ExecutablePath);
        }

        using CancellationTokenSource timeoutSource =
            new(
                _options.Timeout);

        using CancellationTokenSource linkedSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutSource.Token);

        Task<string> standardOutputTask =
            process.StandardOutput.ReadToEndAsync(
                cancellationToken);

        Task<string> standardErrorTask =
            process.StandardError.ReadToEndAsync(
                cancellationToken);

        try
        {
            await process.WaitForExitAsync(
                linkedSource.Token);
        }
        catch (OperationCanceledException)
        {
            TryKill(
                process);

            Forget(
                standardOutputTask);

            Forget(
                standardErrorTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return new ChromaprintFingerprintResult
                {
                    Status =
                        ChromaprintStatus.Cancelled,

                    FilePath =
                        request.FilePath,

                    Message =
                        "La generación de la huella fue cancelada."
                };
            }

            return new ChromaprintFingerprintResult
            {
                Status =
                    ChromaprintStatus.Timeout,

                FilePath =
                    request.FilePath,

                Message =
                    $"fpcalc no terminó dentro de " +
                    $"{_options.Timeout.TotalSeconds:0} segundos."
            };
        }

        string standardOutput =
            await standardOutputTask;

        string standardError =
            await standardErrorTask;

        if (process.ExitCode != 0)
        {
            return new ChromaprintFingerprintResult
            {
                Status =
                    ChromaprintStatus.ProcessError,

                FilePath =
                    request.FilePath,

                ExitCode =
                    process.ExitCode,

                Message =
                    string.IsNullOrWhiteSpace(
                        standardError)
                        ? $"fpcalc terminó con el código " +
                          $"{process.ExitCode}."
                        : standardError.Trim()
            };
        }

        return ParseOutput(
            request.FilePath,
            standardOutput,
            process.ExitCode);
    }

    private static ChromaprintFingerprintResult ParseOutput(
        string filePath,
        string standardOutput,
        int exitCode)
    {
        if (string.IsNullOrWhiteSpace(
                standardOutput))
        {
            return new ChromaprintFingerprintResult
            {
                Status =
                    ChromaprintStatus.InvalidOutput,

                FilePath =
                    filePath,

                ExitCode =
                    exitCode,

                Message =
                    "fpcalc no devolvió ninguna salida."
            };
        }

        try
        {
            ChromaprintProcessOutputDto? output =
                JsonSerializer.Deserialize<
                    ChromaprintProcessOutputDto>(
                        standardOutput,
                        SerializerOptions);

            if (output is null ||
                string.IsNullOrWhiteSpace(
                    output.Fingerprint))
            {
                return new ChromaprintFingerprintResult
                {
                    Status =
                        ChromaprintStatus.InvalidOutput,

                    FilePath =
                        filePath,

                    ExitCode =
                        exitCode,

                    Message =
                        "La salida de fpcalc no contiene una huella."
                };
            }

            return new ChromaprintFingerprintResult
            {
                Status =
                    ChromaprintStatus.Success,

                FilePath =
                    filePath,

                Fingerprint =
                    output.Fingerprint,

                Duration =
                    TimeSpan.FromSeconds(
                        output.Duration),

                ExitCode =
                    exitCode,

                Message =
                    "La huella acústica se generó correctamente."
            };
        }
        catch (JsonException exception)
        {
            return new ChromaprintFingerprintResult
            {
                Status =
                    ChromaprintStatus.InvalidOutput,

                FilePath =
                    filePath,

                ExitCode =
                    exitCode,

                Message =
                    "La salida de fpcalc no pudo interpretarse: " +
                    exception.Message
            };
        }
    }

    private static void TryKill(
        Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(
                    entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    /// <summary>
    /// Observa el resultado de una tarea descartada para evitar
    /// excepciones no observadas cuando el proceso se cancela
    /// o expira antes de terminar de leer su salida.
    /// </summary>
    private static void Forget(
        Task<string> task)
    {
        _ = task.ContinueWith(
            static completedTask => _ = completedTask.Exception,
            TaskContinuationOptions.OnlyOnFaulted |
            TaskContinuationOptions.ExecuteSynchronously);
    }
}
