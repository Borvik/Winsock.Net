namespace WinsockDemo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.grpServer = new System.Windows.Forms.GroupBox();
            this.txtServerLog = new System.Windows.Forms.TextBox();
            this.cmdServerSend = new System.Windows.Forms.Button();
            this.txtServerSend = new System.Windows.Forms.TextBox();
            this.pbServerReceive = new System.Windows.Forms.ProgressBar();
            this.pbServerSend = new System.Windows.Forms.ProgressBar();
            this.chkLogServer = new System.Windows.Forms.CheckBox();
            this.cmdServerListen = new System.Windows.Forms.Button();
            this.grpClient = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cmdClientSendFile = new System.Windows.Forms.Button();
            this.txtClientLog = new System.Windows.Forms.TextBox();
            this.cmdClientSend = new System.Windows.Forms.Button();
            this.txtClientSend = new System.Windows.Forms.TextBox();
            this.pbClientReceive = new System.Windows.Forms.ProgressBar();
            this.pbClientSend = new System.Windows.Forms.ProgressBar();
            this.chkLogClient = new System.Windows.Forms.CheckBox();
            this.cmdClientConnect = new System.Windows.Forms.Button();
            this.wskServer = new Treorisoft.Net.Winsock();
            this.wskClient = new Treorisoft.Net.Winsock();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.grpServer.SuspendLayout();
            this.grpClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer1.Size = new System.Drawing.Size(747, 368);
            this.splitContainer1.SplitterDistance = 233;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgv, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(233, 368);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connected Clients";
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Location = new System.Drawing.Point(3, 16);
            this.dgv.Name = "dgv";
            this.dgv.ReadOnly = true;
            this.dgv.RowHeadersVisible = false;
            this.dgv.Size = new System.Drawing.Size(227, 349);
            this.dgv.TabIndex = 1;
            this.dgv.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.grpServer, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.grpClient, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(510, 368);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // grpServer
            // 
            this.grpServer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpServer.Controls.Add(this.txtServerLog);
            this.grpServer.Controls.Add(this.cmdServerSend);
            this.grpServer.Controls.Add(this.txtServerSend);
            this.grpServer.Controls.Add(this.pbServerReceive);
            this.grpServer.Controls.Add(this.pbServerSend);
            this.grpServer.Controls.Add(this.chkLogServer);
            this.grpServer.Controls.Add(this.cmdServerListen);
            this.grpServer.Location = new System.Drawing.Point(3, 3);
            this.grpServer.Name = "grpServer";
            this.grpServer.Size = new System.Drawing.Size(249, 362);
            this.grpServer.TabIndex = 0;
            this.grpServer.TabStop = false;
            this.grpServer.Text = "Server (Closed)";
            // 
            // txtServerLog
            // 
            this.txtServerLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerLog.Location = new System.Drawing.Point(6, 74);
            this.txtServerLog.Multiline = true;
            this.txtServerLog.Name = "txtServerLog";
            this.txtServerLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtServerLog.Size = new System.Drawing.Size(237, 260);
            this.txtServerLog.TabIndex = 6;
            // 
            // cmdServerSend
            // 
            this.cmdServerSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdServerSend.Enabled = false;
            this.cmdServerSend.Location = new System.Drawing.Point(168, 46);
            this.cmdServerSend.Name = "cmdServerSend";
            this.cmdServerSend.Size = new System.Drawing.Size(75, 23);
            this.cmdServerSend.TabIndex = 5;
            this.cmdServerSend.Text = "Send";
            this.cmdServerSend.UseVisualStyleBackColor = true;
            this.cmdServerSend.Click += new System.EventHandler(this.cmdServerSend_Click);
            // 
            // txtServerSend
            // 
            this.txtServerSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerSend.Enabled = false;
            this.txtServerSend.Location = new System.Drawing.Point(6, 48);
            this.txtServerSend.Name = "txtServerSend";
            this.txtServerSend.Size = new System.Drawing.Size(162, 20);
            this.txtServerSend.TabIndex = 4;
            this.txtServerSend.Enter += new System.EventHandler(this.txtServerSend_Enter);
            // 
            // pbServerReceive
            // 
            this.pbServerReceive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbServerReceive.Location = new System.Drawing.Point(0, 351);
            this.pbServerReceive.Name = "pbServerReceive";
            this.pbServerReceive.Size = new System.Drawing.Size(249, 11);
            this.pbServerReceive.TabIndex = 3;
            // 
            // pbServerSend
            // 
            this.pbServerSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbServerSend.Location = new System.Drawing.Point(0, 340);
            this.pbServerSend.Name = "pbServerSend";
            this.pbServerSend.Size = new System.Drawing.Size(249, 11);
            this.pbServerSend.TabIndex = 2;
            // 
            // chkLogServer
            // 
            this.chkLogServer.AutoSize = true;
            this.chkLogServer.Location = new System.Drawing.Point(87, 23);
            this.chkLogServer.Name = "chkLogServer";
            this.chkLogServer.Size = new System.Drawing.Size(58, 17);
            this.chkLogServer.TabIndex = 1;
            this.chkLogServer.Text = "Log All";
            this.chkLogServer.UseVisualStyleBackColor = true;
            // 
            // cmdServerListen
            // 
            this.cmdServerListen.Location = new System.Drawing.Point(6, 19);
            this.cmdServerListen.Name = "cmdServerListen";
            this.cmdServerListen.Size = new System.Drawing.Size(75, 23);
            this.cmdServerListen.TabIndex = 0;
            this.cmdServerListen.Text = "Listen";
            this.cmdServerListen.UseVisualStyleBackColor = true;
            this.cmdServerListen.Click += new System.EventHandler(this.cmdServerListen_Click);
            // 
            // grpClient
            // 
            this.grpClient.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpClient.Controls.Add(this.button2);
            this.grpClient.Controls.Add(this.button1);
            this.grpClient.Controls.Add(this.cmdClientSendFile);
            this.grpClient.Controls.Add(this.txtClientLog);
            this.grpClient.Controls.Add(this.cmdClientSend);
            this.grpClient.Controls.Add(this.txtClientSend);
            this.grpClient.Controls.Add(this.pbClientReceive);
            this.grpClient.Controls.Add(this.pbClientSend);
            this.grpClient.Controls.Add(this.chkLogClient);
            this.grpClient.Controls.Add(this.cmdClientConnect);
            this.grpClient.Location = new System.Drawing.Point(258, 3);
            this.grpClient.Name = "grpClient";
            this.grpClient.Size = new System.Drawing.Size(249, 362);
            this.grpClient.TabIndex = 1;
            this.grpClient.TabStop = false;
            this.grpClient.Text = "Client (Closed)";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(87, 311);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 311);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cmdClientSendFile
            // 
            this.cmdClientSendFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClientSendFile.Enabled = false;
            this.cmdClientSendFile.Location = new System.Drawing.Point(168, 19);
            this.cmdClientSendFile.Name = "cmdClientSendFile";
            this.cmdClientSendFile.Size = new System.Drawing.Size(75, 23);
            this.cmdClientSendFile.TabIndex = 8;
            this.cmdClientSendFile.Text = "Send File";
            this.cmdClientSendFile.UseVisualStyleBackColor = true;
            // 
            // txtClientLog
            // 
            this.txtClientLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtClientLog.Location = new System.Drawing.Point(6, 74);
            this.txtClientLog.Multiline = true;
            this.txtClientLog.Name = "txtClientLog";
            this.txtClientLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtClientLog.Size = new System.Drawing.Size(237, 260);
            this.txtClientLog.TabIndex = 7;
            // 
            // cmdClientSend
            // 
            this.cmdClientSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClientSend.Enabled = false;
            this.cmdClientSend.Location = new System.Drawing.Point(168, 46);
            this.cmdClientSend.Name = "cmdClientSend";
            this.cmdClientSend.Size = new System.Drawing.Size(75, 23);
            this.cmdClientSend.TabIndex = 6;
            this.cmdClientSend.Text = "Send";
            this.cmdClientSend.UseVisualStyleBackColor = true;
            this.cmdClientSend.Click += new System.EventHandler(this.cmdClientSend_Click);
            // 
            // txtClientSend
            // 
            this.txtClientSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtClientSend.Enabled = false;
            this.txtClientSend.Location = new System.Drawing.Point(6, 48);
            this.txtClientSend.Name = "txtClientSend";
            this.txtClientSend.Size = new System.Drawing.Size(162, 20);
            this.txtClientSend.TabIndex = 5;
            this.txtClientSend.Enter += new System.EventHandler(this.txtClientSend_Enter);
            // 
            // pbClientReceive
            // 
            this.pbClientReceive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbClientReceive.Location = new System.Drawing.Point(0, 351);
            this.pbClientReceive.Name = "pbClientReceive";
            this.pbClientReceive.Size = new System.Drawing.Size(249, 11);
            this.pbClientReceive.TabIndex = 4;
            // 
            // pbClientSend
            // 
            this.pbClientSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbClientSend.Location = new System.Drawing.Point(0, 340);
            this.pbClientSend.Name = "pbClientSend";
            this.pbClientSend.Size = new System.Drawing.Size(249, 11);
            this.pbClientSend.TabIndex = 3;
            // 
            // chkLogClient
            // 
            this.chkLogClient.AutoSize = true;
            this.chkLogClient.Location = new System.Drawing.Point(87, 23);
            this.chkLogClient.Name = "chkLogClient";
            this.chkLogClient.Size = new System.Drawing.Size(58, 17);
            this.chkLogClient.TabIndex = 1;
            this.chkLogClient.Text = "Log All";
            this.chkLogClient.UseVisualStyleBackColor = true;
            // 
            // cmdClientConnect
            // 
            this.cmdClientConnect.Location = new System.Drawing.Point(6, 19);
            this.cmdClientConnect.Name = "cmdClientConnect";
            this.cmdClientConnect.Size = new System.Drawing.Size(75, 23);
            this.cmdClientConnect.TabIndex = 0;
            this.cmdClientConnect.Text = "Connect";
            this.cmdClientConnect.UseVisualStyleBackColor = true;
            this.cmdClientConnect.Click += new System.EventHandler(this.cmdClientConnect_Click);
            // 
            // wskServer
            // 
            this.wskServer.CustomTextEncoding = ((System.Text.Encoding)(resources.GetObject("wskServer.CustomTextEncoding")));
            this.wskServer.ClientCountChanged += new System.EventHandler(this.wskServer_ClientCountChanged);
            this.wskServer.ConnectionRequest += new System.EventHandler<Treorisoft.Net.ConnectionRequestEventArgs>(this.wskServer_ConnectionRequest);
            this.wskServer.DataArrival += new System.EventHandler<Treorisoft.Net.DataArrivalEventArgs>(this.wskServer_DataArrival);
            this.wskServer.ErrorReceived += new System.EventHandler<Treorisoft.Net.ErrorReceivedEventArgs>(this.wskServer_ErrorReceived);
            this.wskServer.ReceiveProgress += new System.EventHandler<Treorisoft.Net.ReceiveProgressEventArgs>(this.wskServer_ReceiveProgress);
            this.wskServer.SendProgress += new System.EventHandler<Treorisoft.Net.SendProgressEventArgs>(this.wskServer_SendProgress);
            this.wskServer.StateChanged += new System.EventHandler<Treorisoft.Net.StateChangedEventArgs>(this.wskServer_StateChanged);
            // 
            // wskClient
            // 
            this.wskClient.CustomTextEncoding = ((System.Text.Encoding)(resources.GetObject("wskClient.CustomTextEncoding")));
            this.wskClient.DataArrival += new System.EventHandler<Treorisoft.Net.DataArrivalEventArgs>(this.wskClient_DataArrival);
            this.wskClient.ErrorReceived += new System.EventHandler<Treorisoft.Net.ErrorReceivedEventArgs>(this.wskClient_ErrorReceived);
            this.wskClient.ReceiveProgress += new System.EventHandler<Treorisoft.Net.ReceiveProgressEventArgs>(this.wskClient_ReceiveProgress);
            this.wskClient.SendProgress += new System.EventHandler<Treorisoft.Net.SendProgressEventArgs>(this.wskClient_SendProgress);
            this.wskClient.StateChanged += new System.EventHandler<Treorisoft.Net.StateChangedEventArgs>(this.wskClient_StateChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(747, 368);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Winsock Demo";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.grpServer.ResumeLayout(false);
            this.grpServer.PerformLayout();
            this.grpClient.ResumeLayout(false);
            this.grpClient.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Treorisoft.Net.Winsock wskServer;
        private Treorisoft.Net.Winsock wskClient;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox grpServer;
        private System.Windows.Forms.Button cmdServerListen;
        private System.Windows.Forms.GroupBox grpClient;
        private System.Windows.Forms.Button cmdClientConnect;
        private System.Windows.Forms.CheckBox chkLogServer;
        private System.Windows.Forms.CheckBox chkLogClient;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private System.Windows.Forms.SaveFileDialog dlgSave;
        private System.Windows.Forms.ProgressBar pbServerReceive;
        private System.Windows.Forms.ProgressBar pbServerSend;
        private System.Windows.Forms.ProgressBar pbClientReceive;
        private System.Windows.Forms.ProgressBar pbClientSend;
        private System.Windows.Forms.TextBox txtServerLog;
        private System.Windows.Forms.Button cmdServerSend;
        private System.Windows.Forms.TextBox txtServerSend;
        private System.Windows.Forms.TextBox txtClientLog;
        private System.Windows.Forms.Button cmdClientSend;
        private System.Windows.Forms.TextBox txtClientSend;
        private System.Windows.Forms.Button cmdClientSendFile;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}

