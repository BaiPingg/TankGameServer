using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 服务器001.Logic
{
    public class PlayerTempData
    {
        //状态
        public enum Status
        {
            None,
            Room,
            Fight
        }
        public Status status;
        public PlayerTempData()
        {
            status = Status.None;
        }
        //room
        public Room room;
        public int team = 1;
        public bool isOwner = false;
        //战场相关
        public long lastUpdateTime;
        public float posX;
        public float posY;
        public float posZ;
        public long lastShootTime;
        public float hp = 100;
    }
}
