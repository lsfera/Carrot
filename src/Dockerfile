FROM amd64/buildpack-deps:buster-curl as base

RUN wget https://dot.net/v1/dotnet-install.sh && chmod +x dotnet-install.sh
RUN ./dotnet-install.sh -Channel 2.1 && \
    ./dotnet-install.sh -Channel 3.1 && \
    ./dotnet-install.sh -Channel 5.0

ENV PATH="/root/.dotnet:$PATH"
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE true
ENV DOTNET_CLI_TELEMETRY_OPTOUT true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT true

WORKDIR /src
COPY . ./

RUN dotnet restore "Carrot.Benchmarks/Carrot.Benchmarks.csproj"
RUN dotnet build "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework netcoreapp2.1
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework netcoreapp3.1
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework net5.0

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "Carrot.Benchmarks.dll"]