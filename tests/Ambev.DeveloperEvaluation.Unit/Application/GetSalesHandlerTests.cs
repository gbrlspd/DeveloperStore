using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSalesHandler"/> class.
/// </summary>
public class GetSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSalesHandler _handler;

    public GetSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given a page of sales When listing Then returns mapped items and total count")]
    public async Task Handle_ValidRequest_ReturnsMappedPageAndTotalCount()
    {
        // Given
        var sales = new List<Sale> { SaleHandlerTestData.GenerateExistingSale(), SaleHandlerTestData.GenerateExistingSale() };
        var mappedResults = sales.Select(s => new SaleResult { Id = s.Id, SaleNumber = s.SaleNumber }).ToList();
        var command = new GetSalesCommand { Page = 1, Size = 10 };

        _saleRepository.GetPagedAsync(1, 10, null, null, Arg.Any<CancellationToken>())
            .Returns((sales, 2));
        _mapper.Map<IEnumerable<SaleResult>>(sales).Returns(mappedResults);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Sales.Should().BeEquivalentTo(mappedResults);
        result.TotalCount.Should().Be(2);
    }

    [Fact(DisplayName = "Given an invalid page size When listing Then throws validation exception")]
    public async Task Handle_InvalidSize_ThrowsValidationException()
    {
        // Given
        var command = new GetSalesCommand { Page = 1, Size = 0 };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }
}
