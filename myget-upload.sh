#!/bin/bash
for pkg in output/* ; do
  [ -e "$pkg" ] || continue
  nuget push "$pkg" "$MYGET_API_KEY" -Source "https://www.myget.org/F/fluent-migrator/api/v2/package"
done
