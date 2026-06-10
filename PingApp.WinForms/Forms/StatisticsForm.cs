using PingApp.Application.Features.Statistics;
using PingApp.WinForms.Models;
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

            var activeItems = statistics.Where(t => t.Statuses.Count >= 2).ToList();

            if (activeItems.Count == 0)
            {
                formsPlot.Plot.Title("Недостаточно данных для построения графика.");
                formsPlot.Refresh();
                return;
            }

            foreach (var stat in activeItems)
            {
                double[] xs = stat.Statuses.Select(s => s.DateTime.ToOADate()).ToArray();
                double[] ys = stat.Statuses.Select(s => s.AtWork ? 1.0 : 0.0).ToArray();

                string name = stat.Address;
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

            double[] yPositions = { 0, 1 };
            string[] yLabels = { "Не в сети", "В сети" };

            formsPlot.Plot.Axes.Left.SetTicks(yPositions, yLabels);

            formsPlot.Plot.Axes.SetLimitsY(-0.2, 1.2);

            formsPlot.Plot.ShowLegend(Alignment.UpperLeft);

            formsPlot.Refresh();
        }
    }
}