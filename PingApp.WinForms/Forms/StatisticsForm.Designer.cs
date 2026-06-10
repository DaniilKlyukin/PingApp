namespace PingApp.WinForms
{
    partial class StatisticsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            formsPlot = new ScottPlot.WinForms.FormsPlot();
            SuspendLayout();
            // 
            // formsPlot
            // 
            formsPlot.Dock = DockStyle.Fill;
            formsPlot.Location = new Point(0, 0);
            formsPlot.Margin = new Padding(4, 3, 4, 3);
            formsPlot.Name = "formsPlot";
            formsPlot.Size = new Size(800, 450);
            formsPlot.TabIndex = 0;
            // 
            // StatisticsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(formsPlot);
            Name = "StatisticsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Статистика";
            ResumeLayout(false);
        }

        #endregion

        private ScottPlot.WinForms.FormsPlot formsPlot;
    }
}