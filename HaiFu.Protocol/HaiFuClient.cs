using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace HaiFu.Protocol
{
    public class HaiFuClient:IDisposable
    {
        public string Name { get; set; }
        public string SavePath { get; set; }
        private TcpClient client;
        private bool stop;
        private Thread thread;
        public event Action<HaiFuClient, string> MsgEventHandle;
        public event Action<HaiFuClient, string, int> FileEventHandle;
        public event Action<HaiFuClient, bool> ClientStateEventHandle;
        public string Guid { get; private set; }
        public HaiFuClient(TcpClient client, string guid)
        {
            this.client = client;
            this.Guid = guid;
            this.thread = new Thread(this.Run);
            this.thread.Name = guid;
            this.thread.IsBackground = true;
            this.thread.Start();
        }
        private void Run(object obj)
        {
            while (!this.stop && this.client.Connected)
            {
                byte[] bys = new byte[1024 * 10];
                try
                {
                    int length = this.client.GetStream().Read(bys, 0, bys.Length);
                    if (length > 0)
                    {
                        string config = Encoding.UTF8.GetString(bys, 0, length);
                        if (!string.IsNullOrEmpty(config))
                        {
                            JObject json = JObject.Parse(config);
                            string type = json.Value<string>("Type");
                            switch (type)
                            {
                                case "Name":
                                    this.Name = json.Value<string>("Content");
                                    this.ClientStateEventHandle?.Invoke(this, true);
                                    break;
                                case "Msg":
                                    string content = json.Value<string>("Content");
                                    this.MsgEventHandle?.Invoke(this, content);
                                    break;
                                case "File":
                                    string fileName = json.Value<string>("FileName");
                                    string name = Path.GetFileName(fileName);
                                    name = Path.Combine(this.SavePath, name);
                                    long len = json.Value<long>("FileSize");
                                    Stopwatch stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    this.FileEventHandle?.Invoke(this, $"开始接收文件[{name},大小[{len}]", 0);
                                    this.ReadFile(name, len);
                                    stopwatch.Stop();
                                    this.FileEventHandle?.Invoke(this, $"接收成功！用时:{stopwatch.Elapsed.ToString()}", 100);
                                    break;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    this.ClientStateEventHandle?.Invoke(this, false);
                }
            }
        }
        private void ReadFile(string fileName, long length)
        {
            FileStream fs = File.Create(fileName);
            long len = 0;
            while(len < length)
            {
                try
                {
                    byte[] bys = new byte[1024 * 1000];
                    int ll = this.client.GetStream().Read(bys, 0, bys.Length);
                    fs.Write(bys, 0, ll);
                    len += ll;
                    int progress = (int)(len * 100 / length);
                    this.FileEventHandle?.Invoke(this, fileName, progress);
                    Application.DoEvents();
                }
                catch(Exception ex)
                {
                    len = length;
                    this.ClientStateEventHandle?.Invoke(this, false);
                }
            }
            fs.Close();
        }
        public void SendFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                JObject json = new JObject();
                json["Type"] = "File";
                json["FileName"] = fileName;
                FileStream fs = File.OpenRead(fileName);
                json["FileSize"] = fs.Length;
                string config = json.ToString();
                byte[] bys = Encoding.UTF8.GetBytes(config);
                this.client.GetStream().Write(bys, 0, bys.Length);
                Thread.Sleep(10);
                long len = 0;
                long total = fs.Length;
                while(len < total)
                {
                    try
                    {
                        byte[] bbs = new byte[1024 * 1000];
                        int ll = fs.Read(bbs, 0, bbs.Length);
                        this.client.GetStream().Write(bbs, 0, ll);
                        len += ll;
                        int progress = (int)(len * 100 / total);
                        this.FileEventHandle?.Invoke(this, fileName, progress);
                        Application.DoEvents();
                    }
                    catch(Exception ex)
                    {
                        len = total;
                        this.ClientStateEventHandle?.Invoke(this, false);
                    }
                }
                fs.Close();
            }
        }
        public void SendMsg(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                JObject json = new JObject();
                json["Type"] = "Msg";
                json["Content"] = msg;
                string config = json.ToString();
                byte[] bys = Encoding.UTF8.GetBytes(config);
                try
                {
                    this.client.GetStream().Write(bys, 0, bys.Length);
                }
                catch(Exception ex)
                {
                    this.ClientStateEventHandle?.Invoke(this, false);
                }
            }
        }
        public void SendName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                JObject json = new JObject();
                json["Type"] = "Name";
                json["Content"] = name;
                json["Flag"] = System.Guid.NewGuid().ToString();
                string config = json.ToString();
                byte[] bys = Encoding.UTF8.GetBytes(config);
                try
                {
                    this.client.GetStream().Write(bys, 0, bys.Length);
                }
                catch (Exception ex)
                {
                    this.ClientStateEventHandle?.Invoke(this, false);
                }
            }
        }
        public void Dispose()
        {
            this.ClientStateEventHandle?.Invoke(this, false);
            this.stop = true;
            this.client?.Client.Close();
            this.client?.Close();
            this.client?.Dispose();
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return this.client?.Client?.RemoteEndPoint.ToString();
        }
    }
}
