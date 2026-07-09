using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleHandler"/> class.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _handler = new CancelSaleHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then marks it cancelled, persists and publishes event")]
    public async Task Handle_ActiveSale_CancelsAndPublishesEvent()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();

        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishSaleCancelledAsync(
            Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given unknown sale id When cancelling Then throws not found exception")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new CancelSaleCommand(Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given an already cancelled sale When cancelling Then throws domain exception")]
    public async Task Handle_AlreadyCancelledSale_ThrowsDomainException()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        sale.Cancel();
        var command = new CancelSaleCommand(sale.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
    }
}
