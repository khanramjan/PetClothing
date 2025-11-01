FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/PetClothingShop.API/PetClothingShop.API.csproj", "PetClothingShop.API/"]
COPY ["backend/PetClothingShop.Core/PetClothingShop.Core.csproj", "PetClothingShop.Core/"]
COPY ["backend/PetClothingShop.Infrastructure/PetClothingShop.Infrastructure.csproj", "PetClothingShop.Infrastructure/"]
RUN dotnet restore "PetClothingShop.API/PetClothingShop.API.csproj"
COPY backend/. .
WORKDIR "/src/PetClothingShop.API"
RUN dotnet build "PetClothingShop.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetClothingShop.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetClothingShop.API.dll"]
