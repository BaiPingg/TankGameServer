using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 服务器001.Core;

namespace 服务器001.Logic
{
    public partial class HandlePlayerlMsg
    {
        //开始战斗
        /*   public void MsgStartFight(Player player, ProtocolBase protoBase) {
               ProtocolBytes protocol = new ProtocolBytes();
               protocol.AddString("StartFight");
               //条件判断
               if (player.tempData.status != PlayerTempData.Status.Room) {
                   Console.WriteLine("MsgStartFight status err" + player.id);
                   protocol.AddInt(-1);
                   player.Send(protocol);
                   return;
               }
               if (!player.tempData.isOwner) {
                   Console.WriteLine("MsgStartFight owner err" + player.id);
                   protocol.AddInt(-1);
                   player.Send(protocol);
                   return;
               }
               Room room = player.tempData.room;
               if (!room.CanStart()){
                   Console.WriteLine("MsgStartFight Canstart err" + player.id);
                   protocol.AddInt(-1);
                   player.Send(protocol);
                   return;
               }
               //开始战斗
               protocol.AddInt(0);
               player.Send(protocol);
               room.StartFight();
           }*/
    }
}
