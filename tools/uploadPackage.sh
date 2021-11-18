#!/bin/bash

print_help()
{
  echo "
  Usage:  bash uploadPackage.sh -f <PACKAGE_FILE> -u <USER> -p <PASSWORD> -d <RELEASE_SERVER_URL>

    Parameters:
      - PACKAGE_FILE:   The path of the zip file that contains the package to upload.
                            (required: yes)
      - USER:               User to use for authentication.
                            (required: yes)
      - PASSWORD:           Password to use for authentication.
                            (required: yes)
      - RELEASE_SERVER_URL: URL of the release server to upload the package to.
                            (required: yes)
  "
}

while getopts ":f:u:p:d:h" opt; do
  case $opt in
    f) PACKAGE_FILE="$OPTARG"
    ;;
    u) USER="$OPTARG"
    ;;
    p) PASSWORD="$OPTARG"
    ;;
    d) RELEASE_SERVER_URL="$OPTARG"
    ;;
    h) print_help; exit
    ;;
    \?) echo "
  Invalid option \"-$OPTARG\"">&2; print_help; exit
    ;;
  esac
done

if [ -z "$PACKAGE_FILE" ]
then
  echo "Required parameter <PACKAGE_FILE> not set."
  print_help
  exit
fi

if [ -z "$USER" ]
then
  echo "Required parameter <USER> not set."
  print_help
  exit
fi

if [ -z "$PASSWORD" ]
then
  echo "Required parameter <PASSWORD> not set."
  print_help
  exit
fi

if [ -z "$RELEASE_SERVER_URL" ]
then
  echo "Required parameter <RELEASE_SERVER_URL> not set."
  print_help
  exit
fi

if [ ! -f "$PACKAGE_FILE" ]
then
  echo "Package file could not be found under \"$PACKAG_FILE\"."
  exit
fi

UPLOAD_URL="$RELEASE_SERVER_URL/artifacts"
AUTHORIZATION_TOKEN=$(echo -n "$USER:$PASSWORD" | base64)

curl -k -X PUT $UPLOAD_URL -H "Authorization: Basic $AUTHORIZATION_TOKEN" -H 'content-type: multipart/form-data' -F "package=@$PACKAGE_FILE;type=application/zip"
