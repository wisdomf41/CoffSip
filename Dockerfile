# Builds the same ASP.NET Core Web API for local Docker and Render.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CoffSip.Api/CoffSip.Api.csproj", "CoffSip.Api/"]
COPY ["CoffSip.Application/CoffSip.Application.csproj", "CoffSip.Application/"]
COPY ["CoffSip.Domain/CoffSip.Domain.csproj", "CoffSip.Domain/"]
COPY ["CoffSip.Infrastructure/CoffSip.Infrastructure.csproj", "CoffSip.Infrastructure/"]
RUN dotnet restore "CoffSip.Api/CoffSip.Api.csproj"
COPY . .
RUN dotnet publish "CoffSip.Api/CoffSip.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "CoffSip.Api.dll"]
