using System.Text;
using AudioMetadataManager.UI.Services.MetadataSources.Models;
using AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Execution;

namespace AudioMetadataManager.UI.Services.MetadataSources
    .Pipeline.Diagnostics;

/// <summary>
/// Genera informes legibles sobre la ejecución completa del
/// pipeline de búsqueda de metadatos.
///
/// No modifica archivos ni selecciona automáticamente
/// candidatos.
/// </summary>
public static class MetadataSearchPipelineDiagnostics
{
    private const int MaximumCandidatesPerSource = 10;

    /// <summary>
    /// Construye un informe completo del pipeline.
    /// </summary>
    public static string BuildReport(
        MetadataSearchPipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        StringBuilder builder =
            new();

        AppendHeader(
            builder);

        AppendGeneralInformation(
            builder,
            result);

        AppendAttempts(
            builder,
            result);

        AppendFinalSummary(
            builder,
            result);

        builder.AppendLine();
        builder.AppendLine(
            "=== Fin del diagnóstico del pipeline ===");

        return builder.ToString();
    }

    private static void AppendHeader(
        StringBuilder builder)
    {
        builder.AppendLine(
            "=== Diagnóstico de MetadataSearchPipeline ===");

        builder.AppendLine();
    }

    private static void AppendGeneralInformation(
        StringBuilder builder,
        MetadataSearchPipelineResult result)
    {
        builder.AppendLine(
            $"Id de ejecución: " +
            $"{result.Context.ExecutionId}");

        builder.AppendLine(
            $"Archivo: " +
            $"{result.Context.FileDisplayName}");

        builder.AppendLine(
            $"Consulta principal original: " +
            $"{result.Context.PrimaryQueryDisplay}");

        builder.AppendLine(
            $"Consulta alternativa original: " +
            $"{result.Context.AlternativeQueryDisplay}");

        builder.AppendLine(
            $"Ejecución completada: " +
            $"{ToSpanish(result.ExecutionSucceeded)}");

        builder.AppendLine(
            $"Motivo de detención: " +
            $"{GetStopReasonDisplay(result.StopReason)}");

        builder.AppendLine(
            $"Intentos ejecutados: " +
            $"{result.AttemptCount}");

        builder.AppendLine(
            $"Duración total: " +
            $"{result.ElapsedTime.TotalMilliseconds:0} ms");

        if (!string.IsNullOrWhiteSpace(
                result.ErrorMessage))
        {
            builder.AppendLine(
                $"Error general: " +
                $"{result.ErrorMessage}");
        }

        builder.AppendLine();
    }

    private static void AppendAttempts(
        StringBuilder builder,
        MetadataSearchPipelineResult result)
    {
        if (result.Attempts.Count == 0)
        {
            builder.AppendLine(
                "No se ejecutaron intentos de búsqueda.");

            builder.AppendLine();

            return;
        }

        builder.AppendLine(
            "--- Intentos ejecutados ---");

        builder.AppendLine();

        foreach (
            MetadataSearchAttempt attempt
            in result.Attempts)
        {
            AppendAttempt(
                builder,
                attempt);
        }
    }

    private static void AppendAttempt(
        StringBuilder builder,
        MetadataSearchAttempt attempt)
    {
        builder.AppendLine(
            $"Intento #{attempt.AttemptNumber}");

        builder.AppendLine(
            $"Tipo de consulta: " +
            $"{attempt.Query.Kind}");

        builder.AppendLine(
            $"Prioridad: " +
            $"{attempt.Query.Priority}");

        builder.AppendLine(
            $"Consulta: " +
            $"{attempt.Query.DisplayText}");

        builder.AppendLine(
            $"Motivo: " +
            $"{DisplayValue(attempt.Query.Reason)}");

        builder.AppendLine(
            $"Resultado: " +
            $"{GetAttemptOutcomeDisplay(attempt.Outcome)}");

        builder.AppendLine(
            $"Candidatos utilizables: " +
            $"{attempt.CandidateCount}");

        builder.AppendLine(
            $"Duración: " +
            $"{attempt.ElapsedTime.TotalMilliseconds:0} ms");

        builder.AppendLine(
            $"Mensaje: " +
            $"{DisplayValue(attempt.Message)}");

        builder.AppendLine();

        AppendSourceResults(
            builder,
            attempt.SourceResults);

        builder.AppendLine(
            "----------------------------------------");

        builder.AppendLine();
    }

