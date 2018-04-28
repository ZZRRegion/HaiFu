using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using HaiFu.Protocol;

namespace HaiFuClient
{
    public partial class FrmMain : FrmMainBase
    {
        private HaiFu.Protocol.HaiFuClient client;
        public FrmMain()
        {
            InitializeComponent();
            DevCommon.Main = this;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (this.client != null)
            {
                this.client.MsgEventHandle -= this.Client_MsgEventHandle;
                this.client.FileEventHandle -= this.Client_FileEventHandle;
                this.client.Dispose();
                this.btnConnect.Text = "连接";
                this.client = null;
            }
            else
            {
                try
                {
                    TcpClient client = new TcpClient();
                    int port = (int)this.nudPort.Value;
                    IPEndPoint end = new IPEndPoint(IPAddress.Parse(this.txtIP.Text), port);
                    client.Connect(end);
                    if (client.Connected)
                    {
                        this.btnConnect.Text = "断开";
                        this.client = new HaiFu.Protocol.HaiFuClient(client, Guid.NewGuid().ToString("N"));
                        this.client.SavePath = this.txtPath.Text;
                        this.client.SendName(this.txtName.Text);
                        this.client.MsgEventHandle += Client_MsgEventHandle;
                        this.client.FileEventHandle += Client_FileEventHandle;
                    }
                }
                catch(Exception ex)
                {
                    this.MsgBox(ex.Message);
                }
            }
        }

        private void Client_FileEventHandle(HaiFu.Protocol.HaiFuClient arg1, string arg2, int progress)
        {
            Action action = () => {
                string msg = $"[{arg2}], {progress}{Environment.NewLine}";
                this.AppendText(msg);
            };
            this.Invoke(action);
        }

        private void Client_MsgEventHandle(HaiFu.Protocol.HaiFuClient arg1, string arg2)
        {
            Action action = () => {
                string msg = $"{DateTime.Now}:{arg2}{Environment.NewLine}";
                this.AppendText(msg);
            };
            this.Invoke(action);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            this.client.SendMsg(this.txtContent.Text);
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog(this) == DialogResult.OK)
            {
                this.AppendText("开始发送文件");
                this.client.SendFile(ofd.FileName);
                this.AppendText("发送文件成功");
            }
        }
        private void AppendText(string msg)
        {
            this.rtbContent.AppendText(msg);
            this.rtbContent.SelectionStart = this.rtbContent.Text.Length;
            this.rtbContent.ScrollToCaret();
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.txtName.Text = StConfig.Name;
            this.txtPath.Text = StConfig.SavePath;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            StConfig.Name = this.txtName.Text;
            StConfig.SavePath = this.txtPath.Text;
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = this.txtPath.Text;
            if(fbd.ShowDialog(this) == DialogResult.OK)
            {
                this.txtPath.Text = fbd.SelectedPath;
                this.client.SavePath = this.txtPath.Text;
            }
        }
    }
}
