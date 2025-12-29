# Dalilak API

A robust .NET 6 Web API application built with Clean Architecture principles, featuring secure authentication, chat services, and comprehensive user management capabilities.

---

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Authentication Flow](#authentication-flow)
- [Contributing](#contributing)

---

## Overview

Dalilak API is a production-ready ASP.NET Core Web API that implements modern authentication patterns and real-time communication features. The application follows Clean Architecture principles to ensure maintainability, testability, and separation of concerns.

**Built With:** .NET 6 | Entity Framework Core | JWT Authentication | SQL Server

---

## Key Features

### Authentication & User Management

- **User Registration** with email verification via OTP
- **Secure Login/Logout** using JWT token-based authentication
- **Token Management:**
  - Access tokens for API authorization
  - Refresh tokens for seamless session renewal
- **Password Operations:**
  - Change password (for authenticated users)
  - Forgot password with OTP verification
  - Reset password with secure token validation
- **Profile Management:**
  - Update user profile information
  - Delete user account with confirmation
- **Email Services:**
  - OTP generation and delivery
  - Password reset notifications
  - Account verification emails
  - Customizable email templates

### Chat Services

- **Session Management:**
  - Create new chat sessions
  - Rename existing sessions
  - Delete sessions and associated messages
- **Messaging:**
  - Send and receive messages within sessions
  - Retrieve message history
  - Paginated message results for performance
- **AI Integration:**
  - AI proxy endpoints for intelligent responses
  - Extensible chat functionality

---

## Architecture

This project implements Clean Architecture with clear separation of concerns across four layers:

```
Dalilak/
--- API/              # Presentation Layer (Controllers, Middleware, Filters)
--- Core/             # Domain Layer (Entities, Interfaces, DTOs)
--- Services/         # Business Logic Layer (Service Implementations)
--- Repository/       # Data Access Layer (EF Core, Database Context)
```

### Layer Responsibilities

**API Layer (Presentation)**
- Handles HTTP requests and responses
- API controllers and routing
- Request validation and filtering
- Authentication and authorization middleware
- Error handling middleware

**Core Layer (Domain)**
- Domain entities and models
- Business interfaces and contracts
- Data Transfer Objects (DTOs)
- Domain-specific logic and rules

**Services Layer (Business Logic)**
- Implementation of business interfaces
- Authentication and authorization logic
- Email and notification services
- Password hashing and validation
- JWT token generation and validation

**Repository Layer (Data Access)**
- Entity Framework Core database context
- Repository pattern implementations
- Database migrations
- Data access logic

---

## Technology Stack

- **.NET 6** - Application framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - Object-relational mapping (ORM)
- **SQL Server** - Relational database
- **JWT (JSON Web Tokens)** - Stateless authentication
- **Swagger/OpenAPI** - API documentation and testing
- **BCrypt/PBKDF2** - Password hashing (via PasswordService)

---

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- SQL Server (LocalDB, Express, or Full Edition)
- Visual Studio 2022, VS Code, or Rider
- Git (for version control)

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/KamalElsayedJR/DalilakAPI
   cd DalilakAPI
   ```

2. **Configure Database Connection**

   Open `API/appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DalilakDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Configure JWT Settings**

   In `appsettings.json`, set up JWT configuration:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "your-secure-secret-key-minimum-32-characters",
       "Issuer": "DalilakAPI",
       "Audience": "DalilakClient",
       "ExpirationInMinutes": 60,
       "RefreshTokenExpirationInDays": 7
     }
   }
   ```

4. **Configure Email Settings**

   Set up SMTP configuration for email services:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@example.com",
       "SenderPassword": "your-app-specific-password",
       "SenderName": "Dalilak Support",
       "EnableSsl": true
     }
   }
   ```
   
   > **Note:** For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) instead of your regular password.

5. **Apply Database Migrations**
   ```bash
   dotnet ef database update --project Repository --startup-project API
   ```

6. **Build the Solution**
   ```bash
   dotnet build
   ```

7. **Run the Application**
   ```bash
   cd API
   dotnet run
   ```

