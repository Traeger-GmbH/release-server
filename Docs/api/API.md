# Dokumentation der REST-API (Stand 13.11.2019)

## 1. Upload eines Release-Artifacts (Story 2)

Ermöglicht den Upload eines eindeutigen Release-Artefakts.

- **URL** : `/releaseartifact/upload/:product/:os/:hwArch/:version`

- **Method** : `PUT`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

    `version=[string]`: Die Versionsnummer (z.B. 1.0)

- **Auth required** : NO

- **Data**: ZIP-File mit `'content-type: multipart/form-data;`

- **Success Response**

    **Condition** : Sobald das Paket erfolgreich entpackt und gespeichert wurde.

    **Code** : `200 OK`

    **Content example** : Upload of the artifact successful!

- **Error Response 1**

    **Condition** : Wenn ein falscher Body übergeben wird (kein `'content-type: multipart/form-data;`)

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

    **Condition** : Wenn der Body >500 MB ist.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Request body too large."
        }
    ```

- **Sample Call**: PUT https://localhost:5001/releaseartifact/upload/softwareX/ubuntu/arm64/1.0


<div style="page-break-after: always;">


## 2. Liste alle verfügbaren Produkte (Story 3)

Ermöglicht eine Auflistung aller verfügbaren Produkte.

- **URL** : `/releaseartifact/versions/:product`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: der eindeutige Produktname (z.B. softwareX)


- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn Produkte mit dem spezifizierten Namen existieren.

    **Code** : `200 OK`

    **Content example** :

    ```json
        {
            "ProductIdentifier": "softwareX",
            "Version": "1.0",
            "Os": "debian",
            "HwArchitecture": "amd64"
        },
        {
            "ProductIdentifier": "softwareX",
            "Version": "1.1",
            "Os": "debian",
            "HwArchitecture": "amd64"
        }
    ```

- **Error Response**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/versions/softwareX


<div style="page-break-after: always;">


## 3. Liefere Changelog für ein Produkt (Story 4)

Liefert den Changelog für ein spezifisches Produkt. 

- **URL** : `/releaseartifact/info/:product/:os/:hwArch/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

    `version=[string]`: Die Versionsnummer (z.B. 1.0)


- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn das spezifische Produkt existiert.

    **Code** : `200 OK`

    **Content example** :

    ```text
        Release 1.0.0
        - This is an example
        - This is another example
    ```

- **Error Response**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert bzw. keine Release-Notes vorhanden sind.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Error: Release notes for this artifact not found!"
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/info/softwareX/debian/amd64/1.0


<div style="page-break-after: always;">


## 4. Liste alle verfügbaren Plattformen eines Produktes (Story 5)

Listet alle verfügbaren Plattformen für ein spezifisches Product

