using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Threading;
using System.Data;
using 服务器001.Logic;

namespace 服务器001.Core
{
    public class ServNet
    {
        //消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();
        //监听套接字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;
        //单例
        public static ServNet instance;
        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        /*由应用程序自己发送心跳包来检测连接是否正常，大致的方法是：服务器在一个 Timer事件中定时 向客户端发送一个短精悍的数据包，然后启动一个低级别的线程，在该线程中不断检测客户端的回应， 如果在一定时间内没有收到客户端的回应，即认为客户端已经掉线；同样，如果客户端在一定时间内没 有收到服务器的心跳包，则认为连接不可用。
         */
        //心跳时间
        public long heartBeatTime = 180;
        //协议
        public ProtocolBase proto;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ServNet()
        {
            instance = this;
        }
        /// <summary>
        ///  获取连接池索引，返回负数表示连接失败
        /// </summary>
        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="host">ip</param>
        /// <param name="port">端口</param>
        public void Start(string host, int port)
        {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;
            //连接池
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            //socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //bind
            IPAddress ipAdr = IPAddress.Parse(host);

            IPEndPoint ipEp = new IPEndPoint(IPAddress.Any, port);
            listenfd.Bind(ipEp);

            //listen
            listenfd.Listen(maxConn);
            //accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器] 启动成功");
        }

        /// <summary>
        /// Accept回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("客户端连接【" + adr + "】conn池 ID：" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Accept失败：" + e.Message);
            }
        }

        
        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            lock (conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0)
                    {
                        Console.WriteLine("收到[" + conn.GetAdress() + "]断开连接");
                        conn.Close();
                        return;
                    }
                    conn.buffCount += count;
                    ProcessData(conn);
                    //继续接收
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                catch (Exception e)
                {
                    Console.WriteLine("收到kle【" + conn.GetAdress() + "】 断开连接" + e.Message);
                    conn.Close();
                }
            }

        }

        private void ProcessData(Conn conn)
        {
            //小于字节长度
            if (conn.buffCount < sizeof(Int32))
            {
                return;
            }
            //消息长度
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
            if (conn.buffCount < conn.msgLength + sizeof(Int32))
            {
                return;
            }
            //消息处理
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
            HandleMsg(conn, protocol);
            //清除已经处理的消息
            int count = conn.buffCount - conn.msgLength - sizeof(Int32);
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0)
            {
                ProcessData(conn);
            }

        }
        //发送
        public void Send(Conn conn, ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            try
            {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[发送消息]" + conn.GetAdress() + ":" + e.Message);
            }
        }

        //广播
        public void Broadcast(ProtocolBase protocol)
        {
            for (int i = 0; i < conns.Length; i++)
            {
                if (!conns[i].isUse)
                    continue;
                if (conns[i].player == null)
                    continue;
                Send(conns[i], protocol);
            }
        }
        //主定时器
        public void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            //处理xintiao
            HeartBeat();
            timer.Start();
        }
        //心跳
        public void HeartBeat()
        {
            //Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;
                if (conn.lastTickTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAdress());
                    lock (conn)
                        conn.Close();
                }
            }
        }

        private void HandleMsg(Conn conn, ProtocolBase protoBase)
        {
            string name = protoBase.GetName();
            string methodName = "Msg" + name;
            //连接协议分发
            if (conn.player == null || name == "HeatBeat" || name == "Logout")
            {
                MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[警告]HandleMsg没有处理连接方法";
                    Console.WriteLine(str + methodName);
                    return;
                }
                Object[] obj = new Object[] { conn, protoBase };
                Console.WriteLine("[处理连接消息]" + conn.GetAdress() + ":" + name);
                mm.Invoke(handleConnMsg, obj);
            }
            //角色协议分发
            else
            {
                MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[警告]HandleMsg没有处理玩家方法";
                    Console.WriteLine(str + methodName);
                    return;
                }
                Object[] obj = new Object[] { conn.player, protoBase };
                Console.WriteLine("[处理玩家消息]" + conn.player.id + ":" + name);
                mm.Invoke(handlePlayerMsg, obj);
            }

        }
        #region 后台使用的打印和关闭的方法

        /// <summary>
        /// 打印连接信息
        /// </summary>
        public void Print()
        {
            Console.WriteLine("===服务器登陆信息===");
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                    continue;
                if (!conns[i].isUse)
                    continue;
                string str = "连接[" + conns[i].GetAdress() + "]";
                if (conns[i].player != null)
                    str += "玩家id" + conns[i].player.id;
                Console.WriteLine(str);
            }

        }

        /// <summary>
        /// 后台使用的关闭服务器的方法
        /// </summary>
        public void Close()
        {
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null)
                    continue;
                if (!conn.isUse)
                    continue;
                lock (conn)
                {
                    conn.Close();
                }
            }
        }
        #endregion

    }
}
