#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

rm -rf ./Upload
mkdir -p ./Upload/Data/Scripts
cp ./Scripts/UDSERebalance/*.cs ./Upload/Data/Scripts/
rsync -rt ./Data/ ./Upload/Data/
rsync -rt ./Assets/ ./Upload/