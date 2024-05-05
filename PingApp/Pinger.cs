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

        public Pinger(IList<UserStatistics> statistics)
        {
            this.statistics = statistics.ToDictionary(
                k => k.Address,
                v =>
                {
                    var stack = new Stack<WorkStatus>();

                    foreach (var status in v.Statuses)
                        stack.Push(status);

                    return stack;
                });

            foreach (var stat in statistics)
            {
                if (!string.IsNullOrEmpty(stat.Nickname))
                    AddNickname(stat.Address, stat.Nickname);
            }
        }

        public event UserLogged? OnLoggedIn;
        public event UserLogged? OnLoggedOut;

        private Dictionary<string, Stack<WorkStatus>> statistics = new();
        private Dictionary<string, string> addressNicknames = new();

        public IEnumerator<UserStatistics> GetEnumerator()
        {
            foreach (var (address, statuses) in statistics)
            {
                yield return new UserStatistics
                {
                    Address = address,
                    Nickname = GetNickname(address),
                    Statuses = statuses.Reverse().ToList()
                };
            }
        }

        public bool TryGetUserLastTimeAtWork(string nameOrAddress, out bool atWork, out DateTime lastTimeAtWork, out TimeSpan dt)
        {
            if (!statistics.ContainsKey(nameOrAddress) || !statistics[nameOrAddress].Any())
            {
                lastTimeAtWork = DateTime.MinValue;
                dt = TimeSpan.MinValue;
                atWork = false;
                return false;
            }

            var stack = statistics[nameOrAddress];

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

        public void AddUser(string address)
        {
            if (!statistics.ContainsKey(address))
                statistics.Add(address, new Stack<WorkStatus>());
        }

        public void RemoveUser(string address)
        {
            if (statistics.ContainsKey(address))
            {
                statistics.Remove(address);
                RemoveNickname(address);
            }
        }

        public void AddNickname(string address, string nickname)
        {
            if (!addressNicknames.ContainsKey(address))
                addressNicknames.Add(address, nickname);
            else
                addressNicknames[address] = nickname;
        }

        public void RemoveNickname(string address)
        {
            if (addressNicknames.ContainsKey(address))
                addressNicknames.Remove(address);
        }

        public string? GetNickname(string address)
        {
            return addressNicknames.GetValueOrDefault(address);
        }

        public List<WorkStatus> GetUserStatuses(string address)
        {
            return statistics[address].Reverse().ToList();
        }

        public async void UpdateStatistics()
        {
            var userStatusTasks = new List<Task<UserStatus>>();

            foreach (var (nameOrAddress, stack) in statistics)
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
            foreach (var (_, stack) in statistics)
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

        /// <summary>
        /// Сохранить статус пользователя. При сохранении данные сжимаются таким образом, чтобы хранить только изменения статуса, 
        /// т.е. только моменты когда пользователь заходит или выходит из сети.
        /// </summary>
        /// <param name="userStatus"></param>
        private void addStatistic(UserStatus userStatus)
        {
            if (!statistics.ContainsKey(userStatus.Address))
            {
                var stack = new Stack<WorkStatus>();
                stack.Push(userStatus.WorkStatus);
                statistics.Add(userStatus.Address, stack);
            }
            else
            {
                var stack = statistics[userStatus.Address];

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
