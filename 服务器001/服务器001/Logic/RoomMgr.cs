using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 服务器001.Core;

namespace 服务器001.Logic
{
    public class RoomMgr
    {
        //单例
        public static RoomMgr instance;
        public RoomMgr()
        {
            instance = this;
        }
        //房价列表
        public List<Room> list = new List<Room>();

        //创建房间
        public void CreateRoom(Player player)
        {
            Room room = new Room();
            lock (list)
            {
                list.Add(room);
                room.AddPlayer(player);
            }
        }

        //玩家离开
        public void LeaveRoom(Player player)
        {
            PlayerTempData tempData = player.tempData;
            if (tempData.status == PlayerTempData.Status.None)
            {
                return;
            }
            Room room = tempData.room;
            lock (list)
            {
                room.DelPlayer(player.id);
                if (list.Count == 0)
                {
                    list.Remove(room);
                }
            }
        }

        //协议
        public ProtocolBytes GetRoomList()
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetRoomList");
            int count = list.Count;
            //房间数量
            protocol.AddInt(count);
            //每个房间信息
            for (int i = 0; i < count; i++)
            {
                Room room = list[i];
                protocol.AddInt(room.list.Count);
                protocol.AddInt((int)room.status);
            }
            return protocol;
        }
    }
}
