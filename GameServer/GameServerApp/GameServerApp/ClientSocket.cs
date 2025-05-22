
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerApp
{
    /// <summary>
    /// 客户端连接对象 负责和客户端进行通讯
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// 所属角色
        /// </summary>
        private Role m_Role;
        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket m_socket;
        /// <summary>
        /// 接收数据的线程
        /// </summary>
        private Thread m_reveiveThread;

        #region 接收消息所需变量
        /// <summary>
        /// 接收数据包的数组字节数组缓冲区
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[10240];
        /// <summary>
        /// 接收数据包的缓冲数据流
        /// </summary>
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
        #endregion


        #region 发送消息所需变量
        /// <summary>
        /// 发送消息队列
        /// </summary>
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();
        /// <summary>
        /// 检查队列的委托
        /// </summary>
        private Action m_CheckSendueue;

        #endregion

        public ClientSocket(Socket socket,Role role)
        {
            m_socket = socket;
            m_Role = role;
            m_Role.clientSocket = this;
            //启动线程 进行接收数据
            m_reveiveThread = new Thread(ReceiveMsg);
            m_reveiveThread.Start();
            m_CheckSendueue = OnCheckSendQueueCallBack;
            //temp
            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                ms.WriteUTF8String(string.Format("欢迎登录服务器" + DateTime.Now.ToString()));
                this.SendMsg(ms.ToArray());
            }
        }

        #region ReceiveMgr 接收数据
        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMsg()
        {
            //异步接收数据
            m_socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_socket);
        }

        #endregion

        #region ReceiveCallBack 接收数据回调
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                int len = m_socket.EndReceive(ar);
                if (len > 0)
                {
                    //已经接收到数据

                    //把接收到的数据 写入缓冲数据流的尾部
                    m_ReceiveMS.Position = m_ReceiveMS.Length;
                    //把指定长度的字节 写入数据流
                    m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);
                    //如果缓存数据流的长度>2，说明至少有个不完整的包过来了
                    //为什么是2？ 以为客户端封装数据包 用的是ushort， 长度就是2
                    if (m_ReceiveMS.Length > 2)
                    {
                        //进行循环，拆分数据包
                        while (true)
                        {
                            //把数据流指针位置放在0处
                            m_ReceiveMS.Position = 0;
                            //currMsgLen = 包体的长度
                            int currMsgLen = m_ReceiveMS.ReadUShort();
                            //currFullMsgLen 总包的长度=包头长度+包体长度
                            int currFullMsgLen = 2 + currMsgLen;
                            //如果数据流的长度>=整包的长度 说明至少收到了一个完整包
                            if (m_ReceiveMS.Length >= currFullMsgLen)
                            {
                                //至少收到一个完整包
                                //定义包体的byte[]数组
                                byte[] buffer = new byte[currMsgLen];
                                //把数据流指针放到2的位置， 也就是包体的位置
                                m_ReceiveMS.Position = 2;
                                //把包体读到byte[]数组
                                m_ReceiveMS.Read(buffer, 0, currMsgLen);
                                ushort protoCode = 0;
                                byte[] protoContent = new byte[buffer.Length - 2];
                                //临时处理
                                //buffer 这个byte[]数组就是包体 也就是我们要的数据
                                using (MMO_MemoryStream ms2 = new MMO_MemoryStream(buffer))
                                {
                                    //string msg = ms2.ReadUTF8String();
                                    //Console.WriteLine(msg);

                                    protoCode = ms2.ReadUShort();
                                    ms2.Read(protoContent, 0, protoContent.Length);
                                }
                                Console.WriteLine("protoCode---" + protoCode);

                                if (protoCode == ProtoCodeDef.test)
                                {
                                    testProto test = testProto.GetProto(protoContent);
                                    Console.WriteLine("protoId---" + test.Id);
                                    Console.WriteLine("protoName---" + test.Name);
                                    Console.WriteLine("protoAge---" + test.Age);


                                    MailProto mailProto = new MailProto();
                                    mailProto.IsSuccess = false;
                                    mailProto.ErrorCode = -9999;
                                    mailProto.content = "我是邮件";
                                    this.SendMsg(mailProto.ToArray());
                                }

                                using (MMO_MemoryStream ms=new MMO_MemoryStream())
                                {
                                    //ms.WriteUTF8String(string.Format("服务器时间" + DateTime.Now.ToString()));
                                    //this.SendMsg(ms.ToArray());
                                }
                                //========================处理剩余字节数组======================================

                                //剩余字节长度
                                int remainLen = (int)m_ReceiveMS.Length - currFullMsgLen;
                                if (remainLen > 0)
                                {
                                    //把指针放在第一个包的尾部
                                    m_ReceiveMS.Position = currFullMsgLen;
                                    //定义剩余字节数组
                                    byte[] remainBuffer = new byte[remainLen];
                                    //把数据流读取到剩余字节数组
                                    m_ReceiveMS.Read(remainBuffer, 0, remainLen);
                                    //清空数据流
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);
                                    //把剩余字节数组重新写入数据流
                                    m_ReceiveMS.Write(remainBuffer, 0, remainBuffer.Length);
                                    remainBuffer = null;
                                }
                                else
                                {
                                    //没有剩余字节
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);
                                }
                            }
                            else
                            {
                                //还没有收到完整包
                                break;
                            }
                        }
                    }
                    //进行下一次接收数据包
                    ReceiveMsg();
                }
                else
                {
                    //客户端断开连接
                    Console.WriteLine("客户端{0}断开连接", m_socket.RemoteEndPoint.ToString());
                    RoleMgr.Instance.AllRole.Remove(m_Role);
                }
            }
            catch
            {
                Console.WriteLine("客户端{0}断开连接", m_socket.RemoteEndPoint.ToString());
                RoleMgr.Instance.AllRole.Remove(m_Role);
            }

        }
        #endregion

        //===========================================================
        #region OnCheckSendQueueCallBack 检查队列的委托回调
        /// <summary>
        /// 检查队列的委托回调
        /// </summary>
        private void OnCheckSendQueueCallBack()
        {
            lock (m_SendQueue)
            {
                //如果队列中有数据包 则发送数据包
                if (m_SendQueue.Count > 0)
                {
                    //发送数据包
                    Send(m_SendQueue.Dequeue());
                }
            }
        }
        #endregion

        #region 封装数据包
        /// <summary>
        /// 封装数据包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] MakeBuffer(byte[] data)
        {
            byte[] retBuffer = null;
            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                ms.WriteUShort((ushort)data.Length);
                ms.Write(data, 0, data.Length);
                retBuffer = ms.ToArray();

            }
            return retBuffer;
        }
        #endregion

        #region SendMsg 发送消息 把消息加入到队列
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="buffer"></param>
        public void SendMsg(byte[] buffer)
        {
            //得到封装后的数据包
            byte[] sendBuffer = MakeBuffer(buffer);
            lock (m_SendQueue)
            {
                ///把数据包加入队列
                m_SendQueue.Enqueue(sendBuffer);
                ///启动委托 (执行委托)
                m_CheckSendueue.BeginInvoke(null, null);
            }
        }
        #endregion

        #region Send 真正发送数据包到服务器
        /// <summary>
        /// 真正发送数据包到服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void Send(byte[] buffer)
        {
            m_socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_socket);
        }
        #endregion

        #region SendCallBack 发送数据包的回调
        /// <summary>
        /// 发送数据包的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            m_socket.EndSend(ar);
            //继续检查队列
            OnCheckSendQueueCallBack();
        }
        #endregion

    }
}
