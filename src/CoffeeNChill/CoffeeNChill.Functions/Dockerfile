FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY CoffeeNChill.Functions.csproj .

RUN dotnet restore CoffeeNChill.Functions.csproj

COPY . .

RUN dotnet publish CoffeeNChill.Functions.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore


FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS runtime

WORKDIR /home/site/wwwroot

COPY --from=build /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot

ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true

ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated