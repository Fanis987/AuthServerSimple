# AuthServerSimple

A server built with .NET 10, designed to issue JWT tokens for different roles and audiences.   
The project follows clean architecture, leveraging ASP.NET Core Identity for user management and Entity Framework Core with Postgres DB for data persistence.  

## Features
- **JWT Token Issuance**: Secure token generation with customizable traits.
- **User Registration**: Using ASP.NET Core Identity.
- **Role-Based Access Control**: Supports user roles embedded in JWT claims.
- **Support for multiple audiences**: Can issue token for different audiences.

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
Note: You can adapt the port, 5400 is chosen to avoid conficts with other postgres instances possibly running on port 5432

### JWT Options
**The options needed to issue the JWT tokens on token request**
```json
"JwtOptions": {
    "IssuerSigningKey": "3q2+7wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
    "Issuer": "generic-idp",
    "Audiences" :[
        "issue-tracker-endpoints",
        "some-other-endpoints"
    ],
"ExpiresInMinutes": 15
}
```

### Seed Options
**On startup, the app can optionally create three roles (Support,Dev,Admin) and a corresponding user for each role, based on the provided passwords**
```json
"SeedOptions": {
    "AddDefaults": true,
    "SupportPassword": "SuppDemonstration123!@#",
    "DevPassword": "DevDemonstration123!@#",
    "AdminPassword" : "AdminDemonstration123!@#"
}
```

## Getting Started

1.  **Clone the repository**.
2. **Run postgres instance via docker** (or normally)
   ```bash
   docker run -d --name authserver-db -e POSTGRES_USER=postgresUser -e POSTGRES_PASSWORD=postgresPw -e POSTGRES_DB=AuthServerSimpleDb -p 5400:5432 postgres:latest
   ```
3. **Update Connection String**: Ensure your PostgreSQL instance is running and update the `DefaultConnection` in `appsettings.Development.json`.
4. **Run the API** (will auto-apply DB migrations):
    ```bash
    dotnet run --project AuthServerSimple.Presentation.ServiceHost
    ```

## API Endpoints

### Auth Controller (`/api/auth`)

- **POST `/register`**: Registers a new user.
  - **Request Body**:
    ```json
    {
      "email": "user@example.com",
      "password": "YourSecurePassword123!",
      "role": "Dev"
    }
    ```
- **POST `/token`**: Authenticates a user and returns a JWT token if successful.
  - **Request Body**:
    ```json
    {
      "email": "user@example.com",
      "password": "YourSecurePassword123!",
      "audience": "your-api-endpoints"
    }
    ```
  - **Response Body** (Success):
    ```json
    {
      "isSuccess": true,
      "message": "Login successful",
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    }
    ```

### Role Controller (`/api/role`)

- **GET `/`**: Retrieves all existing roles.
  - **Response Body**:
    ```json
    [
      { "roleName": "Admin" },
      { "roleName": "Dev" }
    ]
    ```
- **POST `/`**: Creates a new role.
  - **Request Body**:
    ```json
    {
      "roleName": "NewRole"
    }
    ```
- **PUT `/`**: Updates an existing role name.
  - **Request Body**:
    ```json
    {
      "oldRoleName": "OldRole",
      "newRoleName": "UpdatedRole"
    }
    ```
- **DELETE `/{roleName}`**: Deletes a specific role.

### User Controller (`/api/user`)

- **GET `/`**: Retrieves all registered users.
  - **Response Body**:
    ```json
    [
      {
        "email": "user@example.com",
        "roles": ["Dev", "Support"]
      }
    ]
    ```