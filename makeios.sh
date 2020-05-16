#!/bin/bash

UNITY_APP_PATH="/Applications/Unity/Hub/Editor/2019.2.21f1/Unity.app/Contents/MacOS/Unity"
UNITY_PROJECT_PATH="./"
UNITY_BUILD_NAME="MakeBuilder.iOS"
UNITY_LOG_PATH="makeios.log"
PROJECT_DIR="~/Downloads"

$UNITY_APP_PATH -batchmode -quit -projectPath $UNITY_PROJECT_PATH -executeMethod $UNITY_BUILD_NAME -logFile $UNITY_LOG_PATH
# -output-dir $PROJECT_DIR

if [ $? -eq 1 ]; then
    echo "error! check logfile: ${UNITY_LOG_PATH}"
    exit 1
fi
echo "Success!"
exit 0
