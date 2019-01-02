using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using 服务器001.Logic;

namespace 服务器001.Core
{
    public class DataMgr
    {
        MySqlConnection sqlConn;
        //单例
        public static DataMgr instance;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DataMgr()
        {
            instance = this;
            Connect();
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        public void Connect()
        {
            //数据库
            string connStr = "server=localhost;user id=root;password=224746; database=game;";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr] MusqlConnect" + e.Message);
                return;
            }
        }

        /// <summary>
        /// 判断输入是否为安全字符
        /// </summary>
        /// <param name="str">传入一个字符串</param>
        /// <returns></returns>
        public bool IsSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        /// <summary>
        /// 根据传入的id判断是否存在该用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true：可以注册，false：不能注册</returns>
        private bool CanRegister(string id)
        {
            //防止sql注入
            //先判断是不是安全字符
            if (!IsSafeStr(id))
                return false;

            //查询id是否存在
            string cmdStr = string.Format("select * from user where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {   
                //开启数据库连接
                MySqlDataReader datareader = cmd.ExecuteReader();
                bool hasRows = datareader.HasRows;
                datareader.Close();//关闭
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="id">名称</param>
        /// <param name="pw">密码</param>
        /// <returns></returns>
        public bool Register(string id, string pw)
        {
            //防止sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw))
            {
                Console.WriteLine("[DataMgr] Register使用非法字符");
                return false;
            }
            //能否注册
            if (!CanRegister(id))
            {
                Console.WriteLine("[DaraMgr]Register!CantRegister");
                return false;
            }
            
            string cmdStr = string.Format("insert into user set id='{0}',pw='{1}'", id, pw);
            //写入数据库User表
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DaraMgr]Register" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 根据id创建角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Createplayer(string id)
        {
            //防止sql注入
            if (!IsSafeStr(id))
                return false;
            //序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerDate playerDate = new PlayerDate();
            try
            {
                formatter.Serialize(stream, playerDate);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DaraMgr]CreatePlayer 序列化" + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            //写入数据库
            string cmdStr = string.Format("insert into player set id = '{0}',data = @data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DaraMgr]CreatePlayer 写入" + e.Message);
                return false;
            }

        }

        /// <summary>
        ///  检查用户名和密码
        /// </summary>
        public bool CheckPassWord(string id, string pw)
        {
            //防止sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw))
                return false;
            //查询
            string cmdStr = string.Format("select * from user where id='{0}'and pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DaraMgr]CheckPassWord" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public PlayerDate GetPlayerData(string id)
        {
            PlayerDate playerDate = null;
            //防止sql注入
            if (!IsSafeStr(id))
                return playerDate;
            //查询
            string cmdStr = string.Format("select * from player where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerDate;
                }
                dataReader.Read();
                long len = dataReader.GetBytes(1, 0, null, 0, 0);//1是data
                buffer = new byte[len];
                dataReader.GetBytes(1, 0, buffer, 0, (int)len);
                dataReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[DaraMgr]GetPlayerData 查询" + e.Message);
                return playerDate;
            }
            //反序列化
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerDate = (PlayerDate)formatter.Deserialize(stream);
                return playerDate;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerDate 反序列化" + e.Message);
                return playerDate;
            }

        }

        /// <summary>
        /// 保存角色
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerDate playerDate = player.data;
            //反序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, playerDate);
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化" + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            //写入数据库
            string formatStr = "update player set data =@data where id = '{0}';";
            string cmdStr = string.Format(formatStr, player.id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 写入" + e.Message);
                return false;
            }
        }
    }


}
