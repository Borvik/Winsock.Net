using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Treorisoft.Net;
using Treorisoft.Net.Utilities;

namespace WinsockDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();
            AddCloseButtonColumn();
            AddColumn("RemoteHost", "IP", typeof(string));
            AddColumn("State", "State", typeof(State));
            AddSendFileButtonColumn();
        }

        private void AddColumn(string propertyName, string headerText, Type dataType)
        {
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = propertyName;
            column.HeaderText = headerText;
            column.Name = string.Format("col{0}", propertyName);
            column.ValueType = dataType;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dgv.Columns.Add(column);
        }
        private void AddCloseButtonColumn()
        {
            var column = new DataGridViewButtonColumn()
            {
                HeaderText = "",
                Name = "colCloseClient",
                Text = "Close",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader
            };
            dgv.Columns.Add(column);
        }
        private void AddSendFileButtonColumn()
        {
            var column = new DataGridViewButtonColumn()
            {
                HeaderText = "",
                Name = "colSendFile",
                Text = "Send File",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader
            };
            dgv.Columns.Add(column);
        }

        #region Server

        private bool ServerLog { get { return chkLogServer.Checked; } }
        private void EvalServerButtons()
        {
            bool anyConnected = wskServer.Clients.Count > 0 && wskServer.Clients.Values.Cast<Winsock>().Any(w => w.State == State.Connected);
            cmdServerSend.Enabled = anyConnected;
            txtServerSend.Enabled = anyConnected;
        }
        private void RefreshClients()
        {
            dgv.DataSource = wskServer.Clients.Values.ToList();
            dgv.AutoResizeColumns();
        }

        private void cmdServerListen_Click(object sender, EventArgs e)
        {
            if(cmdServerListen.Text == "Listen")
            {
                if (ServerLog) LogServer("Parent: Listen()");
                wskServer.Listen();
            }
            else
            {
                if (ServerLog) LogServer("Parent: Close()");
                wskServer.Close();
            }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if(e.ColumnIndex == dgv.Columns["colCloseClient"].Index)
            {
                if (ServerLog) LogServer("Client: Close()");
                var row = dgv.Rows[e.RowIndex] as DataGridViewRow;
                var client = row.DataBoundItem as Winsock;
                client.Close();
            }
            else if (e.ColumnIndex == dgv.Columns["colSendFile"].Index)
            {
                if (dlgOpen.ShowDialog(this) != DialogResult.OK) return;

                LogServer(string.Format("Client: Sending file \"{0}\"", dlgOpen.FileName));
                var row = dgv.Rows[e.RowIndex] as DataGridViewRow;
                var client = row.DataBoundItem as Winsock;
                client.SendFile(dlgOpen.FileName);
            }
        }

        #region Server Events

        private void wskServer_ClientCountChanged(object sender, EventArgs e)
        {
            EvalServerButtons();
            RefreshClients();
        }

        private void wskServer_Connected(object sender, Treorisoft.Net.ConnectedEventArgs e)
        {
            string source = (sender == wskServer) ? "Parent" : "Client";
            if (ServerLog) LogServer(string.Format("{0}: Connected ({1} on port {2})", source, e.RemoteIP, e.RemotePort));
        }

        private void wskServer_ConnectionRequest(object sender, Treorisoft.Net.ConnectionRequestEventArgs e)
        {
            if (ServerLog) LogServer(string.Format("Parent: Connection Request ({0})", e.RemoteIP));
            if (e.RemoteIP == "bad IP")
            {
                e.Cancel = true; //Cancels a blacklisted IP address
            }
            else
                wskServer.Accept(e.Client);
        }

        private void wskServer_DataArrival(object sender, Treorisoft.Net.DataArrivalEventArgs e)
        {
            string source = (sender == wskServer) ? "Parent" : "Client";
            if (ServerLog) LogServer(string.Format("{0} Data Arrival ({1}: {2} bytes)", source, e.RemoteIP, e.TotalBytes));
            var data = (sender as Winsock).Get();
            if (data.GetType() == typeof(string))
                LogServer(string.Format("{0} Received: {1}", source, data));
            else if (data.GetType() == typeof(FileData))
            {
                LogServer(string.Format("{0} File Received: {1}", source, ((FileData)data).FileName));
                HandleIncomingFile((FileData)data);
            }
            else
                LogServer(string.Format("{0} Data Arrived: {1}", source, data.GetType()));
        }

        private void wskServer_Disconnected(object sender, EventArgs e)
        {
            string source = (sender == wskServer) ? "Parent" : "Client";
            if (ServerLog) LogServer(string.Format("{0}: Disconnected", source));
        }

        private void wskServer_ErrorReceived(object sender, Treorisoft.Net.ErrorReceivedEventArgs e)
        {
            string from = (sender == wskServer) ? "Parent" : "Client";
            LogServer(string.Format("{0}: Error {1}({2}): {3}", from, e.Function, e.SocketError, e.Message));
        }

        private void wskServer_ReceiveProgress(object sender, Treorisoft.Net.ReceiveProgressEventArgs e)
        {
            pbServerReceive.Minimum = 0;
            pbServerReceive.Maximum = 100;
            pbServerReceive.Value = (int)(100 * e.PercentComplete);
        }

        private void wskServer_SendProgress(object sender, Treorisoft.Net.SendProgressEventArgs e)
        {
            pbServerSend.Minimum = 0;
            pbServerSend.Maximum = 100;
            pbServerSend.Value = (int)(100 * e.PercentComplete);
        }

        private void wskServer_StateChanged(object sender, Treorisoft.Net.StateChangedEventArgs e)
        {
            if(sender == wskServer)
            {
                if (ServerLog) LogServer(string.Format("Parent: State Change ({0})", e.NewState));
                grpServer.Text = string.Format("Server ({0})", e.NewState);
                switch (wskServer.State)
                {
                    case State.Listening:
                        cmdServerListen.Text = "Close";
                        cmdServerListen.Enabled = true;
                        break;
                    case State.Closed:
                        cmdServerListen.Text = "Listen";
                        cmdServerListen.Enabled = true;
                        break;
                    default:
                        cmdServerListen.Enabled = false;
                        break;
                }
            }
            else
            {
                EvalServerButtons();
                if (ServerLog) LogServer(string.Format("Client: State Changed ({0})", e.NewState));
                RefreshClients();
            }
        }

        #endregion

        #endregion

        #region Client

        private bool ClientLog { get { return chkLogClient.Checked; } }

        private void cmdClientConnect_Click(object sender, EventArgs e)
        {
            if(cmdClientConnect.Text == "Connect")
            {
                if (ClientLog) LogClient(string.Format("Connect ({0}:{1})", wskClient.RemoteHost, wskClient.RemotePort));
                wskClient.Connect();
            }
            else
            {
                if (ClientLog) LogClient("Close()");
                wskClient.Close();
            }
        }

        #region Client Events

        private void wskClient_DataArrival(object sender, DataArrivalEventArgs e)
        {
            if (ClientLog) LogClient(string.Format("Data Arrival ({0}: {1} bytes)", e.RemoteIP, e.TotalBytes));
            object data = (wskClient.LegacySupport) ? wskClient.Get<string>() : wskClient.Get();
            if (data.GetType() == typeof(string))
                LogClient(string.Format("Received: {0}", data));
            else if (data.GetType() == typeof(FileData))
            {
                LogClient(string.Format("File Received: {0}", ((FileData)data).FileName));
                HandleIncomingFile((FileData)data);
            }
            else
                LogClient(string.Format("Data Arrived: {0}", data.GetType()));
        }

        private void wskClient_Disconnected(object sender, EventArgs e)
        {
            if (ClientLog) LogClient("Disconnected");
        }

        private void wskClient_Connected(object sender, Treorisoft.Net.ConnectedEventArgs e)
        {
            if (ClientLog) LogClient(string.Format("Connected ({0} on port {1})", e.RemoteIP, e.RemotePort));
        }

        private void wskClient_ErrorReceived(object sender, ErrorReceivedEventArgs e)
        {
            LogClient(string.Format("Error {0}({1}): {2}", e.Function, e.SocketError, e.Message));
        }

        private void wskClient_ReceiveProgress(object sender, ReceiveProgressEventArgs e)
        {
            pbClientReceive.Minimum = 0;
            pbClientReceive.Maximum = 100;
            pbClientReceive.Value = (int)(100 * e.PercentComplete);
        }

        private void wskClient_SendProgress(object sender, SendProgressEventArgs e)
        {
            pbClientSend.Minimum = 0;
            pbClientSend.Maximum = 100;
            pbClientSend.Value = (int)(100 * e.PercentComplete);
        }

        private void wskClient_StateChanged(object sender, StateChangedEventArgs e)
        {
            if (ClientLog) LogClient(string.Format("State Changed: {0}", e.NewState));
            grpClient.Text = string.Format("Client ({0})", e.NewState);
            switch (wskClient.State)
            {
                case State.Closed:
                    cmdClientConnect.Text = "Connect";
                    cmdClientConnect.Enabled = true;
                    cmdClientSendFile.Enabled = false;
                    cmdClientSend.Enabled = false;
                    txtClientSend.Enabled = false;
                    break;
                case State.Connected:
                    cmdClientConnect.Text = "Close";
                    cmdClientConnect.Enabled = true;
                    cmdClientSendFile.Enabled = true;
                    cmdClientSend.Enabled = true;
                    txtClientSend.Enabled = true;
                    break;
                default:
                    cmdClientConnect.Enabled = false;
                    cmdClientSendFile.Enabled = false;
                    cmdClientSend.Enabled = false;
                    txtClientSend.Enabled = false;
                    break;
            }
        }

        #endregion

        #endregion

        #region Logging

        private void LogClient(string str) { Log(txtClientLog, str); }
        private void LogServer(string str) { Log(txtServerLog, str); }
        private void Log(TextBox txt, string str)
        {
            txt.AppendText(str + Environment.NewLine);
            txt.Select(txt.TextLength - 1, 0);
            txt.ScrollToCaret();
        }

        #endregion

        private void cmdServerSend_Click(object sender, EventArgs e)
        {
            string message = txtServerSend.Text.Trim();
            if(message != "")
            {
                if (ServerLog) LogServer(string.Format("Client: Sending string \"{0}\"", message));
                wskServer.Clients.SendToAll(message);
                txtServerSend.Text = string.Empty;
                txtServerSend.Focus();
            }
        }

        private void cmdClientSend_Click(object sender, EventArgs e)
        {
            string message = txtClientSend.Text.Trim();
            if(message != "")
            {
                if (ClientLog) LogClient(string.Format("Sending string \"{0}\"", message));
                wskClient.Send(message);
                txtClientSend.Text = string.Empty;
                txtClientSend.Focus();
            }
        }

        private void txtServerSend_Enter(object sender, EventArgs e)
        {
            AcceptButton = cmdServerSend;
        }

        private void txtClientSend_Enter(object sender, EventArgs e)
        {
            AcceptButton = cmdClientSend;
        }

        private void HandleIncomingFile(FileData file)
        {
            dlgSave.FileName = file.FileName;
            if (dlgSave.ShowDialog(this) == DialogResult.OK)
                File.Move(file.Info.FullName, dlgSave.FileName);
            else file.Info.Delete();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            wskClient.SslProtocol = System.Security.Authentication.SslProtocols.Default;
            wskClient.RemoteHost = "www.precision.com";
            wskClient.RemotePort = 443;
            wskClient.LegacySupport = true;
            wskClient.CustomTextEncoding = Encoding.UTF8;
            wskClient.Connect("www.precision.com");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GET / HTTP/1.1");
            sb.AppendLine("Host: www.precision.com");
            sb.AppendLine("passive: true");
            sb.AppendLine("Accepts: text/html,application/xhtml+xml,application/xml;q=0.9");
            sb.AppendLine();
            wskClient.Send(sb.ToString());
        }
    }
}
