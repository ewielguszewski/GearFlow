FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Api/GearFlow.Api/GearFlow.Api.csproj src/Api/GearFlow.Api/
COPY src/Shared/GearFlow.Shared.Abstractions/GearFlow.Shared.Abstractions.csproj src/Shared/GearFlow.Shared.Abstractions/
COPY src/Shared/GearFlow.Shared.Infrastructure/GearFlow.Shared.Infrastructure.csproj src/Shared/GearFlow.Shared.Infrastructure/
COPY src/Modules/Availability/GearFlow.Modules.Availability.Application/GearFlow.Modules.Availability.Application.csproj src/Modules/Availability/GearFlow.Modules.Availability.Application/
COPY src/Modules/Availability/GearFlow.Modules.Availability.Contracts/GearFlow.Modules.Availability.Contracts.csproj src/Modules/Availability/GearFlow.Modules.Availability.Contracts/
COPY src/Modules/Availability/GearFlow.Modules.Availability.Core/GearFlow.Modules.Availability.Core.csproj src/Modules/Availability/GearFlow.Modules.Availability.Core/
COPY src/Modules/Availability/GearFlow.Modules.Availability.Infrastructure/GearFlow.Modules.Availability.Infrastructure.csproj src/Modules/Availability/GearFlow.Modules.Availability.Infrastructure/
COPY src/Modules/Catalog/GearFlow.Modules.Catalog.Application/GearFlow.Modules.Catalog.Application.csproj src/Modules/Catalog/GearFlow.Modules.Catalog.Application/
COPY src/Modules/Catalog/GearFlow.Modules.Catalog.Contracts/GearFlow.Modules.Catalog.Contracts.csproj src/Modules/Catalog/GearFlow.Modules.Catalog.Contracts/
COPY src/Modules/Catalog/GearFlow.Modules.Catalog.Domain/GearFlow.Modules.Catalog.Domain.csproj src/Modules/Catalog/GearFlow.Modules.Catalog.Domain/
COPY src/Modules/Catalog/GearFlow.Modules.Catalog.Infrastructure/GearFlow.Modules.Catalog.Infrastructure.csproj src/Modules/Catalog/GearFlow.Modules.Catalog.Infrastructure/
COPY src/Modules/Reservations/GearFlow.Modules.Reservations.Application/GearFlow.Modules.Reservations.Application.csproj src/Modules/Reservations/GearFlow.Modules.Reservations.Application/
COPY src/Modules/Reservations/GearFlow.Modules.Reservations.Domain/GearFlow.Modules.Reservations.Domain.csproj src/Modules/Reservations/GearFlow.Modules.Reservations.Domain/
COPY src/Modules/Reservations/GearFlow.Modules.Reservations.Infrastructure/GearFlow.Modules.Reservations.Infrastructure.csproj src/Modules/Reservations/GearFlow.Modules.Reservations.Infrastructure/

RUN dotnet restore src/Api/GearFlow.Api/GearFlow.Api.csproj

COPY . .

RUN dotnet publish src/Api/GearFlow.Api/GearFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "GearFlow.Api.dll"]
