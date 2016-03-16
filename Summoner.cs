using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading;

namespace RiotAPILib
{
    public class Summoner
    {
        public static string APIKey { get; set; }
        public static string Region { get; set; }

        public static JObject SearchSummoner(ref int statusCode, string name)
        {
            string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.4/summoner/by-name/{1}?api_key={2}", Region, name, APIKey);
            using (WebClient wc = new WebClient())
            {
                byte[] result = wc.DownloadData(url);
                JObject obj = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
                try
                {
                    statusCode = int.Parse(obj["status"]["status_code"].ToString());
                    return obj;
                }
                catch
                {
                    statusCode = 0;
                    return obj;
                }
            }
        }
        public static JObject SearchSummoner(ref int statusCode, long id)
        {
            string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.4/summoner/{1}?api_key={2}", Region, id, APIKey);
            using (WebClient wc = new WebClient())
            {
                byte[] result = wc.DownloadData(url);
                JObject obj = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
                try
                {
                    statusCode = int.Parse(obj["status"]["status_code"].ToString());
                    return obj;
                }
                catch
                {
                    statusCode = 0;
                    return obj;
                }
            }
        }
        public static List<JObject> SearchSummoners(string[] ids)
        {
            List<string> nids = new List<string>();
            foreach (var item in ids)
            {
                if (SummonerList.FirstOrDefault(u => u.id == item) == null)
                    nids.Add(item);
            }
            ids = nids.ToArray();
            string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.4/summoner/{1}?api_key={2}", Region, string.Join(",", ids), APIKey);
            using (WebClient wc = new WebClient())
            {
                byte[] result = wc.DownloadData(url);
                try
                {
                    List<JObject> obj = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result)).Values<JObject>().ToList();
                    return obj;
                }
                catch
                {
                    return new List<JObject>();
                }
            }
        }


        public static List<Summoner> SummonerList { get; set; }

        public string id { get; set; }
        public JObject SummonerInfo { get; set; }
        public JObject Ranked { get; set; }
        public JObject Summary { get; set; }
        public JObject Matchlist { get; set; }
        public JObject LeagueTier { get; set; }


        public Summoner(JObject summonerInfo, bool preLoad = true)
        {
            this.id = summonerInfo.Values().FirstOrDefault()["id"].ToString();
            if (SummonerList == null)
            {
                SummonerList = new List<Summoner>();
            }
            var exist = SummonerList.FirstOrDefault(u => u.id == id);
            if (exist == null)
            {
                this.SummonerInfo = summonerInfo;
                if (preLoad)
                {
                    this.Ranked = getRanked();
                    this.Summary = getSummary();
                    this.Matchlist = getMatchlist();
                    this.LeagueTier = getLeagueTier();
                }
                SummonerList.Add(this);
            }


        }
        public Summoner(string id)
        {
            this.id = id;
            if (SummonerList == null)
            {
                SummonerList = new List<Summoner>();
            }
            var exist = SummonerList.FirstOrDefault(u => u.id == id);
            if (exist == null)
            {
                //this.Ranked = getRanked();
                //this.Summary = getSummary();
                this.Matchlist = getMatchlist();
                //this.LeagueTier = getLeagueTier();
                SummonerList.Add(this);
            }
            //else
            //{

            //    this.SummonerInfo = exist.SummonerInfo;
            //    this.Ranked = exist.Ranked;
            //    this.Summary = exist.Summary;
            //    this.Matchlist = exist.Matchlist;
            //    this.LeagueTier = exist.LeagueTier;
            //}

        }

        public JObject getRanked()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.3/stats/by-summoner/{1}/ranked?api_key={2}", Region, id, APIKey);
                byte[] result = wc.DownloadData(url);
                return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
            }
        }
        public JObject getSummary()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.3/stats/by-summoner/{1}/summary?api_key={2}", Region, id, APIKey);
                byte[] result = wc.DownloadData(url);
                return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
            }
        }
        public JObject getMatchlist()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = null;
                    string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v2.2/matchlist/by-summoner/{1}?seasons=PRESEASON2016&api_key={2}", Region, id, APIKey);
                    byte[] result = wc.DownloadData(url);
                    return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
                }
            }
            catch
            {
                Thread.Sleep(1000);
                return getMatchlist();
            }
        }
        public JObject getLeagueTier()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Proxy = null;
                string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v2.5/league/by-summoner/{1}?api_key={2}", Region, id, APIKey);
                byte[] result = wc.DownloadData(url);
                return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
            }
        }
    }
}
