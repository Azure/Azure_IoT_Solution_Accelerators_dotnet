Tokens
==========

## What is a JWT?

 A [JWT (JSON Web Token)](https://jwt.io/) is a compact, URL-safe means of transferring
 information that contains JSON objects encoded and serialized for transmission.

## How is this used?
We can get user information like name and roles values from the JWT token. In this
implementation, we use [Azure AAD tokens](https://docs.microsoft.com/azure/active-directory/develop/active-directory-token-and-claims).

The claims are added to the AAD application manifest so that the token contains the desired information.
You can learn more about optional token claims [here](https://docs.microsoft.com/azure/active-directory/active-directory-claims-mapping).
