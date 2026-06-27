FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["WebAPIDevSecOps/WebAPIDevSecOps.csproj", "WebAPIDevSecOps/"]
COPY ["UnitTest/UnitTest.csproj", "UnitTest/"]
COPY ["IntegrationTest/IntegrationTest.csproj", "IntegrationTest/"]
COPY ["SecurityTest/SecurityTest.csproj", "SecurityTest/"]
COPY ["WebAPIDevSecOps.slnx", "."]
RUN dotnet restore

COPY . .
RUN dotnet build -c Release --no-restore

FROM build AS test
RUN dotnet test UnitTest/UnitTest.csproj -c Release --no-build --verbosity normal
RUN dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --verbosity normal
RUN dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --verbosity normal

FROM build AS publish
RUN dotnet publish WebAPIDevSecOps/WebAPIDevSecOps.csproj -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "WebAPIDevSecOps.dll"]
