using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
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
/// Contains unit tests for the <see cref="CancelSaleItemHandler"/> class.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _handler = new CancelSaleItemHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "Given active item When cancelling Then marks it cancelled, persists and publishes event")]
    public async Task Handle_ActiveItem_CancelsAndPublishesEvent()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        var item = sale.Items.First();
        var command = new CancelSaleItemCommand(sale.Id, item.Id);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        item.IsCancelled.Should().BeTrue();

        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishItemCancelledAsync(
            Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given unknown sale id When cancelling item Then throws not found exception")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given unknown item id When cancelling Then throws domain exception")]
    public async Task Handle_UnknownItem_ThrowsDomainException()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        var command = new CancelSaleItemCommand(sale.Id, Guid.NewGuid());
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
    }
}
