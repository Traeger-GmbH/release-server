## v0.2.0 / 2021-06-20

* [FEATURE] Proof & apply code style and conventions #34 
* [FEATURE] Introduce a changelog with categories, affected platforms and internationalization for the managed artifacts #55 
* [FEATURE] Implement a data validation after uploading #15 
* [CHANGE] GetReleaseInfo now returns the release date #54 
* [CHANGE] Check filesystem permissions during startup #46 
* [FEATURE] Add a Swagger documentation for the endpoints #42 
* [FEATURE] Ensure async operations #44
* [FEATURE] Add a backup and restore mechanism via REST #37
* [CHANGE] Adjust all response messages so that they are formatted as valid JSON #36 
* [FEATURE] Add a "catch all" route #35
* [CHANGE] Upgrade to .Net Core v3.1 #29

## v0.1.0 / 2019-12-15

* [FEATURE] Enable the Release Server to run in a Docker Container #22
* [CHANGE] Compare passwords in constant time to avoid timing attack vulnerabilities #33
* [CHANGE] Use a logger instead of console prints #10
* [FEATURE] Add Basic Auth for the DELETE & PUT endpoints #25
* [CHANGE] Use Linq & DirectoryInfo / FileInfo for the product filtering (increase Performance) #24
* [FEATURE] Create a CI pipelien for the Release Server #28 
* [CHANGE] Adjust the uploading mechanism to prevent data corruption #23
* [CHANGE] Refactor the version handling #20
* [FEATURE] Implement the possiblity to configure the Release Server #21 
* [FEATURE] Implement an endpoint for downloading a specific artifact #12 