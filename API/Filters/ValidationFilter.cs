using API.Responses.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var firstError = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => x.Value?.Errors.FirstOrDefault()?.ErrorMessage)
                    .FirstOrDefault(msg => !string.IsNullOrWhiteSpace(msg));

                var message = firstError ?? "Validation failed";

                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null,
                    Errors = null
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}
