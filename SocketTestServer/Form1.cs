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

namespace SocketTestServer
{
    public partial class Form1 : Form
    {
        //public Socket serverSocket;
        //public List<Socket> socketList;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            A.ric = richTextBox1;
            ServerControl SC = new ServerControl();
            SC.Start();
           
        }
        

    }
    public class A
    {
        static public RichTextBox ric;
    }
}
