using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TerminalManager.Entities;


namespace TerminalManager.Helpers
{
    public class APIRequest
    {
        public static async Task<bool> PostRequest(Dictionary<string, string> values)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    Settings.Instance.ApiServerAddress + "/terminaldetails",
                    new FormUrlEncodedContent(values));

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            return false;
        }
        
        public static async Task<string> GetRequest(string parameter)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(Settings.Instance.ApiServerAddress + "/getdetails/" + parameter);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<string>(json, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                }
            }
            return null;
        }
    }
}
