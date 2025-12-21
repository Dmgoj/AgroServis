using AgroServis.Services.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occured: {Message}", exception.Message);

            var statusCode = StatusCodes.Status500InternalServerError;
            var title = "Server Error";
            var detail = "An unexpected error occurred.";

            switch (exception)
            {
                case EntityNotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Resource Not Found";
                    detail = notFoundEx.Message;

                    _logger.LogWarning(
                       "Entity not found: {EntityName} with ID {EntityId}",
                       notFoundEx.EntityName,
                       notFoundEx.EntityId);
                    break;

                case DuplicateEntityException duplicateEx:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Duplicate Serial Number";
                    detail = duplicateEx.Message;

                    _logger.LogWarning("Cannot create equipment - serial number {SerialNumber} already exists", duplicateEx.SerialNumber);
                    break;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problemDetails.Status.Value;

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}