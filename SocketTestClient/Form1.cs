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
using System.Threading;
namespace SocketTestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        public ClientControl client2= new ClientControl();
        private void Form1_Load(object sender, EventArgs e)
        {

            A.ric = richTextBox1;
            A.LB = label2;
            client2.Connect("49.235.96.127", 9335);

            label1.Text = "连接成功";
            client2.Send("连接完成");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client2.Send(textBox1.Text);
            textBox1.Text = "";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                client2.Send("||a");
            }
            catch
            {
                label1.Text = "连接异常";
            }
            }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Convert.ToInt32(e.KeyChar) == Convert.ToInt32(Keys.Enter))
            {
                client2.Send(textBox1.Text);
                textBox1.Text = "";
            }
        }
    }
    public class A
    {
        static public RichTextBox ric;
        static public Label LB;
    }
    public class ClientControl
    {
        private Socket clientSocket;
        public ClientControl()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect(string ip, int port)
        {
            clientSocket.Connect(ip, port);
            Thread threadreceive = new Thread(Receiver);
            threadreceive.IsBackground = true;
            threadreceive.Start();
            //连接服务器完成
        }
        private void Receiver()
        {
            byte[] msg = new byte[1024];
            int msglen = clientSocket.Receive(msg);
            if (Encoding.UTF8.GetString(msg, 0, msglen).Substring(0, "|c|连接人数：".Length) == "|c|连接人数：")
            {
                string mesg = Encoding.UTF8.GetString(msg, 0, msglen);
                A.LB.Text = mesg.Trim().Substring(3);
            }
            else
            {
                A.ric.AppendText(Encoding.UTF8.GetString(msg, 0, msglen) + "\n");
            }
            Receiver();
        }
        public void Send(string msg)
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(msg));
            
        }
    }
}
