using Newtonsoft.Json;

namespace PingApp
{
    public static class PingerJson
    {
        public static string Serialize(Pinger pinger)
        {
            return JsonConvert.SerializeObject(pinger.Statistics, Formatting.Indented);
        }

        public static Pinger Deserialize(string json)
        {
            var stat = JsonConvert.DeserializeObject<Dictionary<string, Stack<WorkStatus>>>(json);

            if (stat == null)
            {
                throw new JsonException($"Can`t deserialize Pinger:\n{json}");
            }

            var keys = stat.Keys.ToArray();
            for (int i = 0; i < stat.Count; i++)
            {
                stat[keys[i]] = new Stack<WorkStatus>(stat[keys[i]]);
            }

            return new Pinger(stat);
        }
    }
}
