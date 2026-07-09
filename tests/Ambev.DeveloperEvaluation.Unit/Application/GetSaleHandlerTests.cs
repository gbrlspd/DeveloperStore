using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHandler"/> class.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale id When getting sale Then returns mapped result")]
    public async Task Handle_ExistingSale_ReturnsMappedResult()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateExistingSale();
        var expectedResult = new SaleResult { Id = sale.Id, SaleNumber = sale.SaleNumber };
        var command = new GetSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().BeSameAs(expectedResult);
    }

    [Fact(DisplayName = "Given unknown sale id When getting sale Then throws not found exception")]
    public async Task Handle_UnknownSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new GetSaleCommand(Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
