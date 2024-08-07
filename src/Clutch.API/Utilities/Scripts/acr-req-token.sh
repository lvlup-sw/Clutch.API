#!/bin/bash

# Usage: ./get_acr_token.sh <tenant-id> <client-id> <client-secret> <acr-name>

TENANT_ID=$1
CLIENT_ID=$2
CLIENT_SECRET=$3
ACR_NAME=$4

# Request the access token
response=$(curl -X POST https://login.microsoftonline.com/$TENANT_ID/oauth2/v2.0/token \
-H "Content-Type: application/x-www-form-urlencoded" \
-d "client_id=$CLIENT_ID" \
-d "scope=https://$ACR_NAME.azurecr.io/.default" \
-d "client_secret=$CLIENT_SECRET" \
-d "grant_type=client_credentials")

# Extract the access token from the response
access_token=$(echo $response | jq -r .access_token)

echo "Access Token: $access_token"
