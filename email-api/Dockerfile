# Build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore dependancies
COPY . .
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8083

 # Run the application
ENTRYPOINT ["dotnet", "email-api.dll"]