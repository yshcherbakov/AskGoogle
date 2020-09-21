using System;
using System.Windows.Forms;
using WebQuery.Agents;

namespace WebQuery.Test.Harness
{
    public partial class MainForm : Form
    {
        private GoogleAgent _agent;

        public MainForm()
        {
            InitializeComponent();

            _agent = new GoogleAgent(webBrowser1);

            //_agent = new GoogleAgent(null);

            _agent.ItemComplete += (s, arg) =>
            {
                if (arg.Result == null || arg.Result.Count == 0) return;

                var line = string.Empty;

                foreach (var item in arg.Result)
                {
                    line += $"{item.Key}\t{item.Value}\t";
                }

                tbResults.AppendText($"{arg.Item}\t{line}\r\n");
            };

            _agent.Complete += (s, arg) => {

                btnSearch.Enabled = true;
                Cursor = Cursors.Default;

                tbQueryText.Focus();
            };
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbQueryText.Text)) return;

                btnSearch.Enabled = false;
                Cursor = Cursors.WaitCursor;
                    
                _agent.Query(tbQueryText.Text.Split('\n'), SearchContext.Company);

            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(ex.Message);
            }
        }
    }
}
