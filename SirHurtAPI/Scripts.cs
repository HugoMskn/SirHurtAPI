using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SirHurtAPI
{
    public static class Scripts
    {
       
       
        private readonly static string DllName = "[SirHurtAPI]";
        private static List<JToken> list = null;
        public static string[] DLScriptHub()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Roblox/WinInet");

                    Console.WriteLine(DllName + "Starting download script list async...");
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(SirHurtAPI.AlwaysGoodCertificate);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var string_ = wc.DownloadString(new Uri("https://sirhurt.net/upl/UIScriptHub/fetch.php"));

                    Console.WriteLine(string_);
                    list = JObject.Parse(string_)["scripts"].Children().Children<JToken>().ToList<JToken>();
                    var lst = new List<string>();
                    foreach (JToken jtoken in list)
                    {
                        lst.Add(jtoken["Name"].ToString());
                    }
                    Console.WriteLine(DllName + "Decode sucess, returning lst...");
                    return lst.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DllName + "Unable to download script list...");
                Console.WriteLine(ex);
                return new List<string>().ToArray();
            }
        }
        public async static Task<Tuple<string,string>> GetScriptInfoFromName(string ScriptName)
        {
            if (list == null)
            {
                DLScriptHub();
            }
            foreach (JToken jtoken in list)
            {
                if (jtoken["Name"].ToString() == ScriptName)
                {
                    Console.WriteLine(DllName + "Correct Name!, downloading image");
                    await Task.Delay(50);
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(SirHurtAPI.AlwaysGoodCertificate);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    return Tuple.Create(jtoken["Desc"].ToString(), jtoken["Picture"].ToString());
                }
            }
            return Tuple.Create("", "");
        }
        public static bool ExecuteFromName(string ScriptName)
        {
            Console.WriteLine(DllName + "Script selected: " + ScriptName);
            string scriptURL = null;
            foreach (JToken jtoken in list)
            {
                if (jtoken["Name"].ToString() == ScriptName)
                {
                    scriptURL = jtoken["FileName"].ToString();
                    Console.WriteLine(DllName + "File name:" + scriptURL);
                }
            }
            if (scriptURL == null)
            {
                Console.WriteLine(DllName + "scriptURL is null, returning (prob bc invalid script name)");
                return false;
            }
            try
            {
                MessageBox.Show(scriptURL);
                return SirHurtAPI.Execute("loadstring(game:HttpGet('https://sirhurt.net/upl/UIScriptHub/Scripts/script.php?script=" + scriptURL + "'))()", true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
