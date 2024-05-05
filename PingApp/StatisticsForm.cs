using System.Data;

namespace PingApp
{
    public partial class StatisticsForm : Form
    {
        public StatisticsForm()
        {
            InitializeComponent();
        }

        public StatisticsForm(List<(string, List<WorkStatus>)> data)
        {
            InitializeComponent();

            formsPlot.Plot.Clear();

            foreach (var (address, list) in data.Where(t => t.Item2.Count >= 2))
            {
                var xs = list.Select(s => s.DateTime.ToOADate()).ToArray();
                var ys = list.Select(s => s.AtWork ? 1.0 : 0.0).ToArray();

                formsPlot.Plot.AddScatter(xs, ys, label: address);
            }

            formsPlot.Plot.XAxis.DateTimeFormat(true);
            formsPlot.Plot.Legend();

            formsPlot.Render();
        }
    }
}
