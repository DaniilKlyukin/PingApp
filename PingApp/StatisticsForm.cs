using System.Data;

namespace PingApp
{

    public partial class StatisticsForm : Form
    {
        public StatisticsForm()
        {
            InitializeComponent();
        }

        public StatisticsForm(List<UserStatistics> statistics)
        {
            InitializeComponent();

            formsPlot.Plot.Clear();

            foreach (var stat in statistics.Where(t => t.Statuses.Count >= 2))
            {
                var xs = stat.Statuses.Select(s => s.DateTime.ToOADate()).ToArray();
                var ys = stat.Statuses.Select(s => s.AtWork ? 1.0 : 0.0).ToArray();

                var name = $"{stat.Address}";
                if (!string.IsNullOrEmpty(stat.Nickname))
                    name += $" ({stat.Nickname})";

                formsPlot.Plot.AddScatter(xs, ys, label: name);
            }

            formsPlot.Plot.XAxis.DateTimeFormat(true);
            // manually define Y axis tick positions and labels
            double[] yPositions = { 0, 1 };
            string[] yLabels = { "Не в сети", "В сети" };
            formsPlot.Plot.YAxis.ManualTickPositions(yPositions, yLabels);
            formsPlot.Plot.Legend();

            formsPlot.Render();
        }
    }
}
