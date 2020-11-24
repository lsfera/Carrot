$Network="benchmark"
$ComposeFile="src\benchmark.docker-compose.yml"
$Container="benchmark"

$env:Network=$Network

Write-Host "Creating network ${Network}"
docker network create $Network

Write-Host "Running benchmark"
docker-compose -f $ComposeFile up --build `
               --abort-on-container-exit `
               --exit-code-from $Container

Write-Host "Clean up"
docker-compose -f $ComposeFile down

Write-Host "Removing network ${Network}"
docker network rm $Network