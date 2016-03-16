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
    public class AllInfo
    {
        public static string APIKey { get; set; }
        public static string Region { get; set; }
        public static string Language { get; set; }

        private static Dictionary<string, Dictionary<string, JObject>> _realmInfobyVersions { get; set; }
        public static Dictionary<string, Dictionary<string, JObject>> realmInfobyVersions
        {
            get
            {
                if (_realmInfobyVersions == null)
                {
                    _realmInfobyVersions = new Dictionary<string, Dictionary<string, JObject>>();
                    foreach (var version in versions)
                    {
                        string cdn = realm["cdn"].ToString();
                        Dictionary<string, JObject> _realmInfo = new Dictionary<string, JObject>();
                        foreach (JProperty item in realm["n"])
                        {
                            _realmInfo.Add(item.Name, JsonConvert.DeserializeObject<JObject>(GetDataFromDDragon(cdn, version, Language, item.Name + ".json")));
                        }
                        _realmInfobyVersions.Add(version, _realmInfo);
                    }
                }
                return _realmInfobyVersions;
            }
        }
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
        // private static Dictionary<string, JObject> _realmInfo { get; set; }
        //public static Dictionary<string, JObject> realmInfo(string version)
        //{

        //    if (_realmInfo == null)
        //    {
        //        _realmInfo = new Dictionary<string, JObject>();
        //        string cdn = realm["cdn"].ToString();
        //        foreach (JProperty item in realm["n"])
        //        {
        //            _realmInfo.Add(item.Name, JsonConvert.DeserializeObject<JObject>(GetDataFromDDragon(cdn, version, Language, item.Name + ".json")));
        //        }
        //    }
        //    return _realmInfo;


        //}
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _infoList { get; set; }
        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> infoList
        {
            get
            {
                if (_infoList == null)
                {
                    _infoList = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    foreach (var reamInfo in realmInfobyVersions)
                    {
                        Dictionary<string, Dictionary<string, string>> infoListV = new Dictionary<string, Dictionary<string, string>>();
                        foreach (var type in reamInfo.Value)
                        {
                            try
                            {
                                Dictionary<string, string> list = new Dictionary<string, string>();
                                string cdn = realm["cdn"].ToString();
                                //string version = realmInfo[typeKey]["version"].ToString();
                                foreach (JProperty item in type.Value["data"])
                                {
                                    string t = type.Key;
                                    if (t == "summoner")
                                        t = "spell";
                                    string url = string.Format("{0}/{1}/img/{2}/{3}", cdn, reamInfo.Key, t, item.Value["image"]["full"].ToString());
                                    list.Add(item.Name, url);
                                }
                                infoListV.Add(type.Key, list);
                            }
                            catch { }
                        }
                        _infoList.Add(reamInfo.Key, infoListV);
                    }
                }

                return _infoList;
            }

        }
        private static Dictionary<string, JObject> _championInfo { get; set; }
        public static Dictionary<string, JObject> championInfo
        {
            get
            {
                if (_championInfo == null)
                {
                    _championInfo = new Dictionary<string, JObject>();
                    string cdn = realm["cdn"].ToString();
                    var reamlnew = realmInfobyVersions.FirstOrDefault();
                    foreach (JProperty item in reamlnew.Value["champion"]["data"])
                    {
                        JObject c = JsonConvert.DeserializeObject<JObject>(GetChampion(cdn, reamlnew.Key, Language, item.Value["id"].ToString()));
                        _championInfo.Add(item.Value["id"].ToString(), c);
                    }



                }
                return _championInfo;
            }
        }

        private static Dictionary<string, JToken> _summonerspell { get; set; }
        public static Dictionary<string, JToken> summonerspell
        {
            get
            {
                if (_summonerspell == null)
                {
                    _summonerspell = new Dictionary<string, JToken>();
                    string cdn = realm["cdn"].ToString();
                    var reamlnew = realmInfobyVersions.FirstOrDefault();
                    foreach (JProperty item in reamlnew.Value["summoner"]["data"])
                    {
                        _summonerspell.Add(item.Value["id"].ToString(), item.Value);
                    }



                }
                return _summonerspell;
            }
        }


        public static List<Skin> championSkins
        {
            get
            {

                var _championSkins = new List<Skin>();

                string cdn = realm["cdn"].ToString();
                foreach (var item in championInfo)
                {
                    var c = item.Value["data"][item.Key];
                    foreach (var skin in c["skins"].Children())
                    {
                        string url = string.Format("{0}/img/champion/loading/{1}_{2}.jpg", cdn, c["id"], skin["num"]);
                        _championSkins.Add(
                            new Skin()
                            {
                                championId = int.Parse(c["key"].ToString()),
                                skinKey = c["id"].ToString() + "_" + skin["num"].ToString(),
                                skinName = skin["name"].ToString(),
                                url = url,
                                skinId = int.Parse(skin["id"].ToString()),
                                txurl = string.Format("http://ossweb-img.qq.com/images/lol/appskin/{0}.jpg", skin["id"])
                            }
                            );
                    }
                }

                return _championSkins;
            }

        }
        public static Dictionary<string, string> championSpells
        {
            get
            {
                Dictionary<string, string> _spells = new Dictionary<string, string>();
                string version = realm["v"].ToString();

                string cdn = realm["cdn"].ToString();
                foreach (var item in championInfo)
                {
                    var c = item.Value["data"][item.Key];

                    foreach (var spell in c["spells"].Children())
                    {
                        string id = spell["id"].ToString();
                        if (!_spells.ContainsKey(id))
                        {
                            string url = string.Format("{0}/{1}/img/spell/{2}", cdn, version, spell["image"]["full"]);
                            _spells.Add(id, url);
                        }
                    }
                }
                return _spells;
            }
        }

        private static List<string> _versions { get; set; }
        public static List<string> versions
        {
            get
            {
                if (_versions == null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Proxy = null;
                        string url = "https://ddragon.leagueoflegends.com/api/versions.json";
                        byte[] result = wc.DownloadData(url);
                        _versions = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(result));
                    }
                }
                return _versions.Take(10).ToList();
            }
        }
        //public static Dictionary<string, string> GetItemUrl()
        //{
        //    if (!infoList.ContainsKey("item"))
        //    {
        //        Dictionary<string, string> list = new Dictionary<string, string>();
        //        string cdn = realm["cdn"].ToString();
        //        //string version = realmInfo["item"]["version"].ToString();
        //        foreach (JProperty item in realmInfo(version)["item"]["data"])
        //        {
        //            string url = string.Format("{0}/{1}/img/{2}/{3}", cdn, version, "item", item.Value["image"]["full"].ToString());
        //            list.Add(item.Name, url);
        //        }
        //        _infoList.Add("item", list);
        //    }
        //    return infoList(version)["item"];
        //}
        private static string GetRealm(string platformId)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                string url = string.Format("https://ddragon.leagueoflegends.com/realms/{0}.json?r={1}", platformId,DateTime.Now);
                byte[] result = wc.DownloadData(url);
                return Encoding.UTF8.GetString(result);
            }
        }
        private static string GetChampion(string cdn, string version, string lang, string championName)
        {
            string file = string.Format("{0}/{1}/data/{2}/champion/{3}", System.Environment.CurrentDirectory, version, lang, championName + ".json");
            string dir = string.Format("{0}/{1}/data/{2}/champion/", System.Environment.CurrentDirectory, version, lang);
            if (!File.Exists(file) || new FileInfo(file).Length == 0)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (WebClient wc = new WebClient())
                {
                    string url = string.Format("{0}/{1}/data/{2}/champion/{3}", cdn, version, lang, championName + ".json");
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
    public class Skin
    {
        public int skinId { get; set; }
        public string skinName { get; set; }
        public string skinKey { get; set; }
        public int championId { get; set; }
        public string url { get; set; }
        public string txurl { get; set; }
    }
}
