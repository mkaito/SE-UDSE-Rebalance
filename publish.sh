#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

rm -rf ./Upload
mkdir ./Upload
rsync -avL ./Output/ ./Upload/

steamcmd +login mkaito "$(gopass games/steam)" +workshop_build_item "$(readlink -f ./mod.vdf)" +quit
