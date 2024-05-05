namespace PingApp
{
    public class UserStatus
    {
        public WorkStatus WorkStatus { get; set; }
        public string Address { get; set; }

        public UserStatus(
            DateTime dateTime,
            string nameOrAddress,
            bool atWork)
        {
            WorkStatus = new WorkStatus(dateTime, atWork);
            Address = nameOrAddress;
        }

        public override string ToString()
        {
            return $"{WorkStatus.DateTime}\t{Address}\t{WorkStatus.AtWork}";
        }

        public static UserStatus FromString(string str)
        {
            var arr = str.Split("\t");
            var dateTime = DateTime.Parse(arr[0]);
            var name = arr[1];
            var atWork = bool.Parse(arr[2]);
            return new UserStatus(dateTime, name, atWork);
        }
    }
}