using System.Runtime.Serialization;

namespace PingApp
{
    [Serializable]
    public class WorkStatus
    {
        [DataMember(Name = "Дата-время")]
        public DateTime DateTime { get; set; }

        [DataMember(Name = "В сети")]
        public bool AtWork { get; set; }

        public WorkStatus(
            DateTime dateTime,
            bool atWork)
        {
            DateTime = dateTime;
            AtWork = atWork;
        }

        public override string ToString()
        {
            return $"{DateTime}\t{AtWork}";
        }
    }
}