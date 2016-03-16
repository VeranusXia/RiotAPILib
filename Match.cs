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
    public class Match
    {

        public static string APIKey { get; set; }
        public static string Region { get; set; }

        public static List<Match> MatchList { get; set; }

        public Match(JToken MatchInfo)
        {

            this.MatchId = MatchInfo["matchId"].ToString();
            if (MatchList == null)
            {
                MatchList = new List<Match>();
            }
            var exist = MatchList.FirstOrDefault(u => u.MatchId == MatchId);
            if (exist == null)
            {
                this.MatchInfo = MatchInfo;
                this.MatchDetail = getMatchDetail();
               // MatchList.Add(this);
            }
        }
        public string MatchId { get; set; }
        public JToken MatchInfo { get; set; }
        public JObject MatchDetail { get; set; }
    
        public JObject getMatchDetail()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = null;
                    string url = string.Format("https://{0}.api.pvp.net/api/lol/{0}/v2.2/match/{1}?includeTimeline=true&api_key={2}", Region, MatchId, APIKey);
                    byte[] result = wc.DownloadData(url);
                    return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(result));
                }
            }
            catch
            {
               Console.WriteLine("429"); return getMatchDetail();
            }
        }
        public List<string> GetSummonerIds()
        {
            return this.MatchDetail["participantIdentities"].Select(u => u["player"]["summonerId"].ToString()).ToList();
        }
    }
}
