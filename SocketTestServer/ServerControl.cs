using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace SocketTestServer
{
    class ServerControl
    {
        /// <summary>
        /// 用户对象
        /// </summary>
        public class User
        {
           public Socket socket;
            public String UserName;
            
        }
        public Socket serverSocket;
        //public List<Socket> socketList;
        public List<User> UserList;
        public ServerControl()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socketList = new List<Socket>();
            UserList = new List<User>();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Enabled = true;
            t.Interval = 2000;
            t.Tick += new System.EventHandler(TimerSendNum);

        }
        private void TimerSendNum(object sender, EventArgs e)
        {
            SendConMsg();
        }
        //服务器启动
        public int Count()
        {
            return UserList.Count();
        }
        public void SendConMsg()
        {
            Broadcast(null, "|c|连接人数：" + Count().ToString());
        }
        /// <summary>
        /// 启动服务端
        /// </summary>
        public void Start()
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 9335));
            serverSocket.Listen(10);
            A.ric.AppendText(DateTime.Now.ToString() + "服务器启动成功\n");
            Thread thread = new Thread(Accept);
            thread.IsBackground = true;
            thread.Start();
            
        }
        /// <summary>
        /// 当有用户连接
        /// </summary>
        private void Accept()
        {
            Socket client = serverSocket.Accept();
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
            A.ric.AppendText(DateTime.Now.ToString() + point.Address + "【" + point.Port + "】连接成功\n");
            User U = new User();
            U.socket = client;
            U.UserName = "A";
            UserList.Add(U);
            //socketList.Add(client);
            Thread threadReveive = new Thread(Receive);
            threadReveive.IsBackground = true;
            threadReveive.Start(client);
            Accept();
        }
        /// <summary>
        /// 收到用户消息
        /// </summary>
        /// <param name="obj"></param>
        private void Receive(object obj)
        {
            Socket client = obj as Socket;
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
            try
            {
                byte[] msg = new byte[1024];
                int msglen = client.Receive(msg);
                string str = Encoding.UTF8.GetString(msg, 0, msglen);
                string ss = "";
                if (str.Length < 3)
                {
                     ss = "";
                }
                else
                {
                     ss = str.Substring(0, 3);
                    //A.ric.AppendText("\n||" + ss);
                }
                if (ss == "||a")
                {
                    //心跳数据包 不需要广播
                    str = str.Substring(3, str.Length);
                }
                else if (ss == "||b")
                {
                    str = str.Substring(3, str.Length);
                    string msgStr = DateTime.Now.ToString() + point.Address + "【" + point.Port + "】" + Encoding.UTF8.GetString(msg, 0, msglen);
                    A.ric.AppendText(msgStr + "\n");
                    Broadcast(client, msgStr);
                }
                else
                {
                    string msgStr = DateTime.Now.ToString() + point.Address + "【" + point.Port + "】" + Encoding.UTF8.GetString(msg, 0, msglen);
                    A.ric.AppendText(msgStr + "\n");
                    Broadcast(client, msgStr);
                }
                Receive(client);

            }
            catch
            {
                A.ric.AppendText(DateTime.Now.ToString() + point.Address + "【" + point.Port + "】断开连接\n");
                
                foreach (var user in UserList)
                {
                    if (user.socket == client)
                    {
                        UserList.Remove(user);
                    }
                }
            }
        }
        ///服务器广播消息
       
        public void Broadcast(Socket clientOhter, string msg)
        {
            //string ss = msg.Substring(0, 3);
            
            {
                //other为消息发送者 
                foreach (var client in UserList)
                {
                    if (client.socket == clientOhter)
                    {
                        client.socket.Send(Encoding.UTF8.GetBytes(msg));
                    }
                    else
                    {
                        client.socket.Send(Encoding.UTF8.GetBytes(msg));
                    }
                }
            }
        }

    }
}
