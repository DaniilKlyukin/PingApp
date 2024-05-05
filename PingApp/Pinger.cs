using System.Diagnostics;
using System.Net.NetworkInformation;

namespace PingApp
{
    public delegate void UserLogged(UserStatus status);

    public class Pinger
    {
        public Pinger()
        {

        }

        public Pinger(Dictionary<string, Stack<WorkStatus>> statistics)
        {
            Statistics = statistics;
        }

        public event UserLogged? OnLoggedIn;
        public event UserLogged? OnLoggedOut;

        public Dictionary<string, Stack<WorkStatus>> Statistics { get; private set; } = new();

        public IEnumerator<KeyValuePair<string, Stack<WorkStatus>>> GetEnumerator()
        {
            foreach (var keyValuePair in Statistics)
            {
                yield return keyValuePair;
            }
        }

        public bool TryGetUserLastTimeAtWork(string nameOrAddress, out bool atWork, out DateTime lastTimeAtWork, out TimeSpan dt)
        {
            if (!Statistics.ContainsKey(nameOrAddress) || !Statistics[nameOrAddress].Any())
            {
                lastTimeAtWork = DateTime.MinValue;
                dt = TimeSpan.MinValue;
                atWork = false;
                return false;
            }

            var stack = Statistics[nameOrAddress];

            var latestStatus = stack.First();

            if (latestStatus.AtWork)
            {
                lastTimeAtWork = latestStatus.DateTime;
                dt = TimeSpan.Zero;
                atWork = true;
                return true;
            }

            foreach (var item in stack)
            {
                if (item.AtWork)
                {
                    lastTimeAtWork = item.DateTime;
                    dt = latestStatus.DateTime - item.DateTime;
                    atWork = false;
                    return true;
                }
            }

            lastTimeAtWork = DateTime.MinValue;
            dt = TimeSpan.MinValue;
            atWork = false;
            return false;
        }

        public void AddUser(string nameOrAddress)
        {
            if (!Statistics.ContainsKey(nameOrAddress))
                Statistics.Add(nameOrAddress, new Stack<WorkStatus>());
        }

        public void RemoveUser(string nameOrAddress)
        {
            if (Statistics.ContainsKey(nameOrAddress))
                Statistics.Remove(nameOrAddress);
        }

        public List<WorkStatus> GetUserStatuses(string nameOrAddress)
        {
            return Statistics[nameOrAddress].ToList();
        }

        public async void UpdateStatistics()
        {
            var userStatusTasks = new List<Task<UserStatus>>();

            foreach (var (nameOrAddress, stack) in Statistics)
            {
                userStatusTasks.Add(
                    Task.Run(
                        () => new UserStatus(DateTime.Now, nameOrAddress, pingHost(nameOrAddress))));
            }

            var userStatuses = await Task.WhenAll(userStatusTasks);

            foreach (var userStatus in userStatuses)
            {
                addStatistic(userStatus);
            }
        }

        public void ClearSatistics()
        {
            foreach (var (_, stack) in Statistics)
            {
                stack.Clear();
            }
        }

        private bool pingHost(string nameOrAddress)
        {
            var ping = new Ping();
            try
            {
                PingReply reply = ping.Send(nameOrAddress);

                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private void addStatistic(UserStatus userStatus)
        {
            if (!Statistics.ContainsKey(userStatus.NameOrAddress))
            {
                var stack = new Stack<WorkStatus>();
                stack.Push(userStatus.WorkStatus);
                Statistics.Add(userStatus.NameOrAddress, stack);
            }
            else
            {
                var stack = Statistics[userStatus.NameOrAddress];

                if (stack.Count >= 2)
                {
                    var lastStatus = stack.Pop();
                    var preLastStatus = stack.Pop();

                    if (preLastStatus.AtWork == userStatus.WorkStatus.AtWork)
                    {
                        stack.Push(preLastStatus);
                        stack.Push(userStatus.WorkStatus);
                    }
                    else
                    {
                        stack.Push(preLastStatus);
                        stack.Push(lastStatus);
                        stack.Push(userStatus.WorkStatus);
                    }

                    if (!lastStatus.AtWork && userStatus.WorkStatus.AtWork)
                    {
                        OnLoggedIn?.Invoke(userStatus);
                    }
                    else if (lastStatus.AtWork && !userStatus.WorkStatus.AtWork)
                    {
                        OnLoggedOut?.Invoke(userStatus);
                    }
                }
                else
                {
                    stack.Push(userStatus.WorkStatus);
                }
            }
        }
    }
}
