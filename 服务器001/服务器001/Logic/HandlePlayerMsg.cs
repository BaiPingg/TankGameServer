using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 服务器001.Core;

namespace 服务器001.Logic
{
    public partial class HandlePlayerMsg
    {
        //获取分数
        //协议参数
        //返回协议：int分数
        public void MsgGetScore(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetScore");
            protocolRet.AddInt(player.data.score);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetScore" + player.id + player.data.score);
        }

        //增加分数
        //协议参数：
        public void MsgAddScore(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            //处理
            player.data.score += 1;
            Console.WriteLine("MsgAddScore" + player.id + " " + player.data.score.ToString());
        }

        //获取玩家列表
        public void MsgGetList(Player player, ProtocolBase protoBase)
        {
            Scene.instance.SendPlayerList(player);
        }
        //跟新信息
        public void MsgUpdateInfo(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            //Console.WriteLine(player.id);
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            float x = protocol.GetFloat(start, ref start);
            float y = protocol.GetFloat(start, ref start);
            float z = protocol.GetFloat(start, ref start);
            int score = player.data.score;
            Scene.instance.UpdateInfo(player.id, x, y, z, score);
            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(x);
            protocolRet.AddFloat(y);
            protocolRet.AddFloat(z);
            protocolRet.AddInt(score);
            ServNet.instance.Broadcast(protocolRet);


        }

        //获取玩家信息
        public void MsgGetAchieve(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetAchieve");
            protocolRet.AddInt(player.data.win);
            protocolRet.AddInt(player.data.fail);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetAchieve" + player.id + player.data.win);
        }


        //开始战斗
        public void MsgStartFight(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("StartFight");
            //条件判断
            if (player.tempData.status != PlayerTempData.Status.Room)
            {
                Console.WriteLine("MsgStartFight status err" + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }
            if (!player.tempData.isOwner)
            {
                Console.WriteLine("MsgStartFight owner err" + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }
            Room room = player.tempData.room;
            if (!room.CanStart())
            {
                Console.WriteLine("MsgStartFight Canstart err" + player.id);
                protocol.AddInt(-1);
                player.Send(protocol);
                return;
            }
            //开始战斗
            protocol.AddInt(0);
            player.Send(protocol);
            room.StartFight();
        }

        //同步坦克单元
        //预测式位置同步
        public void MsgUpdateUnitInfo(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            float posX = protocol.GetFloat(start, ref start);
            float posY = protocol.GetFloat(start, ref start);
            float posZ = protocol.GetFloat(start, ref start);
            float rotX = protocol.GetFloat(start, ref start);
            float rotY = protocol.GetFloat(start, ref start);
            float rotZ = protocol.GetFloat(start, ref start);
            float gunRot = protocol.GetFloat(start, ref start);
            float gunRoll = protocol.GetFloat(start, ref start);
            //获取房间
            if (player.tempData.status != PlayerTempData.Status.Fight)
            {
                Console.WriteLine("房间为空");
                return;
            }
            Room room = player.tempData.room;

            player.tempData.posX = posX;
            player.tempData.posY = posY;
            player.tempData.posZ = posZ;
            player.tempData.lastUpdateTime = Sys.GetTimeStamp();
            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateUnitInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(posX);
            protocolRet.AddFloat(posY);
            protocolRet.AddFloat(posZ);
            protocolRet.AddFloat(rotX);
            protocolRet.AddFloat(rotY);
            protocolRet.AddFloat(rotZ);
            protocolRet.AddFloat(gunRot);
            protocolRet.AddFloat(gunRoll);
            room.Broadcast(protocolRet);


        }
        //shooting 协议
        public void MsgShooting(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            float posX = protocol.GetFloat(start, ref start);
            float posY = protocol.GetFloat(start, ref start);
            float posZ = protocol.GetFloat(start, ref start);
            float rotX = protocol.GetFloat(start, ref start);
            float rotY = protocol.GetFloat(start, ref start);
            float rotZ = protocol.GetFloat(start, ref start);
            //获取房间
            if (player.tempData.status != PlayerTempData.Status.Fight)
                return;
            Room room = player.tempData.room;
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Shooting");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(posX);
            protocolRet.AddFloat(posY);
            protocolRet.AddFloat(posZ);
            protocolRet.AddFloat(rotX);
            protocolRet.AddFloat(rotY);
            protocolRet.AddFloat(rotZ);
            room.Broadcast(protocolRet);
        }
        //伤害同步
        public void MsgHit(Player player, ProtocolBase protoBase)
        {
            //解析协议
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string enemyName = protocol.GetString(start, ref start);
            float damage = protocol.GetFloat(start, ref start);
            //作弊校验
            long lastShootTime = player.tempData.lastShootTime;
            if (Sys.GetTimeStamp() - lastShootTime < 1)
            {
                Console.WriteLine("MsgHit 开炮作弊" + player.id);
                return;
            }
            //获取房间
            if (player.tempData.status != PlayerTempData.Status.Fight)
                return;
            Room room = player.tempData.room;
            //扣除生命值
            if (!room.list.ContainsKey(enemyName))
            {
                Console.WriteLine("MsgHit not Contains enemy" + enemyName);
                return;
            }
            Player enemy = room.list[enemyName];
            if (enemy == null)
                return;
            if (enemy.tempData.hp <= 0)
                return;
            enemy.tempData.hp -= damage;
            Console.WriteLine("MsgHit" + enemyName + "hp" + enemy.tempData.hp);
            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Hit");
            protocolRet.AddString(player.id);
            protocolRet.AddString(enemy.id);
            protocolRet.AddFloat(damage);
            room.Broadcast(protocolRet);
            //胜负判断
            room.UpdateWin();

        }



    }


}
