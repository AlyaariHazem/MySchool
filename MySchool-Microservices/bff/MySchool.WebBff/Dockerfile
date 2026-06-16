FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["bff/MySchool.WebBff/MySchool.WebBff.csproj", "bff/MySchool.WebBff/"]
COPY ["shared/MySchool.Contracts/MySchool.Contracts.csproj", "shared/MySchool.Contracts/"]
COPY ["shared/MySchool.BuildingBlocks/MySchool.BuildingBlocks.csproj", "shared/MySchool.BuildingBlocks/"]
COPY ["bff/MySchool.WebBff/Protos/identity/v1/identity.proto", "bff/MySchool.WebBff/Protos/identity/v1/"]

RUN dotnet restore "bff/MySchool.WebBff/MySchool.WebBff.csproj"

COPY shared/ shared/
COPY bff/MySchool.WebBff/ bff/MySchool.WebBff/

WORKDIR /src/bff/MySchool.WebBff
RUN dotnet publish "MySchool.WebBff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MySchool.WebBff.dll"]
