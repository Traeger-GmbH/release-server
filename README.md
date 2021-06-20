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

The first two REST endpoint examples are documented above. 

For more information about the other endpoints and their usage, read the [Swagger documentation](https://github.com/Traeger-GmbH/release-server/blob/master/Docs/api/swagger.json) .

### Upload an Artifact

__Structure of the artifact payload:__ The payload has to be a zip file and has to contain the following elements:

- the artifact itself (exe, zip or something else)
- the meta information in form of the deployment.json
- a changelog

You can find an example artifact payload in form of a zip file here: [Example artifact payload](https://github.com/Traeger-GmbH/release-server/tree/master/examples)

__PUT request to upload an artifact__:

`curl -k -X PUT http://localhost:5001/releaseartifact/upload/<your_prodcut_name>/debian/amd64/1.0 -H 'Authorization: Basic <username:password>' -H 'content-type: multipart/form-data' -F =@/path/to/the/artifactPayload;type=application/zip`

__Note:__ You have replace the placeholder in the PUT request (product name, authorization header)

__Response example:__

Status: 200 OK
Message: Upload of the artifact successful!

### List all available products

__GET request to list the products__:

 `curl -k -X GET http://localhost:5001/releaseartifact/versions/<your_product_name>`

 __Response example:__

    {
    "productInformation": [
        {
            "identifier": "softwareX",
            "version": "1.0",
            "os": "ubuntu",
            "architecture": "arm64",
            "releaseNotes": {
                "changes": {
                    "de": [
                        {
                            "platforms": [
                                "windows/any",
                                "linux/rpi"
                            ],
                            "added": [
                                "added de 1"
                            ],
                            "fixed": null,
                            "breaking": null,
                            "deprecated": null
                        }
                    ],
                    "en": [
                        {
                            "platforms": [
                                "windows/any",
                                "linux/rpi"
                            ],
                            "added": [
                                "added en 1"
                            ],
                            "fixed": null,
                            "breaking": null,
                            "deprecated": null
                        }
                    ]
                }
            }
        }
    ]
}
