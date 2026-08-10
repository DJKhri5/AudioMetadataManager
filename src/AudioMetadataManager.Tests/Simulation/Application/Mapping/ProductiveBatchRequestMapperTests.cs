using AudioMetadataManager.UI.Services.Simulation
    .Application.Models;
using AudioMetadataManager.UI.Views.Models.Simulation;
using AudioMetadataManager.UI.Views.Models.Simulation.Mapping;
using Xunit;

namespace AudioMetadataManager.Tests.Simulation.Application.Mapping;

public sealed class ProductiveBatchRequestMapperTests
{
    [Fact]
    public void NullSelection_IsRejected()
    {
        ProductiveBatchRequestMapper mapper =
            new();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!));
    }

    [Fact]
    public void EmptySelection_IsRejected()
    {
        ProductiveBatchSelection selection =
            new();

        ProductiveBatchRequestMapper mapper =
            new();

        Assert.Throws<InvalidOperationException>(
            () =>
                mapper.Map(
                    selection));
    }
}