using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateSaleHandler(_saleRepository, _eventPublisher, _mapper);
    }

    [Fact(DisplayName = "Given existing sale When updating Then replaces items, persists, publishes event and returns mapped result")]
    public async Task Handle_ExistingSale_UpdatesAndPublishesEvent()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(sale.Id);
        var expectedResult = new SaleResult { Id = sale.Id, SaleNumber = sale.SaleNumber };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().BeSameAs(expectedResult);
        sale.Items.Should().ContainSingle(item => item.Product.Id == command.Items[0].ProductId);

        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishSaleModifiedAsync(
            Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given unknown sale id When updating Then throws not found exception")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidUpdateCommand();
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given a cancelled sale When updating Then throws domain exception")]
    public async Task Handle_CancelledSale_ThrowsDomainException()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        sale.Cancel();
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(sale.Id);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
    }
}
