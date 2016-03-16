using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiotAPILib
{
    public class Init
    {
        private static void SetAPIKey(string apiKey)
        {
            AllInfo.APIKey = BaseInfo.APIKey = Summoner.APIKey = Match.APIKey = apiKey;
        }
        private static void SetRegion(string region)
        {
            AllInfo.Region = BaseInfo.Region = Summoner.Region = Match.Region = region;
        }
        public static void InitLib(string apikey, string region, string language)
        {
            SetAPIKey(apikey);
            SetRegion(region);
            AllInfo.Language = BaseInfo.Language = language;
        }
    }
}
