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
    public partial class RuntimeLog : Form
    {
        public RuntimeLog()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        object _MudexObject = new object();
        Schedule.Tasks.Proxy.ILogReader _Proxy = null;

        int _PageIndex = 0;
        int _PageSize = 16;
        string _LogType = "ALL";

        private void RuntimeLog_Load(object sender, EventArgs e)
        {
            cmbType.SelectedIndex = 0;
            gridLogs.Columns.Add("ID", "ID");
            gridLogs.Columns.Add("记录者", "记录者");
            gridLogs.Columns.Add("类型", "类型");
            gridLogs.Columns.Add("时间", "时间");
            gridLogs.Columns.Add("内容", "内容");
            gridLogs.Columns.Add("栈", "栈");
            try
            {
                _Proxy = (Schedule.Tasks.Proxy.ILogReader)Activator.GetObject(typeof(Schedule.Tasks.Proxy.ILogReader), MainForm.ServerUri + "/SQLiteLogReader");
                LoadLogs();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        void LoadLogs()
        {
            try
            {
                System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(LoadLogsThread));
                thread.Start();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        void LoadLogsThread()
        {
            lock (_MudexObject)
            {
                try
                {
                    IList<string> list = null;
                    if (_LogType == "ALL")
                        list = _Proxy.Read(_PageIndex * _PageSize, _PageSize);
                    else
                        list = _Proxy.Read(_LogType, _PageIndex * _PageSize, _PageSize);
                    this.BeginInvoke(new BindDataDelegate(BindData), list);
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
                }
            }
        }

        delegate void BindDataDelegate(IList<string> data);
        void BindData(IList<string> list)
        {
            try
            {
                gridLogs.Rows.Clear();
                if (list != null && list.Count > 0)
                {
                    DataGridViewRow[] newRows = new DataGridViewRow[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        string[] arr = list[i].Split(new String[] { "|[LOGBY]:", "|[LOGTYPE]:", "|[LOGTIME]:", "|[CONTENT]:", "|[STACKTRACE]:" }, StringSplitOptions.None);
                        arr[0] = arr[0].Replace("[ID]:", "");
                        row.CreateCells(gridLogs, arr);
                        newRows[i] = row;
                    }
                    gridLogs.Rows.AddRange(newRows);
                }
                gridLogs.Update();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        delegate void ShowErrorDelegate(Exception ex);
        bool _ShowingError;
        void ShowError(Exception ex)
        {
            try
            {
                if (!_ShowingError)
                {
                    _ShowingError = true;
                    MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _ShowingError = false;
                }
            }
            finally { }
        }

        private void btnReflesh_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_MudexObject)
                {
                    _Proxy = (Schedule.Tasks.Proxy.ILogReader)Activator.GetObject(typeof(Schedule.Tasks.Proxy.ILogReader), MainForm.ServerUri + "/SQLiteLogReader");
                    _PageIndex = 0;
                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_MudexObject)
                {
                    if (_PageIndex > 0)
                        _PageIndex--;
                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_MudexObject)
                {
                    if (gridLogs.Rows.Count == _PageSize)
                        _PageIndex++;
                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            try
            {
                if (_LogType == "ALL")
                    _Proxy.Clean();
                else
                    _Proxy.Clean(_LogType);
                lock (_MudexObject)
                {
                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewSelectedRowCollection selectedRows = gridLogs.SelectedRows;
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("没有选择记录!");
                    return;
                }
                string[] ids = new string[selectedRows.Count];
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    ids[i] = selectedRows[i].Cells[0].Value.ToString();
                }
                _Proxy.Delete(ids);
                lock (_MudexObject)
                {
                    LoadLogs();
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), ex);
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _LogType = (string)cmbType.SelectedItem;
        }

    }
}
