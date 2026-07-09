using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(ISaleRepository saleRepository, ISaleEventPublisher eventPublisher, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        sale.UpdateDetails(
            command.SaleDate,
            new CustomerReference(command.CustomerId, command.CustomerName),
            new BranchReference(command.BranchId, command.BranchName));

        sale.ClearItems();
        foreach (var item in command.Items)
            sale.AddItem(new ProductReference(item.ProductId, item.ProductName), item.Quantity, item.UnitPrice);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleModifiedAsync(
            new SaleModifiedEvent(updatedSale.Id, updatedSale.SaleNumber, DateTime.UtcNow),
            cancellationToken);

        return _mapper.Map<SaleResult>(updatedSale);
    }
}
