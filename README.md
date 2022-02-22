# Entitlement

The Autodesk App Store has an Entitlement API service with which you can build a simple copy protection system for your Autodesk App Store desktop Apps. The Entitlement API service exposes a REST based “checkentitlement” API that you can use to identify whether a user has an ‘entitlement’ to use your App or not.
Details on the API:

```
Base URL: https://apps.autodesk.com
End Point: webservices/checkentitlement
Http Method: GET
Parameters: ?userid=***&appid=*** 
Return : Json object.
```

Here userid is the ID of the user whose entitlement needs to be verified. Please note the userid is the internal ID, which is different from the username used to log into the store or into different Autodesk products.
To use this API, from your App make a simple HTTP (REST) call to the Entitlement API, passing in the unique ID of your App, and the userid of the customer currently signed in to their Autodesk ID from the Autodesk product in which your App is running. The Entitlement API response will tell you whether the user has an ‘Entitlement’ to use your App (i.e. it tells you if this user has bought this App or not).
You can use the Entitlement API in your subscription Apps too. In subscription Apps, the result returned depends on whether the user’s subscription has expired or not. (i.e., this API will respond that the user has an entitlement for the App only while the subscription is valid). You can get the unique ID of your App once you submit the App in the Autodesk App Store (please let us know if you have any problem in identifying the id of your App).

## To build

```
git clone https://github.com/MadhukarMoogala/Entitlement.git
cd Entitlement
devenv Entitlement.sln
build
```

## To Run

### Dependency
 
 - Minimum  .NET 4.7

```
Download Entitlement.exe
Entitlement.exe <userId> <appId>
```

## Intergrating with AutoCAD

We need to collect first UserId, this can be checked using Systemvariable `ONLINEUSERID`

```
[CommandMethod("CheckEntitleMent")]
 public static async void CheckEntitlement()
 {
     var doc = Application.DocumentManager.MdiActiveDocument;
     if (doc == null) return;
     var ed = doc.Editor;
     //App Id is unique Id that gets allotted to your app once it is published on store,
     //the appid can be found from your publisher apps page
     var pr = ed.GetString("Enter the AppId that your would like check if you are entitled\n");
     if (pr.Status != PromptStatus.OK) return;

     string appId = pr.StringResult;
     //string appId =  "2024453975166401172"
     var userId = (string)Application.GetSystemVariable("ONLINEUSERID");
     //If ONLINEUSERID is empty or null, user is not logged in to system.
     if (String.IsNullOrEmpty(userId))
     {
         Extensions.AcConnectWebServicesLogin();
     }
     bool isValid = await ed.IsEntitled(userId, appId);
     ed.WriteMessage($"Is Valid {isValid}");

 }
```


