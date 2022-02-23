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
        /// <summary>
        /// This Utility import is necessary to invoke Autodesk Login.
        /// </summary>
        /// <returns></returns>
        [DllImport("AcConnectWebServices.arx", EntryPoint = "AcConnectWebServicesLogin")]
        public static extern bool AcConnectWebServicesLogin();

        public static Editor Ed => Application.DocumentManager.MdiActiveDocument.Editor;
            
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        [CommandMethod("CheckEntitleMent")]
        public static async void CheckEntitlement()
        {

            //App Id is unique Id that gets allotted to your app once it is published on store,
            //the appid can be found from your publisher apps page
            var pr = Ed.GetString("Enter the AppId that your would like check if you are entitled\n");
            if (pr.Status != PromptStatus.OK) return;

            string appId = pr.StringResult;
            //string appId =  "2024453975166401172"
            var userId = (string)Application.GetSystemVariable("ONLINEUSERID");
            //If ONLINEUSERID is empty or null, user is not logged in to system.
            if (String.IsNullOrEmpty(userId))
            {
                AcConnectWebServicesLogin();
            }
            try
            {
                bool isValid = await IsEntitled(userId, appId);
                Ed.WriteMessage($"Is Entitled: {isValid}");
            }
            catch (System.Exception ex)
            {
                Ed.WriteMessage(ex.Message);
            }
           
            

        }
        public static async Task<bool> IsEntitled(string userId, string appId)
        {

            var url = string.Format("https://apps.autodesk.com/webservices/checkentitlement?userid={0}&appid={1}",
                            Uri.EscapeUriString(userId),
                            Uri.EscapeUriString(appId));
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
                    catch (JsonException ex) // Invalid JSON
                    {
                        throw ex;
                    }
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



