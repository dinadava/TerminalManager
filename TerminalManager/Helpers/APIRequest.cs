using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TerminalManager.Entities;

namespace TerminalManager.Helpers
{
    public class APIRequest
    {
        public static bool PostRequest(Dictionary<string, string> values)
        {
            string json = JsonConvert.SerializeObject(values, new KeyValuePairConverter());
            try
            {
                var client = new WebClient();
                client.UploadData(Settings.Instance.ApiServerAddress + "/terminaldetails", "POST",
                    Encoding.Default.GetBytes(json));
                return true;
            }
            catch (WebException ex)
            {
                ErrorLogger.Instance.Write("[TERMINALUPDATE] Error when updating TerminalClientDetails. Exception: " + ex.Response);
                return false;
            }
        }
    }
}
