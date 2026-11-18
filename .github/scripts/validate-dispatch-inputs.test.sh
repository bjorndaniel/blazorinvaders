#!/usr/bin/env bash
# Table-driven tests for validate-dispatch-inputs.sh. Run: bash .github/scripts/validate-dispatch-inputs.test.sh
set -uo pipefail
dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
script="$dir/validate-dispatch-inputs.sh"
pass=0
fail=0

# check <case> <expected exit> <expected error names, space separated> <secret value> VAR=value... -- NAME...
check() {
  local label="$1" want="$2" names="$3" secret="$4"
  shift 4
  local envs=()
  while [[ $1 != -- ]]; do envs+=("$1"); shift; done
  shift
  local out code=0
  out="$(env -u RESOURCE_GROUP -u LOCATION -u APP_NAME -u STORAGE_NAME -u FUNCTION_APP_NAME -u CUSTOM_DOMAIN -u MAX_INSTANCES -u DAILY_QUOTA_GB_SECONDS "${envs[@]}" bash "$script" "$@" 2>&1)" || code=$?
  local ok=1
  [[ $code -eq $want ]] || ok=0
  if [[ $want -eq 1 ]]; then
    local expected=0 line n hits total=0
    for n in $names; do
      expected=$((expected + 1))
      hits=0
      while IFS= read -r line; do
        [[ $line == "::error title=Invalid input::$n must be "* ]] && hits=$((hits + 1))
      done <<<"$out"
      [[ $hits -eq 1 ]] || ok=0
    done
    while IFS= read -r line; do
      [[ -z $line ]] || total=$((total + 1))
    done <<<"$out"
    [[ $total -eq $expected ]] || ok=0
  elif [[ $want -eq 0 && -n $out ]]; then
    ok=0
  fi
  if [[ ${#secret} -ge 4 && $out == *"$secret"* ]]; then ok=0; fi
  if (( ok )); then pass=$((pass + 1)); else fail=$((fail + 1)); echo "FAIL: $label (exit $code, want $want)"; fi
}

D=(RESOURCE_GROUP=blazorinvaders-rg LOCATION=westeurope APP_NAME=blazorinvaders
   STORAGE_NAME=blazorinvadersstore FUNCTION_APP_NAME=blazorinvaders-api CUSTOM_DOMAIN=blazorinvaders.bjorndaniel.se)
ALL=(RESOURCE_GROUP LOCATION APP_NAME STORAGE_NAME FUNCTION_APP_NAME CUSTOM_DOMAIN)
rep() { local s=""; local i; for ((i = 0; i < $2; i++)); do s+="$1"; done; printf '%s' "$s"; }

check "defaults" 0 "" "" "${D[@]}" -- "${ALL[@]}"
check "empty domain optional" 0 "" "" "${D[@]}" CUSTOM_DOMAIN= -- RESOURCE_GROUP 'CUSTOM_DOMAIN?'
l63="$(rep a 63)"; l64="$(rep a 64)"
check "63-char label" 0 "" "" CUSTOM_DOMAIN="$l63.com" -- CUSTOM_DOMAIN
check "storage 3 chars" 0 "" "" STORAGE_NAME=abc -- STORAGE_NAME
check "storage 24 chars" 0 "" "" STORAGE_NAME="$(rep a 24)" -- STORAGE_NAME

dom_bad=('-x.com' '--hostname=evil.com' 'example.com.' '*.example.com' 'https://example.com'
  'a.com;id' 'a.com$(id)' $'a.com\nb.com' "$l64.com" 'localhost' 'exämple.com')
for v in "${dom_bad[@]}"; do
  check "domain rejected: ${v:0:20}" 1 custom_domain "$v" CUSTOM_DOMAIN="$v" -- CUSTOM_DOMAIN
done
# total length 254: three 62-char labels, one 61-char label, ".com"
l62="$(rep a 62)"; long="$l62.$l62.$l62.$(rep a 61).com"
check "domain length 254 (len ${#long})" 1 custom_domain "$long" CUSTOM_DOMAIN="$long" -- CUSTOM_DOMAIN
check "empty domain required" 1 custom_domain "" CUSTOM_DOMAIN= -- CUSTOM_DOMAIN
check "storage ab" 1 storage_name "" STORAGE_NAME=ab -- STORAGE_NAME
check "storage 25 chars" 1 storage_name "$(rep a 25)" STORAGE_NAME="$(rep a 25)" -- STORAGE_NAME
check "storage uppercase" 1 storage_name "BlazorStore" STORAGE_NAME=BlazorStore -- STORAGE_NAME
check "rg leading dash" 1 resource_group "-rg" RESOURCE_GROUP=-rg -- RESOURCE_GROUP
check "rg trailing dot" 1 resource_group "" RESOURCE_GROUP=rg. -- RESOURCE_GROUP
check "app leading dash" 1 app_name "-app" APP_NAME=-app -- APP_NAME
check "app trailing dash" 1 app_name "" APP_NAME=app- -- APP_NAME
check "function app leading dash" 1 function_app_name "-app" FUNCTION_APP_NAME=-app -- FUNCTION_APP_NAME
check "function app trailing dash" 1 function_app_name "" FUNCTION_APP_NAME=app- -- FUNCTION_APP_NAME
check "location lowercase with space" 1 location "" "LOCATION=west europe" -- LOCATION
rg90="$(rep a 90)"; rg91="$(rep a 91)"
check "rg 90 chars" 0 "" "" RESOURCE_GROUP="$rg90" -- RESOURCE_GROUP
check "rg 91 chars" 1 resource_group "$rg91" RESOURCE_GROUP="$rg91" -- RESOURCE_GROUP
check "location with space" 1 location "West Europe" "LOCATION=West Europe" -- LOCATION
check "unset variable" 1 location "" -- LOCATION
check "two invalid inputs" 1 "location app_name" "" LOCATION=-x APP_NAME=-y -- LOCATION APP_NAME
check "rg trailing dash" 0 "" "" RESOURCE_GROUP=rg- -- RESOURCE_GROUP
for n in MAX_INSTANCES DAILY_QUOTA_GB_SECONDS; do
  lname="$(printf '%s' "$n" | tr 'A-Z' 'a-z')"
  for v in 1 10000 999999999; do
    check "$n accepts $v" 0 "" "" "$n=$v" -- "$n"
  done
  for v in 0 01 -1 1.5 ' 1' 1e4 '' 1000000000; do
    check "$n rejects '$v'" 1 "$lname" "" "$n=$v" -- "$n"
  done
done
check "unknown variable" 2 "" "" FOO=bar -- FOO

echo "$pass passed, $fail failed"
(( fail == 0 ))
