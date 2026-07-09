using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteSaleHandler"/> class.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "Given existing sale id When deleting Then returns success response")]
    public async Task Handle_ExistingSale_ReturnsSuccess()
    {
        // Given
        var command = new DeleteSaleCommand(Guid.NewGuid());
        _saleRepository.DeleteAsync(command.Id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given unknown sale id When deleting Then throws not found exception")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new DeleteSaleCommand(Guid.NewGuid());
        _saleRepository.DeleteAsync(command.Id, Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
