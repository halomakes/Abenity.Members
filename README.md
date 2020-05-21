# Abenity.Members [![Version](https://img.shields.io/nuget/v/Abenity.Members)](https://www.nuget.org/packages/Abenity.Members/) [![Downloads](https://img.shields.io/nuget/dt/Abenity.Members)](https://www.nuget.org/packages/Abenity.Members/) [![Build Status](https://api.travis-ci.org/halomademeapc/Abenity.Members.svg?branch=master)](https://travis-ci.org/github/halomademeapc/Abenity.Members)
.NET Standard API Client for interacting with the [Abenity Members API](https://abenity.com/developers/api/members)
This is offered as an alternative to the office [abenity-csharp](https://github.com/Abenity/abenity-csharp) library with some enhancements.

## Configuration
The following information is required to authenticate SSO requests.  
1. API credentials. Specifically: **username**, **password** and **key**. You get these values from Abenity.
2. A public and private key. _You_ need to generate these, and send your public key to Abenity to associate with your account.

Provided below is an example configuration section.  Note that the key entries are paths to files containing the key information, not the keys themselves.
```json
"Abenity": {
    "UseProduction": false,
    "KeyPaths": {
        "PrivateKeyFilePath": "./keys/private.pem",
        "PublicKeyFilePath": "./keys/public.pem"
    },
    "Credentials": {
        "Username": "abcd",
        "Password": "abcde12345^&*",
        "Key": "abcdef"
    }
}
```
*Note that the keys are required to be in the PKCS1 format*

## Example Usage
Register the API Client in your application's service provider
```csharp
var config = new AbenityConfiguration();
Configuration.Bind("Abenity", config);
services.AddSingleton(config);

services.AddHttpClient();
services.AddScoped<IAbenityMembersApiClient, AbenityMembersApiClient>();
```

Inject it into your service/controller and make a call
```csharp
private readonly IAbenityMembersApiClient membersClient;

public MyService(IAbenityMembersApiClient membersClient) {
    this.membersClient = membersClient;
}

public async Task<IActionResult> GoToAbenity() {
    var userInfo = myUserInfoService.GetCurrentUserInfo().ToAbenityRequest();
    var result = await membersClient.AuthenticateUserAsync(userInfo);
    return Redirect(result.Url);
}
```

## Comparison with [`abenity-csharp`](https://github.com/Abenity/abenity-csharp)
This libary is based on the official package provided by Abenity and shares most of its authentication code with that library, but offers several key improvements:
* **Fuller Endpoint Support** - The official library only supports SSO authentication calls.  In this library, methods for calling the *deauthorize* and *reauthorize* user endpoints are also provided.
* **Typed Responses** - The official library just spits out a string as a response to SSO authentication requests and it's up to you to parse them.  This library, by contrast, will provide a typed object for you to access.  If something goes wrong, you get a typed exception with the details populated based on the response from the API, removing some of the guesswork.  This is handled by Json.NET using stream deserialization after header transfer for optimal performance.  
* **Out-of-the-box Dependency Injection Support** - Configuration is bundled into a single object for easy injection and the API client can accept an HttpClient from an IHttpClientFactory for optimal efficiency
* **Better File Handling** - The official library opens the key files at construction and never explicitly closes them.  In this library, file reads are correctly scoped so they are only open for a minimal amount of time.
* **Enhanced Documentation** - XML Comments are provided on public-facing models and methods to help guide usage.  

## Dependencies
* **BouncyCastle** 1.8.5+
* **Newtonsoft.Json** 10.0.3+
* **System.ComponentModel.Annotations** 4.7.0+

## Changelog
**1.0.0** Initial Release