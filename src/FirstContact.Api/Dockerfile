# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files first, so restore is cached unless packages change.
COPY src/FirstContact.Domain/*.csproj         src/FirstContact.Domain/
COPY src/FirstContact.Application/*.csproj    src/FirstContact.Application/
COPY src/FirstContact.Infrastructure/*.csproj src/FirstContact.Infrastructure/
COPY src/FirstContact.Api/*.csproj            src/FirstContact.Api/
COPY Directory.Build.props .
RUN dotnet restore src/FirstContact.Api/FirstContact.Api.csproj

COPY src/ src/
RUN dotnet publish src/FirstContact.Api/FirstContact.Api.csproj -c Release -o /app --no-restore

# ---------- run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app .

# The SQLite file lives here, so it survives a restart when a volume is mounted.
ENV ConnectionStrings__Database="Data Source=/data/firstcontact.db"
RUN mkdir -p /data

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FirstContact.Api.dll"]
