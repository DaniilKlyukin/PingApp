using Newtonsoft.Json;

namespace PingApp
{
    public static class PingerJson
    {
        public static string Serialize(Pinger pinger)
        {
            var statistics = new List<UserStatistics>();

            foreach (var stat in pinger)
            {
                statistics.Add(stat);
            }

            return JsonConvert.SerializeObject(statistics, Formatting.Indented);
        }

        public static Pinger Deserialize(string json)
        {
            var stat = JsonConvert.DeserializeObject<List<UserStatistics>>(json);

            if (stat == null)
            {
                throw new JsonException($"Can`t deserialize Pinger:\n{json}");
            }

            return new Pinger(stat);
        }
    }
}
