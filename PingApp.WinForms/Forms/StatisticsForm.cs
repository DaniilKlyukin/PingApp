using PingApp.Application.Features.Statistics;
using PingApp.Application.Features.Statistics.Common;
using ScottPlot;

namespace PingApp.WinForms
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
            RenderStatistics(statistics);
        }

        private void RenderStatistics(List<UserStatistics> statistics)
        {
            formsPlot.Plot.Clear();

            var activeItems = statistics.Where(t => t.Statuses.Count >= 1).ToList();

            if (activeItems.Count == 0)
            {
                formsPlot.Plot.Title("Недостаточно данных для построения графика.");
                formsPlot.Refresh();
                return;
            }

            foreach (var stat in activeItems)
            {
                var orderedStatuses = stat.Statuses
                    .Select(s => new WorkStatus(s.DateTime.ToLocalTime(), s.AtWork))
                    .OrderBy(s => s.DateTime)
                    .ToList();

                if (orderedStatuses.Count > 0)
                {
                    var lastKnownStatus = orderedStatuses[^1];
                    orderedStatuses.Add(new WorkStatus(DateTime.Now, lastKnownStatus.AtWork));
                }

                var xs = orderedStatuses.Select(s => s.DateTime.ToOADate()).ToArray();
                var ys = orderedStatuses.Select(s => s.AtWork ? 1.0 : 0.0).ToArray();

                var name = stat.Address;
                if (!string.IsNullOrEmpty(stat.Nickname))
                {
                    name += $" ({stat.Nickname})";
                }

                var scatter = formsPlot.Plot.Add.Scatter(xs, ys);

                scatter.ConnectStyle = ConnectStyle.StepHorizontal;
                scatter.MarkerSize = 4;

                scatter.LegendText = name;
            }

            formsPlot.Plot.Axes.DateTimeTicksBottom();

            if (formsPlot.Plot.Axes.Bottom.TickGenerator is ScottPlot.TickGenerators.DateTimeAutomatic dtGen)
            {
                dtGen.LabelFormatter = dt => dt.ToString("dd.MM.yyyy\nHH:mm");
            }

            var yPositions = new double[] { 0, 1 };
            var yLabels = new string[] { "Не в сети", "В сети" };

            formsPlot.Plot.Axes.Left.SetTicks(yPositions, yLabels);
            formsPlot.Plot.Axes.SetLimitsY(-0.2, 1.2);
            formsPlot.Plot.ShowLegend(Alignment.UpperLeft);

            formsPlot.Refresh();
        }
    }
}