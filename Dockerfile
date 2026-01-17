# base layers with fundamental configurations (Ubuntu-based image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# to work with postgres
RUN apt-get update \
    && apt-get install -y libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/* \

WORKDIR /app
EXPOSE 5050

# on runtime some env variables are expected to be overwritten via the CLI
ENV ASPNETCORE_ENVIRONMENT=Production

# Seperate Stage for building 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONF=Production
WORKDIR "/src"

# copying ONLY csproj files to cache the dependencies in a seperate docker layer
COPY ["AuthServerSimple.Presentation.ServiceHost/AuthServerSimple.Presentation.ServiceHost.csproj", "AuthServerSimple.Presentation.ServiceHost/"]
COPY ["AuthServerSimple.Presentation.API/AuthServerSimple.Presentation.API.csproj"                , "AuthServerSimple.Presentation.API/"]
COPY ["AuthServerSimple.Infrastructure.Identity/AuthServerSimple.Infrastructure.Identity.csproj"  , "AuthServerSimple.Infrastructure.Identity/"]
COPY ["AuthServerSimple.Application/AuthServerSimple.Application.csproj"                          , "AuthServerSimple.Application/"]
COPY ["AuthServerSimple.Dtos/AuthServerSimple.Dtos.csproj"                                        , "AuthServerSimple.Dtos/"]
COPY ["AuthServerSimple.Domain/AuthServerSimple.Domain.csproj"                                    , "AuthServerSimple.Domain/"]
RUN dotnet restore "AuthServerSimple.Presentation.ServiceHost/AuthServerSimple.Presentation.ServiceHost.csproj"

# copying the rest of the source code and building in Production Mode
COPY . .
WORKDIR "/src/AuthServerSimple.Presentation.ServiceHost"
RUN dotnet build -c $BUILD_CONF -o /app/build

# New stage for publishing
FROM build AS publish
WORKDIR "/src/AuthServerSimple.Presentation.ServiceHost"
ARG BUILD_CONF=Release
RUN dotnet publish "AuthServerSimple.Presentation.ServiceHost.csproj" -c $BUILD_CONF -o /app/publish

# Final image to be deployed (starting from base image without the build leftovers)
FROM base AS final
WORKDIR /app
# bring only the contents of the publish folder to this prod image
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet","AuthServerSimple.Presentation.ServiceHost.dll"]