    private static void AppendSourceResults(
        StringBuilder builder,
        IReadOnlyList<MetadataSearchResult> sourceResults)
    {
        if (sourceResults.Count == 0)
        {
            builder.AppendLine(
                "El intento no contiene resultados de fuentes.");

            builder.AppendLine();

            return;
        }

        foreach (
            MetadataSearchResult sourceResult
            in sourceResults)
        {
            AppendSourceResult(
                builder,
                sourceResult);
        }
    }

    private static void AppendSourceResult(
        StringBuilder builder,
        MetadataSearchResult sourceResult)
    {
        builder.AppendLine(
            $"Fuente: " +
            $"{DisplayValue(sourceResult.SourceName)}");

        builder.AppendLine(
            $"Estado: " +
            $"{sourceResult.Status}");

        builder.AppendLine(
            $"Consulta utilizada: " +
            $"{DisplayValue(sourceResult.QueryUsed)}");

        builder.AppendLine(
            $"Operación correcta: " +
            $"{ToSpanish(sourceResult.WasSuccessful)}");

        builder.AppendLine(
            $"Candidatos recibidos: " +
            $"{sourceResult.CandidateCount}");

        builder.AppendLine(
            $"Resultados externos totales: " +
            $"{sourceResult.ExternalTotalResults}");

        builder.AppendLine(
            $"Tiempo de respuesta: " +
            $"{sourceResult.ElapsedTime.TotalMilliseconds:0} ms");

        builder.AppendLine(
            $"Requiere aprobación manual: " +
            $"{ToSpanish(sourceResult.RequiresManualApproval)}");

        if (sourceResult.HttpStatusCode.HasValue)
        {
            builder.AppendLine(
                $"Estado HTTP: " +
                $"{sourceResult.HttpStatusCode.Value}");
        }

        if (sourceResult.RemainingRequests.HasValue)
        {
            builder.AppendLine(
                $"Solicitudes restantes: " +
                $"{sourceResult.RemainingRequests.Value}");
        }

        if (!string.IsNullOrWhiteSpace(
                sourceResult.ErrorMessage))
        {
            builder.AppendLine(
                $"Error: " +
                $"{sourceResult.ErrorMessage}");
        }

        builder.AppendLine();

        AppendCandidates(
            builder,
            sourceResult);
    }

    private static void AppendCandidates(
        StringBuilder builder,
        MetadataSearchResult sourceResult)
    {
        List<MetadataCandidate> validCandidates =
            sourceResult.Candidates
                .Where(
                    candidate =>
                        candidate.HasIdentity)
                .OrderBy(
                    candidate =>
                        candidate.SourceRank)
                .Take(
                    MaximumCandidatesPerSource)
                .ToList();

        if (validCandidates.Count == 0)
        {
            builder.AppendLine(
                "Candidatos: ninguno.");

            builder.AppendLine();

            return;
        }

        builder.AppendLine(
            $"Candidatos mostrados: " +
            $"{validCandidates.Count} de " +
            $"{sourceResult.CandidateCount}");

        builder.AppendLine();

        int displayPosition =
            0;

        foreach (
            MetadataCandidate candidate
            in validCandidates)
        {
            displayPosition++;

            AppendCandidate(
                builder,
                candidate,
                displayPosition);
        }

        if (sourceResult.CandidateCount >
            validCandidates.Count)
        {
            builder.AppendLine(
                $"Se omitieron " +
                $"{sourceResult.CandidateCount - validCandidates.Count} " +
                "candidato(s) adicionales del diagnóstico.");

            builder.AppendLine();
        }
    }

    private static void AppendCandidate(
        StringBuilder builder,
        MetadataCandidate candidate,
        int displayPosition)
    {
        builder.AppendLine(
            $"Candidato #{displayPosition}");

        builder.AppendLine(
            $"Procedencia: " +
            $"{DisplayValue(candidate.SourceDisplay)}");

        builder.AppendLine(
            $"Nombre: " +
            $"{DisplayValue(candidate.DisplayName)}");

        builder.AppendLine(
            $"Artista: " +
            $"{DisplayValue(candidate.Artist)}");

        builder.AppendLine(
            $"Título: " +
            $"{DisplayValue(candidate.Title)}");

        builder.AppendLine(
            $"Versión: " +
            $"{DisplayValue(candidate.Version)}");

        builder.AppendLine(
            $"Lanzamiento: " +
            $"{DisplayValue(candidate.ReleaseTitle)}");

        builder.AppendLine(
            $"Sello: " +
            $"{DisplayValue(candidate.Label)}");

        builder.AppendLine(
            $"Género: " +
            $"{DisplayValue(candidate.Genre)}");

        builder.AppendLine(
            $"Año: " +
            $"{DisplayYear(candidate.Year)}");

        builder.AppendLine(
            $"Duración: " +
            $"{DisplayDuration(candidate.Duration)}");

        builder.AppendLine(
            $"Posición en la fuente: " +
            $"{candidate.SourceRank}");

        builder.AppendLine(
            $"URL: " +
            $"{DisplayValue(candidate.SourceUrl)}");

        builder.AppendLine(
            $"Carátula disponible: " +
            $"{ToSpanish(candidate.HasArtwork)}");

        builder.AppendLine();
    }

