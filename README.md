![Build Status](https://github.com/Traeger-GmbH/release-server/workflows/build/badge.svg?branch=master)

# Release server

An application for managing your own release artifacts. The release server provides several REST endpoints for the following operations:

- Upload the release artifacts
- Download the release artifacts
- Get information about the release artifacts (e.g. which version is the latest etc.)

## Setup

You have two options to setup the release server.

### Run with Docker-compose

1. Download the [docker-compose.yml](https://github.com/Traeger-GmbH/release-server/blob/master/docker-compose.yml) and put it in a directory of your choice.

2. Download the appsettings.json from the [configuration examples](https://github.com/Traeger-GmbH/release-server/tree/master/examples) and put it in the same directory of 1.

3. Replace the placeholder in the "Credentials" object in the appsettings.json with your own username and password.

4. Run the application with docker-compose: `docker-compose -f "docker-compose.yml" up -d`

Now, the release server is reachable at http://localhost:5001.
The Swagger UI can be found at http://localhost:5001/swagger.

### Run with Docker

1. Pull the Docker image: `docker pull traeger/release-server:latest`

2. Download the appsettings.json from the [configuration examples](https://github.com/Traeger-GmbH/release-server/tree/master/examples) and put it into a directory of your choice.

3. Replace the placeholder in the "Credentials" object in the appsettings.json with your own username and password.

4. Run the application with docker run: `docker run -d -v <path_to_your_directory>/appsettings.json:/app/appsettings.json -v <path_to_your_directory>/artifacts:/app/artifacts -v <path_to_your_directory>/backups:/app/backups -p 5001:5001  traeger/release-server:latest`
 
    __Note:__ You have to replace the placeholder of the docker run command!  

Now, the release server is reachable at http://localhost:5001.
The Swagger UI can be found at http://localhost:5001/swagger.

## Usage

The first two REST endpoint examples are documented in the following paragraphs. 

For more information about the other endpoints and their usage, read the [Swagger documentation](https://github.com/Traeger-GmbH/release-server/blob/master/Docs/api/swagger.json) .

### Uploading an Artifact

__Structure of the artifact payload:__

The payload has to be a zip file and has to contain the following elements:

- the artifact itself (exe, zip or something else)
- the meta information in form of the deployment.json
- a changelog

You can find an example artifact payload in form of a zip file here: [Example artifact payload](https://github.com/Traeger-GmbH/release-server/tree/master/examples)

__`PUT` request to upload an artifact__:

`curl -k -X PUT http://localhost:5000/artifacts/<product_name>/<operatingSystem>/<architecture>/<version> -H 'Authorization: Basic <username:password>' -H 'content-type: multipart/form-data' -F 'artifact=@<path/to/artifact-zipFile>;type=application/zip'`

> __Note:__ You have replace the placeholders in the `PUT` request (`<product_name>`, `<operatingSystem>`, `<architecture>`, `<version>`, `<username:password>`, `<path/to/artifact-zipFile>`). The value in the authorization header (`<username:password>`) has to be base64 encoded.

__Example response:__

Status: `200 OK`

__`PUT` request to upload a package__:

`curl -k -X PUT http://localhost:5000/artifacts/ -H 'Authorization: Basic <username:password>' -H 'content-type: multipart/form-data' -F 'package=@<path/to/package-zipFile>;type=application/zip'`

> __Note:__ You have replace the placeholders in the `PUT` request (`<username:password>` and `<path/to/artifact-zipFile>`). The value in the authorization header (`<username:password>`) has to be base64 encoded.

__Example response:__

Status: `200 OK`

### Listing all available deployments of a specific product

__`GET` request to list the deployments__:

`curl -k -X GET http://localhost:5000/artifacts/<product_name>/info`


__Example response:__

Status: `200 (OK)`

Body:

```json
{
  "identifier": "softwareX",
  "releases": [{
      "version": "2.0.0-preview-2",
      "platforms": [
        "linux/arm32",
        "linux/arm64",
        "linux/x64",
        "windows/arm64",
        "windows/x64",
        "windows/x86"
      ],
      "releaseDate": "2021-10-28",
      "isPreviewRelease": true,
      "isSecurityPatch": false,
      "changes": {
        "de": [
          {
            "platforms": null,
            "added": [
              
            ],
            "fixed": [
              
            ],
            "breaking": [
              
            ],
            "deprecated": [
              
            ]
          }
        ],
        "en": [
          {
            "platforms": null,
            "added": [
              
            ],
            "fixed": [
              
            ],
            "breaking": [
              
            ],
            "deprecated": [
              
            ]
          }
        ]
      }
    },
    {
      "version": "2.0.0",
      "platforms": [
        "linux/arm32",
        "linux/arm64",
        "linux/x64",
        "windows/arm64",
        "windows/x64",
        "windows/x86"
      ],
      "releaseDate": "2021-11-28",
      "isPreviewRelease": true,
      "isSecurityPatch": false,
      "changes": {
        "de": [
          {
            "platforms": null,
            "added": [
              
            ],
            "fixed": [
              
            ],
            "breaking": [
              
            ],
            "deprecated": [
              
            ]
          }
        ],
        "en": [
          {
            "platforms": null,
            "added": [
              
            ],
            "fixed": [
              
            ],
            "breaking": [
              
            ],
            "deprecated": [
              
            ]
          }
        ]
      }
    },
  ],
  "totalReleaseCount": 2,
  "nextOffset": null
}
```
