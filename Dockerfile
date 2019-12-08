FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine

WORKDIR /app/tests

# Source code for integration tests
COPY ./src ./src
COPY ./test/FluentMigrator.IntegrationTests ./test/FluentMigrator.IntegrationTests
COPY *.props ./
COPY FluentMigrator.snk .

# Scripts for dependency check
COPY ./scripts ./scripts

RUN chmod a+x ./scripts/wait-for-mysql.sh
# RUN find ./scripts -type f -exec chmod a+x {} \;

RUN dotnet restore test/FluentMigrator.IntegrationTests

# Cache
RUN dotnet build ./test/FluentMigrator.IntegrationTests

ENTRYPOINT [ "dotnet", "test", "./test/FluentMigrator.IntegrationTests", "--no-build", "-v=normal" ]
