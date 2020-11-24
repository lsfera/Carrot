FROM amd64/buildpack-deps:buster-curl as base

ENV \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true \
    # Telemetry opt-out
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    # Use invariant culture
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip

RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x dotnet-install.sh
RUN ./dotnet-install.sh -Channel 2.1 && \
    ./dotnet-install.sh -Channel 3.1 && \
    ./dotnet-install.sh -Channel 5.0

ENV PATH="/root/.dotnet:$PATH"

WORKDIR /src
COPY . ./

RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework net5.0
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework netcoreapp3.1
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework netcoreapp2.1

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "Carrot.Benchmarks.dll"]