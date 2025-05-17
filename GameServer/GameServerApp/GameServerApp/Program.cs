using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerApp
{
    /// <summary>
    /// 应用程序入口点
    /// </summary>
    class Program
    {
        /// <summary>
        /// 本机的IP 
        /// </summary>
        private static string m_ServerIP = "192.168.0.197";
        /// <summary>
        /// 本机的端口
        /// </summary>
        private static int m_Point=1011;
        private static Socket m_ServerSocket;

        static void Main(string[] args)
        {
            //实例化socket
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //向操作系统申请一个可用的ip和端口用来通讯
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Point));
            //设置最多3000个排队连接请求
            m_ServerSocket.Listen(3000);
            Console.WriteLine("启动监听{0}成功", m_ServerSocket.LocalEndPoint.ToString());
            Thread mthread = new Thread(ListenClientCallBack);
            mthread.Start();
            Console.ReadLine();
        }
        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private static void ListenClientCallBack()
        {
            while (true)
            {
                //接收客户端请求
                Socket socket = m_ServerSocket.Accept();
                Console.WriteLine("客户端{0}已经连接", socket.RemoteEndPoint.ToString());
                //一个角色就相当于一个客户端
                Role role = new Role();
                ClientSocket clientSocket = new ClientSocket(socket, role);
                ///将角色添加到角色管理
                RoleMgr.Instance.AllRole.Add(role);
            }
        }
    }
}
