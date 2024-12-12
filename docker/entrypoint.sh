#!/bin/bash
echo "Starting container..."
if [ -d "/App/config.json/" ]; then
    echo "Config.JSON is a directory, that indicates volume doesn't have anything to mount!"
    exit 1
elif [ ! -f "/App/config.json" ]; then
    echo "Config.JSON doesn't exist, that indicates there might be no volume pointing to the file!"
    exit 1
else
    echo "Config.JSON is fine"
fi

if [ -d "/App/version.txt/" ]; then
    echo "Version.TXT is a directory, that indicates volume doesn't have anything to mount!"
    exit 1
elif [ ! -f "/App/version.txt" ]; then
    echo "Version.TXT doesn't exist, that indicates there's no volume setup for it!"
    exit 1
else
    echo "Version.TXT is fine"
fi
/App/server