using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Concurrent;
using MySql.Data.MySqlClient;
//using WindowsFormsApplication1;
//using Client;
//using System.Windows.Forms;
//2017 By zhangx

namespace HST_Server
{
    public class MysqlPersistance
    {
        List<string> paramalarmList_mult = new List<string>();
        public int insertData(Param param)
        {
            //List<string> paramalarmList = new List<string>();
            int return_int = 0;
            string sql = string.Format("insert into pflow_tb_data ( CamID, DetectTime ,CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed) values({0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9});select @@Identity as id;",
                                                                         param.CamID,
                                                                         param.DetectTime,
                                                                         param.CPos,
                                                                         param.CNeg,
                                                                         param.CPos_incr,
                                                                         param.CNeg_incr,
                                                                         param.Density,
                                                                         param.Speed,
                                                                         param.AverageUpSpeed,
                                                                         param.AverageDownSfpeed);
            paramalarmList_mult.Add(sql);
            if (paramalarmList_mult.Count > 2)
            {
                if (MysqlHelper.Default.ExecuteNoQueryTran(paramalarmList_mult))
                {
                    //return 1;
                    return_int = 1;
                }
                else
                {
                    return_int = -1;
                }
                paramalarmList_mult.Clear();
                //return -1;  // 插入失败                
            }
            return return_int;
        }

        public int insertData_Hor(ParamHor param ,string table)
        {
            List<string> paramalarmList = new List<string>();
            string sql = string.Format("insert into {18} ( CamID, DetectTime ,CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed,maxSpeed,maxSpeed_up,maxSpeed_down,maxDesity,minSpeed,minSpeed_up,minSpeed_down,minDesity) values({0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17});select @@Identity as id;",
                                                                         param.CamID,
                                                                         param.DetectTime,
                                                                         param.CPos,
                                                                         param.CNeg,
                                                                         param.CPos_incr,
                                                                         param.CNeg_incr,
                                                                         param.Density,
                                                                         param.Speed,
                                                                         param.AverageUpSpeed,
                                                                         param.AverageDownSfpeed,
                                                                         param.maxSpeed,
                                                                         param.maxSpeed_up,
                                                                         param.maxSpeed_down,
                                                                         param.maxDesity,
                                                                         param.minSpeed,
                                                                         param.minSpeed_up,
                                                                         param.minSpeed_down,
                                                                         param.minDesity,
                                                                         table);
            paramalarmList.Add(sql);
            if (MysqlHelper.Default.ExecuteNoQueryTran(paramalarmList))
            {
                return 1;
            }
            return -1;  // 插入失败
        }

        public int insertPoint(Points param)
        {
            List<string> paramalarmList = new List<string>();
            string sql = string.Format("insert into pflow_tb_points (DetectTime ,track_id ,points,CamID) values('{0}',{1},'{2}',{3});select @@Identity as id;",
                                                                         param.DetectTime,
                                                                         param.track_id,
                                                                         param.points,
                                                                         param.CamID);
            paramalarmList.Add(sql);
            if (MysqlHelper.Default.ExecuteNoQueryTran(paramalarmList))
            {
                return 1;
            }
            return -1;  // 插入失败
        }

