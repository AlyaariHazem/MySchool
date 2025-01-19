# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj and other project files
COPY *.csproj .
COPY *.config ./

# Restore the .NET project
RUN dotnet restore

# Copy the rest of your source code
COPY . .

# Build the .NET application
RUN dotnet publish -c Release -o out

# Stage 2: Run the application in a smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app/out ./

# Expose port 80 for the container
EXPOSE 80

# Set the entry point to run your application
ENTRYPOINT ["dotnet", "ServerApp.dll"]
