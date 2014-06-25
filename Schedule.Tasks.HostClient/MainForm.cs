using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Schedule.Tasks.HostClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["server"]))
                this.txtServer.Text = System.Configuration.ConfigurationManager.AppSettings["server"];
            double refreshInterval = 0;
            double.TryParse(System.Configuration.ConfigurationManager.AppSettings["refreshInterval"], out refreshInterval);
            if (refreshInterval > 0)
                _RefreshInterval = refreshInterval;
        }

        object _MudexObject = new object();

        Schedule.Tasks.Proxy.IRuntimeProxy _Proxy = null;
        System.Timers.Timer _Timer = null;
        double _RefreshInterval = 2000;
        public static string ServerUri { private set; get; }

        delegate void NotParaDelegate();

        private void MainForm_Load(object sender, EventArgs e)
        {
            gridServices.Columns.Add("服务名称", "服务名称");
            gridServices.Columns.Add("服务状态", "服务状态");
            gridServices.Columns.Add("计时器状态", "计时器状态");
            gridServices.Columns.Add("日程安排", "日程安排");
            gridServices.Columns.Add("上次执行", "上次执行");
            gridServices.Columns.Add("下次执行", "下次执行");
            gridServices.Columns.Add("执行类", "执行类");
            gridServices.Columns.Add("计时器间隔", "计时器间隔");
            gridServices.Columns.Add("开始时间", "开始时间");
            gridServices.Columns.Add("结束时间", "结束时间");
            gridServices.Columns.Add("已执行次数", "已执行次数");
            gridServices.Columns.Add("正在执行中", "正在执行中");
        }

        delegate void ShowErrorDelegate(Exception ex);
        bool _ShowingError;
        void ShowError(Exception ex)
        {
            if (_Timer != null)
                _Timer.Stop();
            if (!_ShowingError)
            {
                _ShowingError = true;
                MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ShowingError = false;
            }

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ServerUri = txtServer.Text;
                _Proxy = (Schedule.Tasks.Proxy.IRuntimeProxy)Activator.GetObject(typeof(Schedule.Tasks.Proxy.IRuntimeProxy), txtServer.Text + "/RuntimeProxy");
                LoadServices();
                if (_Timer != null)
                {
                    _Timer.Stop();
                    _Timer.Close();
                }
                _Timer = new System.Timers.Timer(_RefreshInterval);
                _Timer.Elapsed += new System.Timers.ElapsedEventHandler(_Timer_Elapsed);
                _Timer.Start();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LoadServices();
        }

        void LoadServices()
        {
            try
            {
                System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(LoadServicesThread));
                thread.Start();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        void LoadServicesThread()
        {
            lock (_MudexObject)
            {
                try
                {
                    List<string> list = _Proxy.Tasks();
                    this.BeginInvoke(new BindDataDelegate(BindData), list);
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
                }
            }
        }

        Dictionary<string, string> _GridValues = new Dictionary<string, string>();
        Dictionary<string, DataGridViewRow> _GridRows = new Dictionary<string, DataGridViewRow>();

        delegate void BindDataDelegate(List<string> data);
        void BindData(List<string> list)
        {
            try
            {
                Dictionary<string, string[]> tempdict = new Dictionary<string, string[]>();
                List<string> changeds = new List<string>();
                List<string> news = new List<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    string[] s = list[i].Split('|');
                    tempdict[s[0]] = s;
                    if (_GridValues.ContainsKey(s[0]))
                    {
                        if (_GridValues[s[0]] != list[i])
                            changeds.Add(s[0]);
                        else
                            continue;
                    }
                    else
                        news.Add(s[0]);
                    _GridValues[s[0]] = list[i];
                }

                DataGridViewRow[] newRows = new DataGridViewRow[news.Count];
                if (news.Count > 0)
                {
                    for (int i = 0; i < news.Count; i++)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(gridServices, tempdict[news[i]]);
                        _GridRows[news[i]] = newRows[i] = row;
                    }
                    gridServices.Rows.AddRange(newRows);
                }
                for (int i = 0; i < changeds.Count; i++)
                {
                    DataGridViewRow row = _GridRows[changeds[i]];
                    string[] s = tempdict[changeds[i]];
                    for (int j = 0; j < row.Cells.Count && j < s.Length; j++)
                    {
                        row.Cells[j].Value = s[j];
                    }
                }
                string[] keys = _GridValues.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!tempdict.ContainsKey(keys[i]))
                    {
                        DataGridViewRow row = _GridRows[keys[i]];
                        _GridValues.Remove(keys[i]);
                        _GridRows.Remove(keys[i]);
                        gridServices.Rows.Remove(row);
                    }
                }
                gridServices.Update();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewSelectedRowCollection selectedRows = gridServices.SelectedRows;
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("没有选择服务!");
                    return;
                }
                foreach (DataGridViewRow row in selectedRows)
                {
                    _Proxy.ContinueTask(row.Cells[0].Value.ToString());
                }
                LoadServices();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewSelectedRowCollection selectedRows = gridServices.SelectedRows;
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("没有选择服务!");
                    return;
                }
                foreach (DataGridViewRow row in selectedRows)
                {
                    _Proxy.PauseTask(row.Cells[0].Value.ToString());
                }
                LoadServices();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnRunService_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewSelectedRowCollection selectedRows = gridServices.SelectedRows;
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("没有选择服务!");
                    return;
                }
                foreach (DataGridViewRow row in selectedRows)
                {
                    System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RunService));
                    thread.Start(row.Cells[0].Value);
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        void RunService(object key)
        {
            _Proxy.RunTask(string.Format("{0}", key));
        }

        private void gridServices_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            new RuntimeLog().Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_Timer != null)
            {
                _Timer.Stop();
                _Timer.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}