8. **Access Swagger UI**
   
   Open your browser and navigate to:
   ```
   https://localhost:5001
   ```
   or
   ```
   http://localhost:5000
   ```

---

## Project Structure

### API Project (Presentation Layer)

```
API/
--- Controllers/
-   --- Auth/
-   -   --- AuthController.cs          # Authentication endpoints
-   --- ChatController.cs              # Chat session and message endpoints
-   --- AIProxyController.cs           # AI integration endpoints
--- Middlewares/
-   --- JwtMiddleware.cs               # JWT token validation
-   --- ErrorHandlingMiddleware.cs     # Global exception handling
--- Filters/
-   --- ValidationFilter.cs            # Model validation filter
--- Requests/
-   --- Auth/                          # Request DTOs for authentication
--- Responses/
-   --- Auth/                          # Response DTOs
--- Extensions/
-   --- ServiceCollectionExtensions.cs # Dependency injection configuration
--- Program.cs                         # Application entry point
```

### Core Project (Domain Layer)

```
Core/
--- Entities/
-   --- Auth/
-   -   --- User.cs                    # User entity
-   -   --- RefreshToken.cs            # Refresh token entity
-   -   --- EmailOtpToken.cs           # Email OTP entity
-   --- Chat/
-       --- ChatSession.cs             # Chat session entity
-       --- ChatMessage.cs             # Chat message entity
--- Interfaces/
-   --- Auth/
-   -   --- IAuthService.cs            # Authentication service contract
-   -   --- IAuthRepository.cs         # Authentication repository contract
-   -   --- IJwtService.cs             # JWT service contract
-   -   --- IPasswordService.cs        # Password service contract
-   -   --- IEmailService.cs           # Email service contract
-   --- Chat/
-   -   --- IChatService.cs            # Chat service contract
-   -   --- IChatRepository.cs         # Chat repository contract
-   --- Common/
-       --- IFileService.cs            # File service contract
--- DTOs/                              # Data Transfer Objects
    --- Auth/
    --- Chat/
```

### Services Project (Business Logic Layer)

```
Services/
--- Implementations/
-   --- Auth/
-   -   --- AuthService.cs             # Authentication business logic
-   --- Chat/
-   -   --- ChatService.cs             # Chat business logic
-   --- Common/
-       --- FileService.cs             # File operations
--- Helpers/
-   --- Auth/
-       --- JwtService.cs              # JWT generation and validation
-       --- PasswordService.cs         # Password hashing and verification
-       --- EmailService.cs            # Email sending functionality
-       --- EmailTemplateService.cs    # Email template management
--- Utilities/
    --- OtpGenerator.cs                # OTP generation utility
```

### Repository Project (Data Access Layer)

```
Repository/
--- Context/
-   --- DalilakDbContext.cs          # EF Core database context
--- Repositories/
-   --- Auth/
-   -   --- AuthRepository.cs          # Authentication data access
-   --- Chat/
-       --- ChatRepository.cs          # Chat data access
--- Migrations/                        # EF Core migrations
```

---

## API Documentation

Once the application is running, access the interactive Swagger UI documentation at:

```
https://localhost:7258/
```

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user account | No |
| POST | `/api/auth/verify-email` | Verify email address with OTP | No |
| POST | `/api/auth/resend-otp` | Resend verification OTP | No |
| POST | `/api/auth/login` | Authenticate and receive tokens | No |
| POST | `/api/auth/refresh-token` | Refresh access token | No |
| POST | `/api/auth/forgot-password` | Request password reset | No |
| POST | `/api/auth/verify-reset-otp` | Verify password reset OTP | No |
| POST | `/api/auth/reset-password` | Reset password with verified OTP | No |
| PUT | `/api/auth/change-password` | Change password | Yes |
| PUT | `/api/auth/update-profile` | Update user profile | Yes |
| DELETE | `/api/auth/delete-account` | Delete user account | Yes |

