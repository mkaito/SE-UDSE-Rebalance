#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

rm -rf ./Upload
mkdir -p ./Upload/Data/Scripts
cp ./Scripts/UDSERebalance/*.cs ./Upload/Data/Scripts/
rsync -rt ./Data/ ./Upload/Data/
rsync -rt ./Assets/ ./Upload/

cat << EOF >| ./mod.vdf
"workshopitem"
{
	"appid"					"244850"
	"publishedfileid"		"2552601155"
	"contentfolder"			"$(readlink -f ./Upload)"
	"previewfile"			"$(readlink -f ./Upload/preview.jpg)"
	"visibility"			"3"
	"title"					"UDSE Rebalance"
	"changenote"			"N/A"
}
EOF