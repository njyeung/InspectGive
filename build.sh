#!/usr/bin/env bash
#
# Regenerates SkinNames.json from ByMykel/CSGO-API.
#
# Fetches the grouped skins list and flattens it to the compact
# {"<weapon_defindex>_<paint_index>": "<name>"} map that SkinNames.cs embeds
# and looks up at runtime (e.g. "10_1053": "FAMAS | Meltdown"). Wear and
# StatTrak/Souvenir are applied by SkinNames.Describe(), so the names here stay
# bare. Run before `dotnet build` to refresh the embedded lookup.
#
# Usage: ./build.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT="$SCRIPT_DIR/SkinNames.json"
SRC="https://raw.githubusercontent.com/ByMykel/CSGO-API/main/public/api/en/skins.json"

echo "Fetching skins from $SRC"
tmp="$(mktemp)"
trap 'rm -f "$tmp"' EXIT

curl --fail --silent --show-error --location "$SRC" -o "$tmp"

# Keep only entries that have both a weapon defindex and a paint index, key them
# "<weapon_id>_<paint_index>", and sort keys (-S) for stable diffs.
jq -S '
  map(select(.weapon.weapon_id != null and .paint_index != null and .name != null))
  | map({ key: "\(.weapon.weapon_id)_\(.paint_index)", value: .name })
  | from_entries
' "$tmp" > "$OUT"

echo "Wrote $(jq 'length' "$OUT") skins to $OUT"
