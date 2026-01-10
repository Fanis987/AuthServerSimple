# AuthServerSimple

A simple and robust authentication server built with .NET 10, designed to issue JWT tokens.   
It leverages ASP.NET Core Identity for user management and Entity Framework Core with Postgres DB for data persistence.  

## Features
- **User Registration & Login**: Full authentication flow using ASP.NET Core Identity.
- **JWT Token Issuance**: Secure token generation with customizable claims and expiration.
- **Role-Based Access Control**: Supports user roles embedded in JWT claims.

## Configuration and start-up
**WARNING**  
**ALL CREDIENTIALS IS THE PROJECT ARE FOR DEMONSTRATION PURPOSES ONLY**    
**ALWAYS STORE YOUR CREDENTIALS IN A SECURE ENVIRONMENT**    

The application can be configured via `appsettings.json` or environment variables. Key sections include:

### Connection Strings
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5400;Database=AuthServerSimpleDb;Username=postgresUser;Password=postgresPw"
}
```

### JWT Options
**The options needed to issue the JWT tokens on successful login**
```json
"JwtOptions": {
  "IssuerSigningKey": "Your-Secret-Key",
  "Issuer": "auth-server",
  "Audience" : "auth-client",
  "ExpiresInMinutes": 15
}
```

### Seed Options
**On startup, the app creates three roles (Support,Dev,Admin) and a corresponding user for each role, based on the provided passwords**
```json
"SeedOptions": {
    "SupportPassword": "SuppDemonstration123!@#",
    "DevPassword": "DevDemonstration123!@#",
    "AdminPassword" : "AdminDemonstration123!@#"
}
```

## Getting Started

1.  **Clone the repository**.
2.  **Update Connection String**: Ensure your PostgreSQL instance is running and update the `DefaultConnection` in `appsettings.Development.json`.
3.  **Run the Application (will also auto-apply DB migrations and entity seeding)**:
    ```bash
    dotnet run --project AuthServerSimple.Presentation.ServiceHost
    ```
Note: You can also create a fresh postgres DB with Docker:
```bash
docker run -d --name authserver-db -e POSTGRES_USER=postgresUser -e POSTGRES_PASSWORD=postgresPw -e POSTGRES_DB=AuthServerSimpleDb -p 5400:5432 postgres:latest
```

Note: You can adapt the port, 5400 is chosen to avoid conficts with other postres instance running on port 5432   
## API Endpoints

### Auth Controller (`/api/auth`)

- **POST `/register`**: Registers a new user.
- **POST `/login`**: Authenticates a user and returns a JWT token if successful.


## Project Structure
**Clean Architecture**: Organizes the solution into distinct layers (Application, Dtos, Infrastructure, Presentation)
- **AuthServerSimple.Presentation.API**: Contains the Web API controllers (e.g., `AuthController`).
- **AuthServerSimple.Presentation.ServiceHost**: The entry point of the application, handles configuration and DI.
- **AuthServerSimple.Application**: Core business logic, services (like `JwtTokenService`), and interfaces.
- **AuthServerSimple.Infrastructure.Identity**: Identity-specific implementation, DbContext, and migrations.
- **AuthServerSimple.Dtos**: Data Transfer Objects for requests and responses.
- **Tests**: Includes unit tests, using xUnit and FakeItEasy mocking lib, for various layers.
