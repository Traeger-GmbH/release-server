![Build Status](https://travis-ci.com/Traeger-GmbH/release-server.svg?branch=master)

# Release server

An application for managing your own release artifacts. The release server provides several REST endpoints for the following operations:

- Upload the release artifacts
- Download the release artifacts
- Get information about the release artifacts (e.g. which version is the latest etc.)

## Setup

You have two options to setup the release server.

### Run with Docker-compose

1. Download the [docker-compose.yml](https://github.com/Traeger-GmbH/release-server/blob/master/docker-compose.yml) and put it in a directory of your choice.

2. Download the appsettings.json and credentials.json from [Configuration examples](https://github.com/Traeger-GmbH/release-server/tree/master/Example) and put them in a directory of your choice.

3. Replace the placeholder in the credentials.json with your own username and __base64 encoded__ password.

4. Run the application with docker-compose: `docker-compose -f "docker-compose.yml" up -d --build`

Now, the release server is reachable at https://localhost:5001.

### Run with Docker

1. Pull the Docker image: `docker pull traeger/release-server:0.1`

2. Download the appsettings.json and credentials.json from [Configuration examples](https://github.com/Traeger-GmbH/release-server/tree/master/Example) and put them into a directory of your choice.

3. Replace the placeholder in the credentials.json with your own username and __base64 encoded__ password.

4. Run the application with docker run: `docker run -d --env SERVER_AUTH_PATH=/auth/credentials.json -v <path_to_your_directory>/appsettings.json:/app/appsettings.json -v <path_to_your_directory>/artifacts:/app/artifacts -v <path_to_your_directory>/credentials.json:/auth/credentials.json -p 5001:5001  traeger/release-server:0.1`

Now, the release server is reachable at https://localhost:5001.

## Usage

The first two REST endpoint examples are documented above. 

For more information about the other endpoints and their usage, read the [API documentation](https://github.com/Traeger-GmbH/release-server/blob/master/Docs/api/API.md).

### Example REST calls

#### Upload an Artifact

__Structure of the artifact:__ The artifact payload has to be a zip file and has to contain the following elements:

- the artifact itself (exe, zip or something else)
- the meta information in form of the deployment.json
- a changelog

You can find an example artifact payload in form of a zip file here: [Example artifact payload](https://github.com/Traeger-GmbH/release-server/tree/master/Example)

__PUT request to upload an artifact__:

`curl -k -X PUT https://localhost:5001/releaseartifact/upload/<your_prodcut_name>/debian/amd64/1.0 -H 'Authorization: Basic <base 64 encoded "username:password">' -H 'content-type: multipart/form-data' -F =@/path/to/the/artifactPayload`

__Response example:__

Status: 200 OK
Message: Upload of the artifact successful!

#### List all available products

__GET request to list the products__:

 `curl -k -X GET https://localhost:5001/releaseartifact/versions/<your_product_name>`

 __Response example:__

     ```json
        {
            "identifier": "softwareX",
            "version": "1.0",
            "os": "debian",
            "architecture": "amd64"
        },
        {
            "identifier": "softwareX",
            "version": "1.1-beta",
            "os": "debian",
            "architecture": "amd64"
        }
     ```
