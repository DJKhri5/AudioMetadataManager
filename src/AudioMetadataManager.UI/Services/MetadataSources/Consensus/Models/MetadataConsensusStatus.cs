namespace AudioMetadataManager.UI.Services.MetadataSources
    .Consensus.Models;

/// <summary>
/// Describe la conclusión alcanzada por el motor de consenso
/// para un campo individual.
/// </summary>
public enum MetadataConsensusStatus
{
    /// <summary>
    /// El campo todavía no ha sido evaluado.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Varias fuentes propusieron valores equivalentes y se
    /// alcanzó un consenso suficientemente sólido.
    /// </summary>
    ConsensusReached = 1,

    /// <summary>
    /// Sólo existe una propuesta utilizable.
    ///
    /// El valor puede conservarse como candidato, pero todavía
    /// no constituye consenso entre varias fuentes.
    /// </summary>
    SingleSource = 2,

    /// <summary>
    /// Existen varias propuestas y una de ellas obtuvo una
    /// ventaja suficiente para ser seleccionada.
    /// </summary>
    MajorityReached = 3,

    /// <summary>
    /// Existen propuestas incompatibles y ninguna obtuvo una
    /// ventaja suficiente.
    /// </summary>
    Conflict = 4,

    /// <summary>
    /// Ninguna fuente entregó un valor utilizable.
    /// </summary>
    NoInformation = 5,

    /// <summary>
    /// El campo no corresponde a la evaluación actual.
    /// </summary>
    NotApplicable = 6
}