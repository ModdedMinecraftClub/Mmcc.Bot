FROM mcr.microsoft.com/dotnet/runtime:5.0.4 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.201-buster-slim AS build
WORKDIR /src

COPY "Mmcc.Bot.sln" "Mmcc.Bot.sln"

COPY "Mmcc.Bot.Protos/Mmcc.Bot.Protos.csproj" "Mmcc.Bot.Protos/Mmcc.Bot.Protos.csproj"
COPY "Mmcc.Bot.Database/Mmcc.Bot.Database.csproj" "Mmcc.Bot.Database/Mmcc.Bot.Database.csproj"
COPY "Mmcc.Bot.Generators/Mmcc.Bot.Generators.csproj" "Mmcc.Bot.Generators/Mmcc.Bot.Generators.csproj"
COPY "Mmcc.Bot.Core/Mmcc.Bot.Core.csproj" "Mmcc.Bot.Core/Mmcc.Bot.Core.csproj"
COPY "Mmcc.Bot.Infrastructure/Mmcc.Bot.Infrastructure.csproj" "Mmcc.Bot.Infrastructure/Mmcc.Bot.Infrastructure.csproj"
COPY "Mmcc.Bot.Tools/Mmcc.Bot.Tools.DbConverter/Mmcc.Bot.Tools.DbConverter.csproj" "Mmcc.Bot.Tools/Mmcc.Bot.Tools.DbConverter/Mmcc.Bot.Tools.DbConverter.csproj"
COPY "Mmcc.Bot/Mmcc.Bot.csproj" "Mmcc.Bot/Mmcc.Bot.csproj"

COPY . .
WORKDIR /src/Mmcc.Bot
RUN dotnet restore
RUN dotnet publish --no-restore -c Release -o /app

FROM build as publish

FROM base as final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "dotnet", "Mmcc.Bot.dll" ]
