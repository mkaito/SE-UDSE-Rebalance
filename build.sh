#!/usr/bin/bash
set -euo pipefail
IFS=$'\n\t'

echo Building...

rm -rf ./Upload
mkdir -p ./Upload/Data/Scripts/UDSERebalance
cp ./Scripts/UDSERebalance/*.cs ./Upload/Data/Scripts/UDSERebalance
cp -r ./Scripts/UDSERebalance/Utilities ./Upload/Data/Scripts/UDSERebalance
cp -r ./Scripts/UDSERebalance/ConfigData ./Upload/Data/Scripts/UDSERebalance
rsync --include='*.sbc' --include='*/' --exclude='*' -rt ./Data/ ./Upload/Data/
rsync -rt ./Assets/ ./Upload/

echo Done