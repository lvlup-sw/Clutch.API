#!/bin/bash

# Create the Redis server
#docker run --name redis -p 6379:6379 -d redis
docker run --name redis -p 6379:6379 -d --network backend-network redis