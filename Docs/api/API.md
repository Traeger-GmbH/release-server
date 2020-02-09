# Documentation of the REST API

## 1. Upload a release artifact

Upload of a specific release artifact.

- **URL** : `/releaseartifact/upload/:product/:os/:hwArch/:version`


- **Method** : `PUT`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

    `version=[string]`: The version (e.g. 1.0)

- **Auth required** : YES

- **Authorization type** : Basic Auth

- **Data**: ZIP-File with `'content-type: multipart/form-data;`

- **Success Response**

    **Condition** : If the uploaded package is unpacked & stored successfully.

    **Code** : `200 OK`

    **Content example** : Upload of the artifact successful!

- **Error Response 1**

    **Condition** : If you provide the wrong body (not `'content-type: multipart/form-data;`)

    **Code** : `400 BAD REQUEST`

    **Content example**

    ```json
       {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        "title": "Bad Request",
        "status": 400,
        "traceId": "|9cd5feb0-4f0378e3da310571."
    }
    ```

- **Error Response 2**

    **Condition** : If the provided body size > 500 MB.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Request body too large."
        }
    ```

- **Error Response 3**

    **Condition** : If the user is not authorized (wrong credentials or missing / invalid authorization header)

    **Code** : `401 UNAUTHORIZED`

- **Sample Call**: PUT https://localhost:5001/releaseartifact/upload/softwareX/ubuntu/arm64/1.0 + authorization header


<div style="page-break-after: always;">


## 2. List all available products

Retrieves all available products.

- **URL** : `/releaseartifact/versions/:product`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)


- **Auth required** : NO

- **Data**: {}

- **Success Response 1**

    **Condition** : If there exists a product with the specified product name.

    **Code** : `200 OK`

    **Content example** :

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

- **Success Response 2**

    **Condition** : If there exists no product with the specified product name.

    **Code** : `200 OK`

    **Content example**

    ```json
        "productInformations": []
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/versions/softwareX


<div style="page-break-after: always;">


## 3. Get the changelog of a specific product

Retrieves the changelog of a specific product.

- **URL** : `/releaseartifact/info/:product/:os/:hwArch/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

    `version=[string]`: The version (e.g. 1.0)


- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : If the specific product exists.

    **Code** : `200 OK`

    **Content example** :

    ```json
        {
            "changelog": "Release 1.0.0\r\n- This is an example\r\n- This is another example"
        }
    ```

- **Error Response**

    **Condition** : If the product with the specified product name does not exist or if there is no changelog available.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Error: Release notes for this artifact not found!"
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/info/softwareX/debian/amd64/1.0


<div style="page-break-after: always;">


## 4. List all available platforms for a specific product

Retrieves all available platforms for a specific product.

- **URL** : `/releaseartifact/platforms/:product/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `version=[string]`: The version (e.g. 1.0)


- **Auth required** : NO

- **Data**: {}

- **Success Response 1**

    **Condition** : If there are existing platforms for the specified product.

    **Code** : `200 OK`

    **Content example** :

    ```json
        {
            "platforms": [
                "debian-amd64",
                "debian-arm64"
            ]
        }
    ```

- **Success Response 2**

    **Condition** : If there exists no platform for the specified product and also if there exists no product with the specified product name.


    **Code** : `200 OK`

    **Content example** :

    ```json
        {
            "platforms": []
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/platforms/softwareX/1.0


<div style="page-break-after: always;">


## 5. List all available versions for a specific platform & product

Retrieves all available versions that are fitting to a specific product & platform (HW architecture + OS).

- **URL** : `/releaseartifact/versions/:product/:os/:hwArch`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response 1**

    **Condition** : If there are existing versions for the specified platform & product.

    **Code** : `200 OK`

    **Content example** :

    ```json
        {
            "versions": [
                "1.1",
                "1.0"
            ]
        }
    ```

- **Success Response 2**

    **Condition** : If there are no versions for the specified product / platform and also if there exists no product with the specified product name.

    **Code** : `200 OK`

    **Content example** :

    ```json
        {
        "versions": []
        }
    ```

- **Error Response**

    **Condition** : If there exists no product with the specified product name.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/versions/softwareX/debian/amd64


<div style="page-break-after: always;">


## 6. Download of a specific product / artifact

Retrieves the artifact of the specified product.

- **URL** : `/releaseartifact/download/:product/:os/:hwArch/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

    `version=[string]`: The version (e.g. 1.0)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : If there exists a product with the specified product name.

    **Code** : `200 OK`

    **Content example** : ZIP file with the artifact.


