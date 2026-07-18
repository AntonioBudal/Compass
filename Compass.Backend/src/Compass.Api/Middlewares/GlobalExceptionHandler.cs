using Compass.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Compass.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção interceptada pelo GlobalExceptionHandler: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Title = "Erro de Validação";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Detail = "Um ou mais campos enviados são inválidos.";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7807#section-3.1";

            // Agrupa os erros do FluentValidation por nome do campo
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            problemDetails.Extensions["errors"] = errors;
        }
        else if (exception is DomainException domainException)
        {
            problemDetails.Title = "Violação de Regra de Negócio";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Detail = domainException.Message;
            problemDetails.Type = "https://compass.app/errors/domain-rule";
        }
        else
        {
            problemDetails.Title = "Erro Interno no Servidor";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Detail = "Ocorreu um erro inesperado ao processar sua requisição.";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7807#section-3.1";
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}