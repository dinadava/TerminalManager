using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TerminalManager.Entities;

namespace TerminalManager.Helpers
{
    public class APIRequest
    {
        public static bool PostRequest(string uri, Dictionary<string, string> values)
        {
            string json = JsonConvert.SerializeObject(values, new KeyValuePairConverter());
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentType = "application/json";
                request.Method = "POST";
                request.AllowAutoRedirect = false;
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(json);
                    writer.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if ((int)response.StatusCode == 200)
                    return true;
                else
                {
                    ErrorLogger.Instance.Write("[API] Post Request Failed! Response: " + response.StatusDescription);
                    return false;
                }
            }
            catch (WebException ex)
            {
                ErrorLogger.Instance.Write("[API] Error when updating TerminalClientDetails. Exception: " + ex.Response);
                return false;
            }
        }

        public static string GetRequest(string uri)
        {
            string result = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if ((int)response.StatusCode == 200)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        if (reader.ReadToEnd() != "")
                            result = reader.ReadToEnd();
                    }
                    else
                        ErrorLogger.Instance.Write("[API] Get Request Failed! Response: " + response.StatusDescription);
                }
            }
            catch (WebException ex)
            {
                ErrorLogger.Instance.Write("[API] Get Request Error. Exception: " + ex.Response);
            }
            return result;
        }
    }
}
