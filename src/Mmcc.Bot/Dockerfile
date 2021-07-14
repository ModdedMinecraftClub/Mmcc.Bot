FROM mcr.microsoft.com/dotnet/runtime:5.0.4 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.201-buster-slim AS build
WORKDIR /src

COPY "Mmcc.Bot.sln" "Mmcc.Bot.sln"

COPY "Mmcc.Bot.Database/Mmcc.Bot.Database.csproj" "Mmcc.Bot.Database/Mmcc.Bot.Database.csproj"
COPY "Mmcc.Bot.Caching/Mmcc.Bot.Caching.csproj" "Mmcc.Bot.Caching/Mmcc.Bot.Caching.csproj"
COPY "Mmcc.Bot.Common/Mmcc.Bot.Common.csproj" "Mmcc.Bot.Common/Mmcc.Bot.Common.csproj"
COPY "Mmcc.Bot.Generators/Mmcc.Bot.Generators.csproj" "Mmcc.Bot.Generators/Mmcc.Bot.Generators.csproj"
COPY "Mmcc.Bot.RemoraAbstractions/Mmcc.Bot.RemoraAbstractions.csproj" "Mmcc.Bot.RemoraAbstractions/Mmcc.Bot.RemoraAbstractions.csproj"
COPY "Mmcc.Bot.Mojang/Mmcc.Bot.Mojang.csproj" "Mmcc.Bot.Mojang/Mmcc.Bot.Mojang.csproj"
COPY "Mmcc.Bot.Polychat/Mmcc.Bot.Polychat.csproj" "Mmcc.Bot.Polychat/Mmcc.Bot.Polychat.csproj"
COPY "Mmcc.Bot.Common.Extensions/Mmcc.Bot.Common.Extensions.csproj" "Mmcc.Bot.Common.Extensions/Mmcc.Bot.Common.Extensions.csproj"
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
