using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
namespace RiotAPILib
{
    public class BaseInfo
    {
        public static string APIKey { get; set; }
        public static string Region { get; set; }
        public static string Language { get; set; }
        private static JObject _realm { get; set; }
        public static JObject realm
        {
            get
            {
                if (_realm == null)
                    _realm = JsonConvert.DeserializeObject<JObject>(GetRealm(Region));
                return _realm;
            }
        }
        private static Dictionary<string, JObject> _realmInfo { get; set; }
        public static Dictionary<string, JObject> realmInfo
        {
            get
            {
                if (_realmInfo == null)
                {
                    _realmInfo = new Dictionary<string, JObject>();
                    string cdn = realm["cdn"].ToString();
                    foreach (JProperty item in realm["n"])
                    {
                        _realmInfo.Add(item.Name, JsonConvert.DeserializeObject<JObject>(GetDataFromDDragon(cdn, item.Value.ToString(), Language, item.Name + ".json")));
                    }
                }
                return _realmInfo;

            }
            set
            {
                _realmInfo = value;
            }
        }
        private static Dictionary<string, Dictionary<string, string>> _infoList { get; set; }
        public static Dictionary<string, Dictionary<string, string>> infoList
        {
            get
            {
                if (_infoList == null)
                {
                    _infoList = new Dictionary<string, Dictionary<string, string>>();
                    foreach (var type in realmInfo)
                    {
                        try
                        {
                            string typeKey = type.Key;
                            Dictionary<string, string> list = new Dictionary<string, string>();
                            string cdn = realm["cdn"].ToString();
                            string version = realmInfo[typeKey]["version"].ToString();
                            foreach (JProperty item in realmInfo[typeKey]["data"])
                            {
                                string url = string.Format("{0}/{1}/img/{2}/{3}", cdn, version, typeKey, item.Value["image"]["full"].ToString());
                                list.Add(item.Name, url);
                            }
                            _infoList.Add(typeKey, list);
                        }
                        catch { }
                    }
                }
                return _infoList;
            }
        }

        private List<string> _versions { get; set; }
        public List<string> versions
        {
            get
            {
                if (_versions == null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string url = "https://ddragon.leagueoflegends.com/api/versions.json";
                        byte[] result = wc.DownloadData(url);
                        _versions = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(result));
                    }
                }
                return _versions;
            }
        }
        public static Dictionary<string, string> GetItemUrl()
        {

            if (!infoList.ContainsKey("item"))
            {
                Dictionary<string, string> list = new Dictionary<string, string>();
                string cdn = realm["cdn"].ToString();
                string version = realmInfo["item"]["version"].ToString();
                foreach (JProperty item in realmInfo["item"]["data"])
                {
                    cdn = "htpp://datadragon-new-807633007.us-east-1.elb.amazonaws.com/cdn";
                    string url = string.Format("{0}/{1}/img/{2}/{3}", cdn, version, "item", item.Value["image"]["full"].ToString());
                    list.Add(item.Name, url);
                }
                infoList.Add("item", list);
            }
            return infoList["item"];
        }
        private static string GetRealm(string platformId)
        {
            using (WebClient wc = new WebClient())
            {
                string url = string.Format("http://datadragon-new-807633007.us-east-1.elb.amazonaws.com/realms/{0}.json", platformId);
                byte[] result = wc.DownloadData(url);
                return Encoding.UTF8.GetString(result);
            }
        }


        private static string GetDataFromDDragon(string cdn, string version, string lang, string key)
        {
            string file = string.Format("{0}/{1}/data/{2}/{3}", System.Environment.CurrentDirectory, version, lang, key);
            string dir = string.Format("{0}/{1}/data/{2}/", System.Environment.CurrentDirectory, version, lang);
            if (!File.Exists(file))
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (WebClient wc = new WebClient())
                {
                    string url = string.Format("{0}/{1}/data/{2}/{3}", cdn, version, lang, key);
                    wc.DownloadFile(url, file);
                }
            }
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string result = sr.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
