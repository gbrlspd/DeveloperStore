using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    /// <summary>
    /// Translates exceptions raised by the Application/Domain layers into the
    /// standard ApiResponse envelope, so every error looks the same regardless
    /// of where it was thrown from.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "Validation Failed",
                    ex.Errors.Select(error => (ValidationErrorDetail)error));
            }
            catch (DomainException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message, []);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, ex.Message, []);
            }
        }

        private static Task WriteResponseAsync(
            HttpContext context,
            int statusCode,
            string message,
            IEnumerable<ValidationErrorDetail> errors)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
