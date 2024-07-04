# OAuthCodePkceFlow
Demonstrates the OAuth authorisation code flow with [PKCE](https://oauth.net/2/pkce/).

# Step by step explanation

## Spinup local webhost as redirection target

A local http endpoint is created at http://localhost/acceptcode. This will serve as redirection target for the authentication uri.

## Authenticating

The tool will open a browser window, and load the authentication dialog. For redirection we pass the previously mentioned uri.

## Get authorisation code

When the authentication dialog is succesfully submitted, the server will redirect to the redirection uri we initally passed. In the controller for this endpoint we grab the authorization code from the headers.

## Exchange code for token

Once we have the token, we can do another request to exchange the code + pkce code verifier for an access token. 

## Display token info

The resulting token information is displayed in the browser window.
