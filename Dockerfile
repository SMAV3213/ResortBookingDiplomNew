# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["ResortBooking.API/ResortBooking.API.csproj", "ResortBooking.API/"]
COPY ["ResortBooking.Applicatiion/ResortBooking.Application.csproj", "ResortBooking.Applicatiion/"]
COPY ["ResortBooking.Infrastructure/ResortBooking.Infrastructure.csproj", "ResortBooking.Infrastructure/"]
COPY ["ResortBooking.Domain/ResortBooking.Domain.csproj", "ResortBooking.Domain/"]

# Restore dependencies
RUN dotnet restore "ResortBooking.API/ResortBooking.API.csproj"

# Copy remaining source code
COPY . .

# Build application
WORKDIR "/src/ResortBooking.API"
RUN dotnet build "ResortBooking.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ResortBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "ResortBooking.API.dll"]
