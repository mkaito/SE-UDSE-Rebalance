#!/usr/bin/bash
set -euo pipefail
IFS=$'\n\t'

rm -rf ./Upload
mkdir -p ./Upload/Data/Scripts/UDSERebalance
cp ./Scripts/UDSERebalance/*.cs ./Upload/Data/Scripts/UDSERebalance
rsync -rt ./Data/ ./Upload/Data/
rsync -rt ./Assets/ ./Upload/
