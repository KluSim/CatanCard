#!/bin/sh
echo -ne '\033c\033]0;catan-client\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/catan-client.x86_64" "$@"