    private static void AppendFinalSummary(
        StringBuilder builder,
        MetadataSearchPipelineResult result)
    {
        builder.AppendLine(
            "--- Resumen final ---");

        builder.AppendLine();

        builder.AppendLine(
            $"Fuentes procesadas en el último intento: " +
            $"{result.ProcessedSourceCount}");

        builder.AppendLine(
            $"Fuentes correctas: " +
            $"{result.SuccessfulSourceCount}");

        builder.AppendLine(
            $"Fuentes con error: " +
            $"{result.FailedSourceCount}");

        builder.AppendLine(
            $"Candidatos totales utilizables: " +
            $"{result.CandidateCount}");

        builder.AppendLine(
            $"Contiene candidatos con aprobación manual: " +
            $"{ToSpanish(result.ContainsManualApprovalCandidates)}");

        if (result.SuccessfulQuery is not null)
        {
            builder.AppendLine(
                $"Consulta que produjo candidatos: " +
                $"{result.SuccessfulQuery.DisplayText}");
        }

        builder.AppendLine(
            $"Resumen técnico: " +
            $"{result.Summary}");
    }

    private static string GetAttemptOutcomeDisplay(
        MetadataSearchAttemptOutcome outcome)
    {
        return outcome switch
        {
            MetadataSearchAttemptOutcome.CandidatesFound =>
                "Candidatos encontrados",

            MetadataSearchAttemptOutcome.NoCandidates =>
                "Sin candidatos",

            MetadataSearchAttemptOutcome.SourcesUnavailable =>
                "Fuentes no disponibles",

            MetadataSearchAttemptOutcome.InvalidRequest =>
                "Consulta no válida",

            MetadataSearchAttemptOutcome.AuthenticationFailure =>
                "Fallo de autenticación",

            MetadataSearchAttemptOutcome.RateLimited =>
                "Límite de solicitudes alcanzado",

            MetadataSearchAttemptOutcome.NetworkFailure =>
                "Fallo de red",

            MetadataSearchAttemptOutcome.InvalidResponse =>
                "Respuesta no válida",

            MetadataSearchAttemptOutcome.UnexpectedFailure =>
                "Error inesperado",

            _ =>
                "Resultado desconocido"
        };
    }

    private static string GetStopReasonDisplay(
        MetadataSearchStopReason stopReason)
    {
        return stopReason switch
        {
            MetadataSearchStopReason.CandidatesFound =>
                "Se encontraron candidatos",

            MetadataSearchStopReason.QueriesExhausted =>
                "Se agotaron las consultas disponibles",

            MetadataSearchStopReason.NoValidQueries =>
                "No se generaron consultas válidas",

            MetadataSearchStopReason.AuthenticationFailure =>
                "Fallo de autenticación",

            MetadataSearchStopReason.RateLimited =>
                "Límite de solicitudes alcanzado",

            MetadataSearchStopReason.NetworkFailure =>
                "Fallo de red",

            MetadataSearchStopReason.InvalidResponse =>
                "Respuesta externa no válida",

            MetadataSearchStopReason.UnexpectedFailure =>
                "Error inesperado",

            _ =>
                "Sin motivo definido"
        };
    }

    private static string DisplayValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
                ? "(sin información)"
                : value.Trim();
    }

    private static string DisplayYear(
        uint year)
    {
        return year == 0
            ? "(sin información)"
            : year.ToString();
    }

    private static string DisplayDuration(
        TimeSpan duration)
    {
        return duration <= TimeSpan.Zero
            ? "(sin información)"
            : duration.ToString(
                @"hh\:mm\:ss");
    }

    private static string ToSpanish(
        bool value)
    {
        return value
            ? "Sí"
            : "No";
    }
}