using AudioMetadataManager.UI.Services.AudioAnalysis.Interfaces;

namespace AudioMetadataManager.UI.Services.AudioAnalysis.Quality;

/// <summary>
/// Mantiene el conjunto de reglas disponibles para el motor
/// de evaluación técnica del audio.
///
/// El registro permite incorporar reglas predeterminadas o
/// personalizadas sin modificar AudioQualityAnalyzer.
///
/// No abre archivos, no procesa PCM y no ejecuta FFT.
/// </summary>
public class AudioQualityRuleRegistry
{
    private readonly List<IAudioQualityRule> _rules =
        new();

    /// <summary>
    /// Crea un registro vacío.
    /// </summary>
    public AudioQualityRuleRegistry()
    {
    }

    /// <summary>
    /// Crea un registro utilizando una colección inicial
    /// de reglas.
    /// </summary>
    public AudioQualityRuleRegistry(
        IEnumerable<IAudioQualityRule> rules)
    {
        ArgumentNullException.ThrowIfNull(
            rules);

        RegisterRange(
            rules);
    }

    /// <summary>
    /// Reglas registradas, ordenadas según su prioridad
    /// de ejecución.
    /// </summary>
    public IReadOnlyList<IAudioQualityRule> Rules =>
        _rules
            .OrderBy(
                rule =>
                    rule.Order)
            .ThenBy(
                rule =>
                    rule.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Cantidad de reglas registradas.
    /// </summary>
    public int Count =>
        _rules.Count;

    /// <summary>
    /// Indica si el registro contiene reglas.
    /// </summary>
    public bool HasRules =>
        _rules.Count > 0;

    /// <summary>
    /// Registra una regla evitando duplicados del mismo tipo.
    /// </summary>
    public bool Register(
        IAudioQualityRule rule)
    {
        ArgumentNullException.ThrowIfNull(
            rule);

        Type ruleType =
            rule.GetType();

        bool alreadyRegistered =
            _rules.Any(
                registeredRule =>
                    registeredRule.GetType() ==
                    ruleType);

        if (alreadyRegistered)
        {
            return false;
        }

        _rules.Add(
            rule);

        return true;
    }

    /// <summary>
    /// Registra varias reglas reutilizando la validación
    /// individual del registro.
    /// </summary>
    public int RegisterRange(
        IEnumerable<IAudioQualityRule> rules)
    {
        ArgumentNullException.ThrowIfNull(
            rules);

        int registeredCount = 0;

        foreach (
            IAudioQualityRule rule
            in rules)
        {
            if (rule is null)
            {
                continue;
            }

            if (Register(
                    rule))
            {
                registeredCount++;
            }
        }

        return registeredCount;
    }

    /// <summary>
    /// Comprueba si existe una regla del tipo solicitado.
    /// </summary>
    public bool Contains<TRule>()
        where TRule : class, IAudioQualityRule
    {
        return _rules.Any(
            rule =>
                rule is TRule);
    }

    /// <summary>
    /// Obtiene una regla registrada del tipo solicitado.
    /// </summary>
    public TRule? Find<TRule>()
        where TRule : class, IAudioQualityRule
    {
        return _rules
            .OfType<TRule>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Elimina una regla del tipo solicitado.
    /// </summary>
    public bool Remove<TRule>()
        where TRule : class, IAudioQualityRule
    {
        TRule? rule =
            Find<TRule>();

        if (rule is null)
        {
            return false;
        }

        return _rules.Remove(
            rule);
    }

    /// <summary>
    /// Elimina todas las reglas registradas.
    /// </summary>
    public void Clear()
    {
        _rules.Clear();
    }

    /// <summary>
    /// Construye el registro predeterminado de la aplicación.
    ///
    /// Las reglas se incorporarán aquí progresivamente
    /// conforme sean implementadas y verificadas.
    /// </summary>
    public static AudioQualityRuleRegistry CreateDefault()
    {
        AudioQualityRuleRegistry registry =
            new();

        registry.Register(
            new MetadataConsistencyRule());

        registry.Register(
            new SpectrumCutoffRule());

        registry.Register(
            new DeclaredBitrateConsistencyRule());

        registry.Register(
            new Mp3TranscodingEvidenceRule());

        return registry;
    }
}