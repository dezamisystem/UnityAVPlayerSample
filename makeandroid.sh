#!/bin/bash

source makeconfig.cfg

UNITY_APP_PATH="${UNITY_APP_FILE_PATH}/Contents/MacOS/Unity"
UNITY_BUILD_NAME="MakeBuilder.Make"
OUTPUT_PATH=$OUTPUT_PATH_ANDROID
BUILD_TARGET="android"
UNITY_LOG_PATH="${UNITY_LOG_DIR}/makeandroid.log"

MAKE_COMMAND="${UNITY_APP_PATH} -batchmode -quit -projectPath ${UNITY_PROJECT_PATH} -executeMethod ${UNITY_BUILD_NAME} -logFile ${UNITY_LOG_PATH} -output-path ${OUTPUT_PATH} -build-target ${BUILD_TARGET} ${UNITY_BUILD_VARIANT}"
echo "${MAKE_COMMAND}"
$MAKE_COMMAND

if [ $? -eq 1 ]; then
    echo "error! check logfile: ${UNITY_LOG_PATH}"
    exit 1
fi
echo "Success!"
exit 0
