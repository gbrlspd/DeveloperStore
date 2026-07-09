using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _eventPublisher, _mapper);
    }

    [Fact(DisplayName = "Given valid command When creating sale Then persists, publishes event and returns mapped result")]
    public async Task Handle_ValidCommand_CreatesSaleAndPublishesEvent()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        var expectedResult = new SaleResult { SaleNumber = command.SaleNumber };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().BeSameAs(expectedResult);

        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleNumber == command.SaleNumber && s.Items.Count == command.Items.Count),
            Arg.Any<CancellationToken>());

        await _eventPublisher.Received(1).PublishSaleCreatedAsync(
            Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given command without items When creating sale Then throws validation exception without touching the repository")]
    public async Task Handle_NoItems_ThrowsValidationException()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items = [];

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an item quantity above 20 When creating sale Then throws validation exception")]
    public async Task Handle_QuantityAboveLimit_ThrowsValidationException()
    {
        // Given: CreateSaleValidator already rejects quantities above 20, so this never
        // reaches the domain -- it's caught one layer earlier than a raw DomainException.
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        command.Items[0].Quantity = 21;

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }
}
