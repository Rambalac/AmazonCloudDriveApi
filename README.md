Amazon Cloud Drive Api for .NET
===============================
General implementation of REST API for Amazon Cloud Drive. Not all functions are implemented, but it can download/upload files and organize files/folders.

[![Build status](https://ci.appveyor.com/api/projects/status/wigge25onobhd31j/branch/master?svg=true)](https://ci.appveyor.com/project/Rambalac/amazonclouddriveapi/branch/master) 
[![Nuget badge](https://buildstats.info/nuget/AmazonCloudDriveApi)](https://www.nuget.org/packages/AmazonCloudDriveApi)

Nuget
-----
You can find nuget package with name ```AmazonCloudDriveApi```

Example
-------

### Desktop authentication
First you need to create security profile in Amazon Dev console and whitelist it. Since August 2016 it became complicated.
There you get your app ClientId and ClientSecret.

Then you create root API object. 
```C#
this.amazon = new AmazonDrive(ClientId, ClientSecret);
```

To authenticate new connection.

```C#
var success = await this.amazon.AuthenticationByExternalBrowser(CloudDriveScopes.ReadAll | CloudDriveScopes.Write | CloudDriveScopes.Profile, TimeSpan.FromMinutes(10));
```

To be able to reauthenticate without confirmation each time on Amazon web site you need to reuse Access and Refresh tokens. After some time tokens get updated by API they have to be stored somewhere by application. For that you need to assign implementation of ```ITokenUpdateListener``` which will store tokens. Reference to the implementation is weak, so do not use any local object, object with your implementation should live while API is used.

#### Implementation example
```C#
public abstract class AmazonTokenListener : ITokenUpdateListener
{
public void OnTokenUpdated(string access_token, string refresh_token, DateTime expires_in)
  {
      var settings = Properties.Settings.Default;

      settings.AuthToken = access_token;
      settings.AuthRenewToken = refresh_token;
      settings.AuthTokenExpiration = expires_in;
      settings.Save();
  }
}
```
Assign object to ```OnTokenUpdate``` property before any login attempt
```C#
this.tokenSaver = new AmazonTokenListener();
this.amazon.OnTokenUpdate
```

Next time when you restart your application and want to relogin into Amazon cloud Drive use
```C#
var success = amazon.AuthenticationByTokens(settings.AuthToken, settings.AuthRenewToken, settings.AuthTokenExpiration)
```

