using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ACAD_EntitilementApi
{
    public class Plugin : IExtensionApplication
    {

        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

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
    }

    public static class Extensions
    {
        [DllImport("AcConnectWebServices.arx", EntryPoint = "AcConnectWebServicesLogin")]
        public static extern bool AcConnectWebServicesLogin();

        public static async Task<bool> IsEntitled(this Editor ed, string userId, string appId)
        {

            var url = string.Format("https://apps.autodesk.com/webservices/checkentitlement?userid={0}&appid={1}", System.Uri.EscapeUriString(userId), System.Uri.EscapeUriString(appId));           
            using (var httpResponse = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    try
                    {

                        EntitlementResponse entitlementResponse = JsonConvert.DeserializeObject<EntitlementResponse>(content);
                        return entitlementResponse.IsValid;
                    }
                    catch (JsonException) // Invalid JSON
                    {
                        ed.WriteMessage("Invalid JSON.");
                    }
                }
                else
                {
                    ed.WriteMessage("HTTP Response was invalid and cannot be deserialized");
                }

            }
            return false;
        }

    }
    public class EntitlementResponse
    {
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}



