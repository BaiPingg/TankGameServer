using System;
using 服务器001.Core;
using 服务器001.Logic;

namespace Serv
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Scene scene = new Scene();
            DataMgr dataMgr = new DataMgr();
            ServNet servNet = new ServNet();
            servNet.proto = new ProtocolBytes();
            servNet.Start("119.4.253.209", 10003);
            RoomMgr roomMgr = new RoomMgr();

            //服务端控制跳出和当前连接玩家信息
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        servNet.Close();
                        return;
                    case "print":
                        servNet.Print();
                        break;
                }
            }

        }
    }
}