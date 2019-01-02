using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 服务器001.Core;

namespace 服务器001.Logic
{
    public class HandlePlayerEvent
    {
        //上线
        public void OnLogin(Player player)
        {
            // Scene.instance.AddPlayer(player.id);
            //  Console.WriteLine("玩家：" + player.id + "上线");
        }

        //下线
        public void OnLogout(Player player)
        {
            if (player.tempData.status == PlayerTempData.Status.Room)
            {
                Room room = player.tempData.room;
                RoomMgr.instance.LeaveRoom(player);
                if (room != null)
                    room.Broadcast(room.GetRoomInfo());
            }
            if (player.tempData.status == PlayerTempData.Status.Fight)
            {
                Room room = player.tempData.room;
                room.ExitFight(player);
                RoomMgr.instance.LeaveRoom(player);
            }
        }
    }
}
