using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sale operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of SalesController.
    /// </summary>
    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale, applying the quantity-based discount rules to each item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return ValidationBadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        var response = _mapper.Map<SaleResponse>(result);

        return Created("GetSaleById", new { id = response.Id }, response);
    }

    /// <summary>
    /// Retrieves a sale by its unique identifier.
    /// </summary>
    [HttpGet("{id}", Name = "GetSaleById")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<GetSaleCommand>(id);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<SaleResponse>(result));
    }

    /// <summary>
    /// Retrieves a paginated, ordered and filtered list of sales.
    /// </summary>
    /// <param name="page">1-based page number (query parameter "_page", default 1).</param>
    /// <param name="size">Number of items per page (query parameter "_size", default 10).</param>
    /// <param name="order">Comma-separated ordering expression (query parameter "_order").</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSales(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var command = new GetSalesCommand
        {
            Page = page,
            Size = size,
            Order = order,
            Filters = ExtractFilters()
        };

        var result = await _mediator.Send(command, cancellationToken);

        var items = _mapper.Map<List<SaleResponse>>(result.Sales.ToList());
        var pagedList = new PaginatedList<SaleResponse>(items, result.TotalCount, page, size);

        return OkPaginated(pagedList);
    }

    /// <summary>
    /// Updates an existing sale's details and items, re-applying the discount rules.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale(
        [FromRoute] Guid id,
        [FromBody] UpdateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return ValidationBadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<SaleResponse>(result));
    }

    /// <summary>
    /// Deletes a sale.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<DeleteSaleCommand>(id);
        await _mediator.Send(command, cancellationToken);

        return new OkObjectResult(new ApiResponse { Success = true, Message = "Sale deleted successfully" });
    }

    /// <summary>
    /// Cancels an entire sale.
    /// </summary>
    [HttpPatch("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CancelSaleCommand>(id);
        await _mediator.Send(command, cancellationToken);

        return new OkObjectResult(new ApiResponse { Success = true, Message = "Sale cancelled successfully" });
    }

    /// <summary>
    /// Cancels a single item within a sale.
    /// </summary>
    [HttpPatch("{id}/items/{itemId}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken)
    {
        var command = new CancelSaleItemCommand(id, itemId);
        await _mediator.Send(command, cancellationToken);

        return new OkObjectResult(new ApiResponse { Success = true, Message = "Item cancelled successfully" });
    }

    /// <summary>
    /// Extracts field=value filters from the query string, excluding the
    /// reserved pagination/ordering parameters (_page, _size, _order).
    /// </summary>
    private IDictionary<string, string> ExtractFilters()
    {
        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_page", "_size", "_order" };

        return Request.Query
            .Where(query => !reserved.Contains(query.Key))
            .ToDictionary(query => query.Key, query => query.Value.ToString());
    }

    private IActionResult ValidationBadRequest(IEnumerable<ValidationFailure> errors) =>
        base.BadRequest(new ApiResponse
        {
            Success = false,
            Message = "Validation Failed",
            Errors = errors.Select(error => (ValidationErrorDetail)error)
        });
}
