# ================================
# Stage 1: Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
 
# Copy toàn bộ solution về container
COPY . .
 
# Restore đúng project (path tính từ root solution)
RUN dotnet restore "src\04.API\SupplyChain.WebApi.csproj"
 
# Build
RUN dotnet build "src\04.API\SupplyChain.WebApi.csproj" \
    -c Release \
    -o /app/build \
    --no-restore
 
# ================================
# Stage 2: Publish
# ================================
FROM build AS publish
RUN dotnet publish "src\04.API\SupplyChain.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false
 
# ================================
# Stage 3: Runtime (nhẹ hơn sdk)
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
 
COPY --from=publish /app/publish .
 
EXPOSE 5013
 
ENTRYPOINT ["dotnet", "SupplyChain.WebApi.dll"]