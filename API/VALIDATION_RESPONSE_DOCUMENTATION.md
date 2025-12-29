# Unified Validation Error Response System

## Overview
This API now implements a unified response format for ALL validation errors (ModelState validation, business logic validation, and general errors). This ensures consistent client-side error handling.

## Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* actual response data */ },
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "Human-readable error message",
  "data": null,
  "errors": null
}
```

## Implementation Details

### 1. ValidationFilter (API/Filters/ValidationFilter.cs)
- Intercepts all ModelState validation errors globally
- Extracts the FIRST meaningful validation error message
- Returns HTTP 400 with unified response format
- Applied automatically to all controller actions

### 2. Program.cs Configuration
```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
```

### 3. ApiResponse<T> Helper Class
Located in `API/Responses/Auth/ApiResponse.cs`, provides:
- `SuccessResponse(data, message)` - For successful operations
- `ErrorResponse(message, errors)` - For error responses

## Examples

### ModelState Validation Errors

**Request:**
```json
POST /api/auth/register
{
  "email": "invalid-email",
  "password": "123"
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "The Email field is not a valid e-mail address.",
  "data": null,
  "errors": null
}
```

### Business Logic Validation Errors

**Request:**
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "wrongpassword"
}
```

**Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid email or password",
  "data": null,
  "errors": null
}
```

### Successful Response

**Request:**
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "correctpassword"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abcd1234...",
    "user": {
      "id": 1,
      "email": "user@example.com",
      "fullName": "John Doe"
    }
  },
  "errors": null
}
```

## Validation Attribute Examples

Common validation attributes used in request models:

```csharp
public class RegisterRequest
{
    [Required]                           // -> "The FullName field is required."
    [StringLength(200, MinimumLength = 2)] // -> "The field FullName must be a string with a minimum length of 2 and a maximum length of 200."
    public string FullName { get; set; }

    [Required]
    [EmailAddress]                       // -> "The Email field is not a valid e-mail address."
    [StringLength(255)]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)] // -> "The field Password must be a string with a minimum length of 8 and a maximum length of 100."
    public string Password { get; set; }

    [Required]
    [Compare("Password")]                // -> "The field ConfirmPassword and Password do not match."
    public string ConfirmPassword { get; set; }
}
```

## Error Handling Flow

1. **ModelState Validation Errors** (400 Bad Request)
   - Handled by `ValidationFilter`
   - Triggered before controller action executes
   - Returns first validation error message

2. **Business Logic Errors** (400/401/404)
   - Handled in controller actions using try-catch
   - Uses `ApiResponse.ErrorResponse(message)`
   - Examples: "Invalid credentials", "User not found", "Email already exists"

3. **Unhandled Exceptions** (500 Internal Server Error)
   - Caught by `ErrorHandlingMiddleware`
   - Returns generic error message (no stack traces)
   - Logs detailed error for debugging

## Status Codes

| Status Code | Usage | Example |
|-------------|-------|---------|
| 200 OK | Successful operations | Login, registration, profile update |
| 400 Bad Request | Validation errors, business logic errors | Invalid input, weak password |
| 401 Unauthorized | Authentication errors | Invalid credentials, expired token |
| 404 Not Found | Resource not found | User not found, session not found |
| 500 Internal Server Error | Unexpected errors | Database errors, unhandled exceptions |

## Migration Guide for Frontend

### Before (Inconsistent):
```javascript
// Different error formats
if (response.status === 400) {
  // Sometimes: { errors: { Email: ["Invalid email"] } }
  // Sometimes: { message: "Invalid email" }
  // Sometimes: plain text
}
```

### After (Consistent):
```javascript
// Always the same format
const response = await fetch('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify(credentials)
});

const result = await response.json();

if (!result.success) {
  // Always available: result.message
  showError(result.message);
} else {
  // Always available: result.data
  handleSuccess(result.data);
}
```

## Testing

### Test ModelState Validation:
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"invalid","password":"short"}'
```

Expected Response:
```json
{
  "success": false,
  "message": "The Email field is not a valid e-mail address.",
  "data": null,
  "errors": null
}
```

### Test Business Logic Validation:
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"wrongpassword"}'
```

Expected Response:
```json
{
  "success": false,
  "message": "Invalid email or password",
  "data": null,
  "errors": null
}
```

## Production Considerations

? **Implemented:**
- No stack traces in error responses
- No internal details exposed
- Consistent error format across all endpoints
- HTTP status codes follow REST conventions
- First validation error returned (not all errors)

? **Security:**
- Generic error messages for authentication failures
- No enumeration attacks (same message for invalid email/password)
- Validation errors don't reveal system internals

? **Maintainability:**
- Central validation logic in `ValidationFilter`
- Reusable `ApiResponse` helper class
- Clear separation of concerns
