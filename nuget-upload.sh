#!/bin/bash
for pkg in output/* ; do
  [ -e "$pkg" ] || continue
  nuget push "$pkg" "$NUGET_API_KEY" -Source "https://api.nuget.org/v3/index.json"
done
