using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace 服务器001.Logic
{
    [Serializable]
    public class PlayerDate
    {
        public int score = 0;
        public int win = 0;
        public int fail = 0;
        public PlayerDate()
        {
            score = 100;
        }
    }
}
