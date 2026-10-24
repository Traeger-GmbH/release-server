## Unreleased

Server:

* [FEATURE] /artifacts/{product}/info lists the files of every release in the new field "artifacts" (platform, file name, size, SHA-256, download URL); the existing fields are unchanged
* [FEATURE] The SHA-256 checksum is calculated on upload and stored in deployment.json; checksums of existing artifacts are added in the background (AddMissingArtifactHashes); older versions ignore the new property
* [FEATURE] Downloads send the checksum as strong ETag and Repr-Digest header (conditional requests are supported)

## v1.0.0 (2026-10-24)

Server (now .NET 10, ASP.NET Core 10 runtime image):

* [CHANGE] Upgrade from .NET Core 3.1 to .NET 10; the public API, the storage layout and the backup format are unchanged
* [FIX] Prevent writing, reading and deleting outside of the artifact directory (path traversal via names in uploads)
* [FIX] Restore verifies the backup before replacing any data; an invalid backup no longer deletes the existing data
* [FIX] Passwords may contain ':'; requests are denied (401) if no credentials are configured
* [FIX] Uploads above 128 MB failed (multipart limit); restores above about 30 MB could fail (request size limit)
* [FIX] Incomplete or broken artifact directories no longer break /info and latest of the whole product
* [FIX] Failed uploads no longer leave a "temp" product behind
* [FIX] Restore accepts additional files and sub directories in release directories (e.g. macOS metadata extracted by old versions) and artifacts in sub directories, like previous versions
* [CHANGE] Uploads ignore operating system metadata in ZIP files (macOS: __MACOSX/, ._*, .DS_Store; Windows: Thumbs.db, ehthumbs.db, desktop.ini, $RECYCLE.BIN/); existing data is not changed
* [CHANGE] Deleting the last release of a platform or product removes the then empty directories; product directories without any release (e.g. left behind by previous versions) are no longer listed by GET /artifacts and not counted as products (their /info was and is 404)
* [FIX] Concurrent first uploads after a start could skip the check of required properties in deployment.json (thread-safety issue of Newtonsoft.Json)
* [FEATURE] Keep the newest backups (BackupRetentionCount, default 5) instead of only the latest one
* [FEATURE] Downloads are streamed from disk and support range requests (resume)
* [CHANGE] A package upload is only rejected if its version already exists (new versions no longer need force=true)
* [CHANGE] Browsers may only call authenticated endpoints from configured origins (Cors:WriteOrigins); public endpoints stay open
* [CHANGE] Remove the query endpoints that were disabled in 2021 (replaced by /artifacts/{product}/info)
* [DOCS] Swagger documentation for external users

Client:

* [CHANGE] Replace the Nuxt 2 client by a Vue 3 single page application served by nginx (port and RELEASE_SERVER_URL unchanged)
* [FIX] Corrupted downloads, missing change logs without English entries, truncated release lists, repeated uploads
* [FIX] The filter field in Explore overflowed its column
* [SECURITY] The password is no longer stored in the local storage of the browser

## v0.2.0

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

## v0.1.0

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