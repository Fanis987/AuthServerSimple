# AuthServerSimple

A server built with .NET 10, designed to issue JWT tokens for different roles and audiences.   
The project follows clean architecture, leveraging ASP.NET Core Identity for user management and Entity Framework Core
with Postgres DB for data persistence. This project aims to assist in prototyping apps, deployment to a real production
environment "as-is" is NOT encouraged.  

The default containerized version of the API serves port 5050 with Http. For a more realistic setup it is
advised to use a reverse proxy like nginx, which should also handle the TLS Termination.

## Features
- **JWT Token Issuance**: Secure token generation with customizable traits.
- **User Registration**: Using ASP.NET Core Identity.
- **Role-Based Access Control**: Supports user roles embedded in JWT claims.
- **Support for multiple audiences**: Can issue token for different audiences.

## Configuration and start-up
**WARNING**  
**ALL CREDIENTIALS IS THE PROJECT ARE FOR DEMONSTRATION PURPOSES ONLY**    
**ALWAYS STORE YOUR CREDENTIALS IN A SECURE ENVIRONMENT**    

The application can be configured via `appsettings.json` or `environment variables`, depending on the chosen setup

Key sections in `appsettings.json` include:

### Connection Strings
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5400;Database=AuthServerSimpleDb;Username=postgresUser;Password=postgresPw"
}
```
Note: Port 5400 is randomly chosen to avoid conficts with other postgres instances possibly running on port 5432

### JWT Options
**The options needed to issue the JWT tokens on token request**
```json
"JwtOptions": {
    "IssuerSigningKey": "3q2+7wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
    "Issuer": "generic-idp",
    "Audiences" :[
        "my-app-endpoints",
        "some-other-endpoints"
    ],
 "ExpiresInMinutes": 15
}
```
Note: Duration can be overwritten in the request to the `/token` endpoint

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

## Getting Started (Dotnet Devs)
Assuming **docker AND dotnet** installation in the target machine:

1.  **Clone the repository**.
2. **Run a postgres instance via docker**
   ```bash
   docker run -d --name authserver-db -e POSTGRES_USER=postgresUser -e POSTGRES_PASSWORD=postgresPw -e POSTGRES_DB=AuthServerSimpleDb -p 5400:5432 postgres:latest
   ```
3. **Update Connection String**: Ensure your PostgreSQL instance is running and update the `DefaultConnection` in `appsettings.Development.json`.
4.  **Run the API** (will auto-apply DB migrations):
    ```bash
    dotnet run --project AuthServerSimple.Presentation.ServiceHost
    ```
Note: You can also run it in **Development** mode via IDE such as **Rider** or **Visual Studio**

## Getting Started (Non-Dotnet Devs)
Assuming **ONLY docker** installation in the target machine:
1.  **Clone the repository**.
2. **Build a docker image** of the Auth Server. ( Will take several mins, depending on your machine)
    ```bash
   docker build -t authserversimple:latest .
   ```
3. Prepare a env file `.env.pseudo-prod` in the root directory. Example below:
    ```js
    ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=AuthServerDb;Username=postgres;Password=secret
    
    JwtOptions__IssuerSigningKey=base64-key-here
    JwtOptions__Issuer=generic-idp
    JwtOptions__Audiences__0=some-endpoints
    JwtOptions__ExpiresInMinutes=15
    
    SeedOptions__AddDefaults=false
    ```
4. Verify that your "authserversimple" image was built by running : `docker images`
5. Start containers of postgres & authserver API, using the `docker-compose.yml` file, by running :
    ```bash
       docker compose up
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
      "rememberMe": true,
      "audience": "your-api-endpoints",
      "DurationInMinutes" : 30,
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
      },
      {
        "Email": "test4@example.com",
        "roles": ["testRole"]
      }
    ]
    ```