- **URL** : `/releaseartifact/platforms/:product/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `version=[string]`: Die Versionsnummer (z.B. 1.0)


- **Auth required** : NO

- **Data**: {}

- **Success Response 1**

    **Condition** : Wenn Plattformen für das spezifische Produkt existieren.

    **Code** : `200 OK`

    **Content example** :

    ```text
        ["debian-amd64","debian-arm64","ubuntu-arm64"]
    ```

- **Success Response 2**

    **Condition** : Wenn keine Plattformen für das spezifische Produkt existieren.

    **Code** : `200 OK`

    **Content example** :

    ```text
        []
    ```

- **Error Response**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/platforms/softwareX/1.0


<div style="page-break-after: always;">


## 5. Liste alle verfügbaren Versionen zu einer Plattform sowie eines Produktes (Story 6)

Listet alle verfügbaren Versionen für ein spezifisches Product & einer spezifischen Plattform

- **URL** : `/releaseartifact/versions/:product/:os/:hwArch`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response 1**

    **Condition** : Wenn Versionen für das spezifische Produkt & Plattform existieren.

    **Code** : `200 OK`

    **Content example** :

    ```text
        ["1.0","1.1"]
    ```

- **Success Response 2**

    **Condition** : Wenn keine Versionen für das spezifische Produkt & Plattform existieren.

    **Code** : `200 OK`

    **Content example** :

    ```text
        []
    ```

- **Error Response**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/versions/softwareX/debian/amd64


<div style="page-break-after: always;">


## 6. Download eines spezifischen Produktes / Artefaktes (Story 7)

Liefert das Artefakt des spezifischen Produktes.

- **URL** : `/releaseartifact/download/:product/:os/:hwArch/:version`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

    `version=[string]`: Die Versionsnummer (z.B. 1.0)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn das spezifizierte Produkt existiert.

    **Code** : `200 OK`

    **Content example** : ZIP-Datei mit dem Artefakt / Produkt.


- **Error Response**

    **Condition** :  Wenn das spezifizierte Produkt nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Unable to find the specified file."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/download/softwareX/debian/amd64/1.0


<div style="page-break-after: always;">


## 7. Download der neuesten Version eines spezifischen Produktes / Artefaktes (Story 8)

Liefert das neueste Artefakt des spezifischen Produktes.

- **URL** : `/releaseartifact/download/:product/:os/:hwArch/latest`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn das spezifizierte Produkt existiert.

    **Code** : `200 OK`

    **Content example** : ZIP-Datei mit dem Artefakt / Produkt.


- **Error Response 1**

    **Condition** :  Wenn das Produkt die spezifizierte Plattform (OS + HW-Architektur) nicht unterstützt.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Sequence contains no elements"
        }
    ```
- **Error Response2**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/download/softwareX/debian/amd64/latest


<div style="page-break-after: always;">


## 8. Liste die neueste Version eines spezifischen Produktes / Artefaktes (Story 9)

Liefert die neueste Versionsnummer des spezifischen Produktes.

- **URL** : `/releaseartifact/latest/:product/:os/:hwArch`

- **Method** : `GET`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn das spezifizierte Produkt existiert.

    **Code** : `200 OK`

    **Content example** :
        ```text
        1.1
        ```


- **Error Response 1**

    **Condition** :  Wenn das Produkt die spezifizierte Plattform (OS + HW-Architektur) nicht unterstützt.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Sequence contains no elements"
        }
    ```
- **Error Response2**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: GET https://localhost:5001/releaseartifact/latest/softwareX/ubuntu/arm64


<div style="page-break-after: always;">


## 9. Lösche ein spezifisches Produkt / Artefakt (Story 10)

Löscht das spezifizierte Produkt.

- **URL** : `/releaseartifact/:product/:os/:hwArch/:version`

- **Method** : `DELETE`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

    `os=[string]`: Das Betriebssystem (z.B. ubuntu)

    `hwArch=[string]`: Die Hardware-Architektur (z.B. amd64)

    `version=[string]`: Die Versionsnummer (z.B. 1.0)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn das spezifizierte Produkt erfolgreich gelöscht wurde.

    **Code** : `200 OK`

    **Content example** :
        ```text
        artifact successfully deleted
        ```

- **Error Response**

    **Condition** : Wenn das Produkt mit dem spezifizierten Namen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: DELETE https://localhost:5001/releaseartifact/softwareX/debian/amd64/1.0



<div style="page-break-after: always;">


## 10. Lösche eine gesamte Produktreihe (Story 10)

Löscht alle Produkte eines spezifischen Produktnamens.

- **URL** : `/releaseartifact/:product`

- **Method** : `DELETE`

- **URL Parameters** :

    **Required:**

    `product=[string]`: Der eindeutige Produktname (z.B. softwareX)

- **Auth required** : NO

- **Data**: {}

- **Success Response**

    **Condition** : Wenn alle Produkte des spezifizierten Produktnamens erfolgreich gelöscht wurden.

    **Code** : `200 OK`

    **Content example** :
        ```text
        product successfully deleted
        ```

- **Error Response**

    **Condition** : Wenn die Produktreihe mit dem spezifizierten Produktnamen nicht existiert.

    **Code** : `500 INTERNAL SERVER ERROR`

    **Content example**

    ```json
        {
            "error": "Could not find a part of the path 'C:\\Path\\softwareX'."
        }
    ```

- **Sample Call**: https://localhost:5001/releaseartifact/codabdix