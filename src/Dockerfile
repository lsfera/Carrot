from mcr.microsoft.com/dotnet/sdk:5.0

# Set environment variables
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true

WORKDIR /src
COPY . ./

#docker run -it --network benchmark --name benchmarkdotnet --privileged -e DOCKER_HOST=tcp://host.docker.internal:2375  benchmarkdotnet  -j long --filter *.DurableMessagesScenario.RunAsync -r netcoreapp50 --profiler EP

#RUN dotnet restore
#RUN dotnet build "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin
RUN dotnet publish "Carrot.Benchmarks/Carrot.Benchmarks.csproj" -c Release -o /src/bin/publish --framework net5.0

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "Carrot.Benchmarks.dll"]