        #region [从数据库中读小时参数]
        /// <summary>
        /// load param and alram from database
        /// </summary>
        /// <param name="param"></param>
        public int loadHorData(CollectData collectData, int camID)
        {
            string sql = string.Format("select CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed from pflow_tb_data_hor where to_days(DetectTime) = to_days(now()) and CamID = {0};", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    if (Convert.ToInt32(dr["CPos"].ToString()) != 0)//total
                    {
                        collectData.list_hor_CPos.Add(Convert.ToInt32(dr["CPos"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg.Add(Convert.ToInt32(dr["CNeg"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CPos_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CPos_incr.Add(Convert.ToInt32(dr["CPos_incr"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg_incr.Add(Convert.ToInt32(dr["CNeg_incr"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Density"].ToString()) != 0)//
                    {
                        collectData.list_hor_Density.Add(Convert.ToDouble(dr["Density"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Speed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed.Add(Convert.ToDouble(dr["Speed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageUpSpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_up.Add(Convert.ToDouble(dr["AverageUpSpeed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageDownSfpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_down.Add(Convert.ToDouble(dr["AverageDownSfpeed"].ToString()));
                    }
                } return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region [从数据库中读7天参数]
        /// <summary>
        /// load param and alram from database
        /// </summary>
        /// <param name="param"></param>
        public int load7DayData(CollectData collectData, int camID)
        {
            string sql = string.Format("select CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed from pflow_tb_data_day  where DetectTime between current_date()-7 and sysdate() and CamID = {0};", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    if (Convert.ToInt32(dr["CPos"].ToString()) != 0)//total
                    {
                        collectData.list_hor_CPos.Add(Convert.ToInt32(dr["CPos"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg.Add(Convert.ToInt32(dr["CNeg"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CPos_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CPos_incr.Add(Convert.ToInt32(dr["CPos_incr"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg_incr.Add(Convert.ToInt32(dr["CNeg_incr"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Density"].ToString()) != 0)//
                    {
                        collectData.list_hor_Density.Add(Convert.ToDouble(dr["Density"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Speed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed.Add(Convert.ToDouble(dr["Speed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageUpSpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_up.Add(Convert.ToDouble(dr["AverageUpSpeed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageDownSfpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_down.Add(Convert.ToDouble(dr["AverageDownSfpeed"].ToString()));
                    }
                } return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion

        public int Data(string param)
        {
            List<string> paramalarmList = new List<string>();
            string sql = string.Format("insert into data ( data) values({0});select @@Identity as id;",
                                                                         param);
            paramalarmList.Add(sql);
            if (MysqlHelper.Default.ExecuteNoQueryTran(paramalarmList))
            {
                return 1;
            }
            return -1;  // 插入失败
        }

        #region [从数据库中读1月参数]
        /// <summary>
        /// load param and alram from database
        /// </summary>
        /// <param name="param"></param>
        public int load1MonthData(CollectData collectData, int camID)
        {
            string sql = string.Format("select CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed from pflow_tb_data_day  where DATE_SUB(CURDATE(), INTERVAL 1 MONTH) <= date(DetectTime) and CamID = {0};", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    if (Convert.ToInt32(dr["CPos"].ToString()) != 0)//total
                    {
                        collectData.list_hor_CPos.Add(Convert.ToInt32(dr["CPos"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg.Add(Convert.ToInt32(dr["CNeg"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CPos_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CPos_incr.Add(Convert.ToInt32(dr["CPos_incr"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg_incr.Add(Convert.ToInt32(dr["CNeg_incr"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Density"].ToString()) != 0)//
                    {
                        collectData.list_hor_Density.Add(Convert.ToDouble(dr["Density"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Speed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed.Add(Convert.ToDouble(dr["Speed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageUpSpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_up.Add(Convert.ToDouble(dr["AverageUpSpeed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageDownSfpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_down.Add(Convert.ToDouble(dr["AverageDownSfpeed"].ToString()));
                    }
                } return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion


        #region [从数据库中读1年参数]
        /// <summary>
        /// load param and alram from database
        /// </summary>
        /// <param name="param"></param>
        public int load1YearData(CollectData collectData, int camID)
        {
            string sql = string.Format("select CPos,CNeg ,CPos_incr,CNeg_incr,Density ,Speed,AverageUpSpeed,AverageDownSfpeed from pflow_tb_data_day  where DATE_SUB(CURDATE(), INTERVAL 12 MONTH) <= date(DetectTime) and CamID = {0};", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    if (Convert.ToInt32(dr["CPos"].ToString()) != 0)//total
                    {
                        collectData.list_hor_CPos.Add(Convert.ToInt32(dr["CPos"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg.Add(Convert.ToInt32(dr["CNeg"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CPos_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CPos_incr.Add(Convert.ToInt32(dr["CPos_incr"].ToString()));
                    }
                    if (Convert.ToInt32(dr["CNeg_incr"].ToString()) != 0)//
                    {
                        collectData.list_hor_CNeg_incr.Add(Convert.ToInt32(dr["CNeg_incr"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Density"].ToString()) != 0)//
                    {
                        collectData.list_hor_Density.Add(Convert.ToDouble(dr["Density"].ToString()));
                    }
                    if (Convert.ToDouble(dr["Speed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed.Add(Convert.ToDouble(dr["Speed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageUpSpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_up.Add(Convert.ToDouble(dr["AverageUpSpeed"].ToString()));
                    }
                    if (Convert.ToDouble(dr["AverageDownSfpeed"].ToString()) != 0)//
                    {
                        collectData.list_hor_Speed_down.Add(Convert.ToDouble(dr["AverageDownSfpeed"].ToString()));
                    }
                } return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion

        public int Dataqq(string param)
        {
            List<string> paramalarmList = new List<string>();
            string sql = string.Format("insert into data ( data) values({0});select @@Identity as id;",
                                                                         param);
            paramalarmList.Add(sql);
            if (MysqlHelper.Default.ExecuteNoQueryTran(paramalarmList))
            {
                return 1;
            }
            return -1;  // 插入失败
        }


        #region [从数据库中读参数]
        /// <summary>
        /// load param and alram from database
        /// </summary>
        /// <param name="param"></param>
        public void loadData(Param param,int camID)
        {
            string sql = string.Format("select CamID, DetectTime ,CPos,CNeg  ,Density ,Speed from tb_data where ID=(select max(ID))and CamID = {0};", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[0];
                    param.CamID = Convert.ToInt32(dr["CamID"].ToString());
                    param.DetectTime = Convert.ToDateTime(dr["DetectTime"].ToString());
                    param.CPos = Convert.ToInt32(dr["CPos"].ToString());
                    param.CNeg = Convert.ToInt32(dr["CNeg"].ToString());
                    param.Density = Convert.ToDouble(dr["Density"].ToString());
                    param.Speed = Convert.ToDouble(dr["Speed"].ToString());
                }
            }
        }

        public int loadLastdata(Param param, int camID)
        {
            string sql = string.Format("select * from tb_data where CamID = {0} order by ID desc limit 0,1;", camID);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[0];
                    param.CamID = Convert.ToInt32(dr["CamID"].ToString());
                    param.DetectTime = Convert.ToDateTime(dr["DetectTime"].ToString());
                    param.CPos = Convert.ToInt32(dr["CPos"].ToString());
                    param.CNeg = Convert.ToInt32(dr["CNeg"].ToString());
                    param.Density = Convert.ToDouble(dr["Density"].ToString());
                    param.Speed = Convert.ToDouble(dr["Speed"].ToString());
                }
            }
            return 1;
        }
        #endregion

        #region [delete TB]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public void deleteTB(string table)
        {
            string sql = string.Format("delete from {0}",table);
            object r1 = MysqlHelper.Default.ExecuteScalar(sql);
            string sql1 = string.Format("truncate table {0}", table);//the incremental primary key strart at 1
            object r11 = MysqlHelper.Default.ExecuteScalar(sql1);
        }
        #endregion

        #region [复制表]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public void copyTB(string table,string totable)
        {
            string sql = string.Format("insert into {0} (traficVolume,trafic1,trafic2,averageLaneSpaceOccupancy,flowRate,density,carID,detectTime,detectEndTime,locateRoad,direction,speed,carHeadway,carSpaceHeadway,points) select traficVolume,trafic1,trafic2,averageLaneSpaceOccupancy,flowRate,density,carID,detectTime,detectEndTime,locateRoad,direction,speed,carHeadway,carSpaceHeadway,points from {1};", totable, table);
            object r1 = MysqlHelper.Default.ExecuteScalar(sql);

            string sql1 = string.Format("delete from {0}", table);
            object r2 = MysqlHelper.Default.ExecuteScalar(sql1);
            string sql2 = string.Format("truncate table {0}", table);//the incremental primary key strart at 1
            object r3 = MysqlHelper.Default.ExecuteScalar(sql2);
        }
        #endregion

        #region [key]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="Ch"></param>
        public int check_tb(string table)
        {
            int count = 0;
            string sql = string.Format("select count(ID) from {0} ;", table);
            DataTable dt = MysqlHelper.Default.ExecuteDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                count = Convert.ToInt32(dr["count(ID)"].ToString());
            }
            return count;
        }
        #endregion

        #region []
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="Ch"></param>
        public void create_tb(string table)
        {
            string sql = string.Format("CREATE TABLE {0} (`ID`  int(11) NOT NULL AUTO_INCREMENT ,`camID`  int(11) NULL ,`time`  datetime(6) NULL ,`passengerVolume`  int(11) NULL ,`Speed`  double(11,2) NULL ,PRIMARY KEY (`ID`));", table);
            MysqlHelper.Default.ExecuteNonQuery(sql);
        }
        #endregion

        #region []
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="Ch"></param>
        public void create_basic_tb(string table)
        {
            string sql = string.Format("CREATE TABLE {0} (`ID`  int(255) NOT NULL AUTO_INCREMENT ,`traficVolume`  int(255) NULL ,`trafic1`  int(255) NULL ,`trafic2`  int(255) NULL ,`carID`  int(255) NULL ,`detectTime`  datetime(6) NULL ,`detectEndTime`  datetime(6) NULL ,`locateRoad`  int(255) NULL ,`direction`  int(255) NULL ,`speed`  double(255,2) NULL ,`carHeadway`  double(255,2) NULL ,`carSpaceHeadway`  double(255,2) NULL ,`averageLaneSpaceOccupancy`  double(255,2) NULL ,`flowRate`  double(255,2) NULL ,`density`  double(255,2) NULL ,`points`  text NULL ,PRIMARY KEY (`ID`));", table);
            MysqlHelper.Default.ExecuteNonQuery(sql);
        }
        #endregion

        #region [复制表]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public void copyTB2(string table, string totable)
        {
            string sql = string.Format("insert into {0} (camID,time,traficVolume,timeMeanSpeed,spaceMeanSpeed) select camID,time,traficVolume,timeMeanSpeed,spaceMeanSpeed from {1};", totable, table);
            object r1 = MysqlHelper.Default.ExecuteScalar(sql);

            string sql1 = string.Format("delete from {0}", table);
            object r2 = MysqlHelper.Default.ExecuteScalar(sql1);
            string sql2 = string.Format("truncate table {0}", table);//the incremental primary key strart at 1
            object r3 = MysqlHelper.Default.ExecuteScalar(sql2);
        }
        #endregion
    }
}
