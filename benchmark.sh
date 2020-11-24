#!/bin/bash

network=benchmark
compose_file=src/benchmark.docker-compose.yml
container=benchmark

export Network=$network

echo "Creating network ${Network}"
docker network create $network

echo "Running benchmark"
docker-compose -f $compose_file up --build \
               --abort-on-container-exit \
               --exit-code-from $container

echo "Clean up"
docker-compose -f $compose_file down

echo "Removing network ${Network}"
docker network rm $network