- **Error Response**

    **Condition** :  If there exists no product with the specified product name.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Unable to find the specified file."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/download/softwareX/debian/amd64/1.0


<div style="page-break-after: always;">


## 7. Download the latest version of a specific artifact / product 

Retrieves the latest artifact of a specific product.

- **URL** : `/releaseartifact/download/:product/:os/:hwArch/latest`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : If the specific product exists.

    **Code** : `200 OK`

    **Content example** : ZIP file with the artifact.


- **Error Response 1**

    **Condition** :  If the product is not available for the specified platform (OS + HW architecture).

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Sequence contains no elements"
        }
    ```
- **Error Response2**

    **Condition** : If there exists no product with the specified product name.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/download/softwareX/debian/amd64/latest


<div style="page-break-after: always;">


## 8. Retrieve the newest version of a specific product

Retrieves the newest version of a specific product.

- **URL** : `/releaseartifact/latest/:product/:os/:hwArch`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : If the specified product exists.

    **Code** : `200 OK`

    **Content example** :
    ```json
        {
            "version": "1.0"
        }
    ```


- **Error Response**

    **Condition** :  If the product is not available for the specified platform (OS + HW architecture) and if the product with the specified product name does not exist.


    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Sequence contains no elements"
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/latest/softwareX/ubuntu/arm64


<div style="page-break-after: always;">


## 9. Delete a specific artififact / product 

Deletes the specified product.

- **URL** : `/releaseartifact/:product/:os/:hwArch/:version`

- **Method** : `DELETE`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

    `os=[string]`: The operating system (e.g. ubuntu)

    `hwArch=[string]`: The hardware architecture (e.g. amd64)

    `version=[string]`: The version (e.g. 1.0)

- **Auth required** : YES

- **Authorization type** : Basic Auth

- **Data**: {}

- **Success Response**

    **Condition** : If the specified product got deleted successfully.

    **Code** : `200 OK`

    **Content example** :
        ```text
        artifact successfully deleted
        ```

- **Error Response 1**

    **Condition** : If there exists no product with the specified product name.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Error Response 2**

    **Condition** : If the user is not authorized (wrong credentials or missing/invalid authorization header)

    **Code** : `401 UNAUTHORIZED`

- **Sample Call**: DELETE https://localhost:5001/releaseartifact/softwareX/debian/amd64/1.0 + authorization header



<div style="page-break-after: always;">


## 10. Delete the whole product line

Deletes all products of a specific product name.

- **URL** : `/releaseartifact/:product`

- **Method** : `DELETE`

- **URL Parameters** :

    **Required:**

    `product=[string]`: The unique product name (e.g. softwareX)

- **Auth required** : YES

- **Authorization type** : Basic Auth

- **Data**: {}

- **Success Response**

    **Condition** : If all products of the specified product name got deleted successfully.

    **Code** : `200 OK`

    **Content example** :
        ```text
        product successfully deleted
        ```

- **Error Response 1**

    **Condition** : If there exists no product line with the specified product name.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Error Response 2**

    **Condition** : If the user is not authorized (wrong credentials or missing/invalid authorization header)

    **Code** : `401 UNAUTHORIZED`

- **Sample Call**: https://localhost:5001/releaseartifact/codabdix + authorization header