### Chat Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/chat/sessions` | Retrieve all chat sessions | Yes |
| POST | `/api/chat/sessions` | Create a new chat session | Yes |
| PUT | `/api/chat/sessions/{id}/rename` | Rename an existing session | Yes |
| DELETE | `/api/chat/sessions/{id}` | Delete a session | Yes |
| GET | `/api/chat/sessions/{id}/messages` | Get messages for a session | Yes |
| POST | `/api/chat/send` | Send a message in a session | Yes |

---

## Configuration

### Required Settings

All configuration settings are stored in `appsettings.json` and `appsettings.Development.json`.

#### 1. Database Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DalilakDb;Trusted_Connection=True;"
  }
}
```

#### 2. JWT Configuration

```json
{
  "JwtSettings": {
    "SecretKey": "minimum-32-character-secret-key-for-production",
    "Issuer": "DalilakAPI",
    "Audience": "DalilakClient",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

> **Security Note:** Use a strong, randomly generated secret key (minimum 256 bits) in production.

#### 3. Email Configuration

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@Dalilak.com",
    "SenderPassword": "secure-app-password",
    "SenderName": "Dalilak Support",
    "EnableSsl": true
  }
}
```

#### 4. CORS Configuration (Optional)

CORS can be configured in `ServiceCollectionExtensions.cs` if your API will be accessed from web applications:

```csharp
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://your-frontend-domain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

---

## Authentication Flow

The API implements a secure JWT-based authentication flow with refresh token support:

### Registration & Verification Flow

1. **User Registration**
   - User submits registration details via `POST /api/auth/register`
   - System validates input and creates a pending user account
   - OTP is generated and sent to the user's email

2. **Email Verification**
   - User receives OTP via email
   - User submits OTP via `POST /api/auth/verify-email`
   - System validates OTP and activates the account

### Login Flow

3. **User Login**
   - User submits credentials via `POST /api/auth/login`
   - System validates credentials and returns:
     - **Access Token** (JWT) - Short-lived token for API authorization
     - **Refresh Token** - Long-lived token for obtaining new access tokens

4. **Making Authenticated Requests**
   - Include the access token in the Authorization header:
     ```
     Authorization: Bearer <your-access-token>
     ```

5. **Token Refresh**
   - When the access token expires, use the refresh token
   - Submit refresh token via `POST /api/auth/refresh-token`
   - Receive a new access token and refresh token pair

### Password Reset Flow

6. **Forgot Password**
   - User requests password reset via `POST /api/auth/forgot-password`
   - System generates OTP and sends it to registered email

7. **Verify Reset OTP**
   - User submits OTP via `POST /api/auth/verify-reset-otp`
   - System validates OTP and returns a reset token

8. **Reset Password**
   - User submits new password with reset token via `POST /api/auth/reset-password`
   - System updates password and invalidates reset token

---

## Contributing

This project is currently maintained by the author.  
Contributions are welcome through pull requests after discussion.

1. **Fork the Repository**
   ```bash
   git clone https://github.com/KamalElsayedJR/DalilakAPI
   ```

2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make Your Changes**
   - Write clean, maintainable code
   - Follow existing code style and conventions
   - Add comments where necessary

4. **Test Your Changes**
   - Ensure all existing tests pass
   - Add new tests for new features
   - Run the application locally to verify

5. **Commit Your Changes**
   ```bash
   git commit -m "Add: Brief description of your changes"
   ```

6. **Push to Your Fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Open a Pull Request**
   - Provide a clear description of your changes
   - Wait for code review and feedback

---

## Authors

- **[Kamal Elsayed]** - Backend Developer (.NET)

---

## Contact

For questions, issues, or suggestions, please contact:

- **Email:** kamalelsayeddev@gmail.com
- **GitHub Profile:** https://github.com/KamalElsayedJR

---

## Important Notes

- **Security:** Never commit sensitive configuration values (passwords, secret keys, connection strings) to version control
- **Environment Variables:** Consider using environment variables or Azure Key Vault for production secrets
- **Database Migrations:** Always back up your database before applying migrations in production
- **HTTPS:** Ensure HTTPS is enforced in production environments
- **Error Handling:** The API uses global error handling middleware for consistent error responses

---

**Made with .NET 6 | Built for scalability and security**
