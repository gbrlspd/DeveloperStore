using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, ISaleEventPublisher eventPublisher, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = new Sale(
            command.SaleNumber,
            command.SaleDate,
            new CustomerReference(command.CustomerId, command.CustomerName),
            new BranchReference(command.BranchId, command.BranchName));

        foreach (var item in command.Items)
            sale.AddItem(new ProductReference(item.ProductId, item.ProductName), item.Quantity, item.UnitPrice);

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleCreatedAsync(
            new SaleCreatedEvent(createdSale.Id, createdSale.SaleNumber, DateTime.UtcNow),
            cancellationToken);

        return _mapper.Map<SaleResult>(createdSale);
    }
}
