FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/IdentityService/MySchool.IdentityService/MySchool.IdentityService.csproj", "services/IdentityService/MySchool.IdentityService/"]
COPY ["shared/MySchool.Contracts/MySchool.Contracts.csproj", "shared/MySchool.Contracts/"]
COPY ["shared/MySchool.BuildingBlocks/MySchool.BuildingBlocks.csproj", "shared/MySchool.BuildingBlocks/"]

RUN dotnet restore "services/IdentityService/MySchool.IdentityService/MySchool.IdentityService.csproj"

COPY shared/ shared/
COPY services/IdentityService/MySchool.IdentityService/ services/IdentityService/MySchool.IdentityService/

WORKDIR /src/services/IdentityService/MySchool.IdentityService
RUN dotnet publish "MySchool.IdentityService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MySchool.IdentityService.dll"]
