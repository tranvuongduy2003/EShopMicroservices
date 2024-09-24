using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Behaviours;

public class ValidationBehaviour<TRequet, TResponse>(IEnumerable<IValidator<TRequet>> validators)
    : IPipelineBehavior<TRequet, TResponse>
    where TRequet : ICommand<TRequet>
{
    public async Task<TResponse> Handle(TRequet request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequet>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(x => x.Errors.Any())
            .SelectMany(x => x.Errors)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}