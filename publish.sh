#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

pandoc -f org -t ./vendor/2bbcode/bbcode_steam.lua <readme.org >|description.bb

description_size=$(wc -c < description.bb)
if [[ $description_size -gt 7999 ]]; then
  echo "Description size too big: $description_size. Limit 8000."
  exit 1
fi

MOD_ID_FULL=2552601155
MOD_ID_LITE=2758563789

if [[ ${1:-full} = "full" ]]; then
    MOD_ID="$MOD_ID_FULL"
else
    MOD_ID="$MOD_ID_LITE"
fi

cat << EOF >| mod.vdf
"workshopitem"
{
  "appid"             "244850"
  "publishedfileid"   "$MOD_ID"
  "contentfolder"     "$PWD/Upload"
  "description"       "$(cat $PWD/description.bb)"
}
EOF

"$(command -v steamcmd)" +login mkaito "${STEAMPASSWORD:-$(gopass show games/steam)}" +workshop_build_item "$(readlink -f ./mod.vdf)" +quit
