#!/usr/bin/env bash
# Validates workflow_dispatch inputs (passed as env vars) against allow-list patterns.
# Usage: validate-dispatch-inputs.sh NAME...   (NAME? = the value may be empty)
# Never prints an input value: it could carry a workflow command or a newline.
set -euo pipefail
export LC_ALL=C

declare -A PATTERN=(
  [RESOURCE_GROUP]='^[A-Za-z0-9_][A-Za-z0-9_.()-]{0,88}[A-Za-z0-9_()-]$'
  [LOCATION]='^[a-z][a-z0-9]{1,39}$'
  [APP_NAME]='^[A-Za-z0-9][A-Za-z0-9-]{0,58}[A-Za-z0-9]$'
  [FUNCTION_APP_NAME]='^[A-Za-z0-9][A-Za-z0-9-]{0,58}[A-Za-z0-9]$'
  [STORAGE_NAME]='^[a-z0-9]{3,24}$'
  [CUSTOM_DOMAIN]='^([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,63}$'
  [MAX_INSTANCES]='^[1-9][0-9]{0,8}$'
  [DAILY_QUOTA_GB_SECONDS]='^[1-9][0-9]{0,8}$'
)
declare -A RULE=(
  [RESOURCE_GROUP]='2-90 characters: letters, digits, _ . ( ) -, starting with a letter, digit or _, not ending with .'
  [LOCATION]='2-40 lowercase letters and digits, starting with a letter (e.g. westeurope)'
  [APP_NAME]='2-60 characters: letters, digits and -, not starting or ending with -'
  [FUNCTION_APP_NAME]='2-60 characters: letters, digits and -, not starting or ending with -'
  [STORAGE_NAME]='3-24 lowercase letters and digits'
  [CUSTOM_DOMAIN]='a domain name such as example.com (up to 253 characters)'
  [MAX_INSTANCES]='a whole number from 1 to 999999999'
  [DAILY_QUOTA_GB_SECONDS]='a whole number from 1 to 999999999'
)

failed=0
for arg in "$@"; do
  name="${arg%\?}"
  optional=0
  [[ $arg == *\? ]] && optional=1
  if [[ -z ${PATTERN[$name]+x} ]]; then
    echo "Unknown input variable: $name" >&2
    exit 2
  fi
  value="${!name-}"
  if [[ -z $value ]]; then
    (( optional )) && continue
  elif [[ $value =~ ${PATTERN[$name]} ]]; then
    if [[ $name != CUSTOM_DOMAIN || ${#value} -le 253 ]]; then
      continue
    fi
  fi
  lower="$(printf '%s' "$name" | tr 'A-Z' 'a-z')"
  echo "::error title=Invalid input::$lower must be ${RULE[$name]}"
  failed=1
done
exit "$failed"
