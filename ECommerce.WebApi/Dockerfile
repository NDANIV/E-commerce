# ---- Build ----
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Copia los 4 proyectos 
    COPY ./ECommerce.WebApi/ECommerce.WebApi.csproj ./ECommerce.WebApi/
    COPY ./ECommerce.Application/ECommerce.Application.csproj ./ECommerce.Application/
    COPY ./ECommerce.Infrastructure/ECommerce.Infrastructure.csproj ./ECommerce.Infrastructure/
    COPY ./ECommerce.Domain/ECommerce.Domain.csproj ./ECommerce.Domain/
    RUN dotnet restore ./ECommerce.WebApi/ECommerce.WebApi.csproj
    
    # Copia el resto del código
    COPY . .
    RUN dotnet publish ./ECommerce.WebApi/ECommerce.WebApi.csproj -c Release -o /app
    
    # ---- Runtime ----
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
    WORKDIR /app
    EXPOSE 8080
    ENV ASPNETCORE_URLS=http://+:8080
    COPY --from=build /app .
    ENTRYPOINT ["dotnet", "ECommerce.WebApi.dll"]