using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HST_Server
{
    class statisticalData
    {
        int CPos_hor_incr = 0;
        int CNeg_hor_incr = 0;
        double aveSpeed = 0;
        double aveSpeed_up = 0;
        double aveSpeed_down = 0;
        double aveDesity = 0;
        double sumSpeed = 0;
        double sumSpeed_up = 0;
        double sumSpeed_down = 0;
        double sumDesity = 0;
        double maxSpeed = 0;
        double maxSpeed_up = 0;
        double maxSpeed_down = 0;
        double maxDesity = 0;
        double minSpeed = 100;
        double minSpeed_up = 100;
        double minSpeed_down = 100;
        double minDesity = 1;
        public void oneHorSta( int flag ,int CamID, CollectData CollectData, int index_CPos, int index_CNeg, int index_Speed, int index_Speed_up, int index_Speed_down, int index_Desity)
        {
            //上行增量
            if (CollectData.list_sec_CPos.Count>1)
            {
                if (index_CPos == 0)
                {
                    CPos_hor_incr = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - flag] - CollectData.list_sec_CPos[index_CPos];
                }
                else
                {
                    CPos_hor_incr = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - flag] - CollectData.list_sec_CPos[index_CPos - 1];
                }
            }
            else
            {
                CPos_hor_incr = 0;
            }
            //下行增量
            if (CollectData.list_sec_CNeg.Count > 1)
            {
                if (index_CNeg == 0)
                {
                    CNeg_hor_incr = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - flag] - CollectData.list_sec_CNeg[index_CNeg];
                }
                else
                {
                    CNeg_hor_incr = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - flag] - CollectData.list_sec_CNeg[index_CNeg - 1];
                }
            }
            else
            {
                CNeg_hor_incr = 0;
            }
            //平均速度
            for (int i = index_Speed; i < CollectData.list_sec_Speed.Count; i++)
            {
                if (CollectData.list_sec_Speed[i] != 0)
                {
                    sumSpeed += CollectData.list_sec_Speed[i];
                    if (CollectData.list_sec_Speed[i] >= maxSpeed)
                    {
                        maxSpeed = CollectData.list_sec_Speed[i];//最大值
                    }
                    if (CollectData.list_sec_Speed[i] <= minSpeed)
                    {
                        minSpeed = CollectData.list_sec_Speed[i];//最小值
                    }
                }
            }
            if ((CPos_hor_incr + CNeg_hor_incr) != 0)
            {
                aveSpeed = sumSpeed / (CPos_hor_incr + CNeg_hor_incr);
            }
            else
            {
                aveSpeed = 0;
            }
            //平均上行速度
            for (int i = index_Speed_up; i < CollectData.list_sec_Speed_up.Count; i++)
            {
                if (CollectData.list_sec_Speed_up[i] != 0)
                {
                    sumSpeed_up += CollectData.list_sec_Speed_up[i];
                    if (CollectData.list_sec_Speed_up[i] >= maxSpeed_up)
                    {
                        maxSpeed_up = CollectData.list_sec_Speed_up[i];//最大值
                    }
                    if (CollectData.list_sec_Speed_up[i] <= minSpeed_up)
                    {
                        minSpeed_up = CollectData.list_sec_Speed_up[i];//最小值
                    }
                }
            }
            if ((CPos_hor_incr) != 0)
            {
                aveSpeed_up = sumSpeed_up / (CPos_hor_incr);
            }
            else
            {
                aveSpeed_up = 0;
            }
            //平均下行速度
            for (int i = index_Speed_down; i < CollectData.list_sec_Speed_down.Count; i++)
            {
                if (CollectData.list_sec_Speed_down[i] != 0)
                {
                    sumSpeed_down += CollectData.list_sec_Speed_down[i];
                    if (CollectData.list_sec_Speed_down[i] >= maxSpeed_down)
                    {
                        maxSpeed_down = CollectData.list_sec_Speed_down[i];//最大值
                    }
                    if (CollectData.list_sec_Speed_down[i] <= minSpeed_down)
                    {
                        minSpeed_down = CollectData.list_sec_Speed_down[i];//最小值
                    }
                }
            }
            if ((CNeg_hor_incr) != 0)
            {
                aveSpeed_down = sumSpeed_down / (CNeg_hor_incr);
            }
            else
            {
                aveSpeed_down = 0;
            }
            //平均密度
            for (int i = index_Desity; i < CollectData.list_sec_Density.Count; i++)
            {
                sumDesity += CollectData.list_sec_Density[i];
                if (CollectData.list_sec_Density[i] >= maxDesity)
                {
                    maxDesity = CollectData.list_sec_Density[i];//最大值
                }
                if (CollectData.list_sec_Density[i] <= minDesity)
                {
                    minDesity = CollectData.list_sec_Density[i];//最小值
                }
            }
            if ((CollectData.list_sec_Density.Count - index_Desity) != 0)
            {
                aveDesity = sumDesity / (CollectData.list_sec_Density.Count - index_Desity);
            }
            else
            {
                aveDesity = 0;
            }
            MysqlPersistance mysqlPersistance = new MysqlPersistance();
            ParamHor param = new ParamHor();
            param.CamID = CamID;
            param.CPos = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - 1];
            param.CNeg = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - 1];
            if (CPos_hor_incr < 0) { param.CPos_incr = 0; } else { param.CPos_incr = CPos_hor_incr; }
            if (CNeg_hor_incr < 0) { param.CNeg_incr = 0; } else { param.CNeg_incr = CNeg_hor_incr; }
            param.Speed = aveSpeed;
            param.maxSpeed = maxSpeed;
            if (minSpeed == 100){param.minSpeed = 0;}else{ param.minSpeed = minSpeed;}
            param.AverageUpSpeed = aveSpeed_up;
            param.AverageDownSfpeed = aveSpeed_down;
            param.maxSpeed_up = maxSpeed_up;
            param.maxSpeed_down = maxSpeed_down;
            if (minSpeed_up == 100) { param.minSpeed_up = 0; } else { param.minSpeed_up = minSpeed_up; }
            if (minSpeed_down == 100) { param.minSpeed_down = 0; } else { param.minSpeed_down = minSpeed_down; }
            param.Density = aveDesity;
            param.maxDesity = maxDesity;
            if (minDesity == 100) { param.minDesity = 0; } else { param.minDesity = minDesity; }
            if (DateTime.Now.Hour == 0) { 
                param.DetectTime =DateTime.Now.AddMinutes(0);
                param.CPos = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - 2];
                param.CNeg = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - 2];
            }else { param.DetectTime = DateTime.Now; }//为方便按日查询数据，将要00：00插入的数据改为23：59插入
            string table = "pflow_tb_data_hor";
            mysqlPersistance.insertData_Hor(param,table);

            sumSpeed = 0;
            sumSpeed_down = 0;
            sumSpeed_up = 0;
            sumDesity = 0;
            maxSpeed = 0;
            maxSpeed_up = 0;
            maxSpeed_down = 0;
            maxDesity = 0;
            minSpeed = 100;
            minSpeed_up = 100;
            minSpeed_down = 100;
            minDesity = 1;
        }

        public void oneDaySta(int CamID, CollectData CollectData, int index_CPos, int index_CNeg, int index_Speed, int index_Speed_up, int index_Speed_down, int index_Desity)
        {
            //上行增量
            if (CollectData.list_sec_CPos.Count > 1)
            {
                if (index_CPos == 0)
                {
                    CPos_hor_incr = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - 1] - CollectData.list_sec_CPos[index_CPos];
                }
                else
                {
                    CPos_hor_incr = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - 1] - CollectData.list_sec_CPos[index_CPos - 1];
                }
            }
            else
            {
                CPos_hor_incr = 0;
            }
            //下行增量
            if (CollectData.list_sec_CNeg.Count > 1)
            {
                if (index_CNeg == 0)
                {
                    CNeg_hor_incr = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - 1] - CollectData.list_sec_CNeg[index_CNeg];
                }
                else
                {
                    CNeg_hor_incr = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - 1] - CollectData.list_sec_CNeg[index_CNeg - 1];
                }
            }
            else
            {
                CNeg_hor_incr = 0;
            }
            //平均速度
            for (int i = index_Speed; i < CollectData.list_sec_Speed.Count; i++)
            {
                if (CollectData.list_sec_Speed[i] != 0)
                {
                    sumSpeed += CollectData.list_sec_Speed[i];
                    if (CollectData.list_sec_Speed[i] >= maxSpeed)
                    {
                        maxSpeed = CollectData.list_sec_Speed[i];//最大值
                    }
                    if (CollectData.list_sec_Speed[i] <= minSpeed)
                    {
                        minSpeed = CollectData.list_sec_Speed[i];//最小值
                    }
                }
            }
            if ((CPos_hor_incr + CNeg_hor_incr) != 0)
            {
                aveSpeed = sumSpeed / (CPos_hor_incr + CNeg_hor_incr);
            }
            else
            {
                aveSpeed = 0;
            }
            //平均上行速度
            for (int i = index_Speed_up; i < CollectData.list_sec_Speed_up.Count; i++)
            {
                if (CollectData.list_sec_Speed_up[i] != 0)
                {
                    sumSpeed_up += CollectData.list_sec_Speed_up[i];
                    if (CollectData.list_sec_Speed_up[i] >= maxSpeed_up)
                    {
                        maxSpeed_up = CollectData.list_sec_Speed_up[i];//最大值
                    }
                    if (CollectData.list_sec_Speed_up[i] <= minSpeed_up)
                    {
                        minSpeed_up = CollectData.list_sec_Speed_up[i];//最小值
                    }
                }
            }
            if ((CPos_hor_incr) != 0)
            {
                aveSpeed_up = sumSpeed_up / (CPos_hor_incr);
            }
            else
            {
                aveSpeed_up = 0;
            }
            //平均下行速度
            for (int i = index_Speed_down; i < CollectData.list_sec_Speed_down.Count; i++)
            {
                if (CollectData.list_sec_Speed_down[i] != 0)
                {
                    sumSpeed_down += CollectData.list_sec_Speed_down[i];
                    if (CollectData.list_sec_Speed_down[i] >= maxSpeed_down)
                    {
                        maxSpeed_down = CollectData.list_sec_Speed_down[i];//最大值
                    }
                    if (CollectData.list_sec_Speed_down[i] <= minSpeed_down)
                    {
                        minSpeed_down = CollectData.list_sec_Speed_down[i];//最小值
                    }
                }
            }
            if ((CNeg_hor_incr) != 0)
            {
                aveSpeed_down = sumSpeed_down / (CNeg_hor_incr);
            }
            else
            {
                aveSpeed_down = 0;
            }
            //平均密度
            for (int i = index_Desity; i < CollectData.list_sec_Density.Count; i++)
            {
                sumDesity += CollectData.list_sec_Density[i];
                if (CollectData.list_sec_Density[i] >= maxDesity)
                {
                    maxDesity = CollectData.list_sec_Density[i];//最大值
                }
                if (CollectData.list_sec_Density[i] <= minDesity)
                {
                    minDesity = CollectData.list_sec_Density[i];//最小值
                }
            }
            if ((CollectData.list_sec_Density.Count - index_Desity) != 0)
            {
                aveDesity = sumDesity / (CollectData.list_sec_Density.Count - index_Desity);
            }
            else
            {
                aveDesity = 0;
            }
            MysqlPersistance mysqlPersistance = new MysqlPersistance();
            ParamHor param = new ParamHor();
            param.CamID = CamID;
            param.CPos = CollectData.list_sec_CPos[CollectData.list_sec_CPos.Count - 1];
            param.CNeg = CollectData.list_sec_CNeg[CollectData.list_sec_CNeg.Count - 1];
            if (CPos_hor_incr < 0) { param.CPos_incr = 0; } else { param.CPos_incr = CPos_hor_incr; }
            if (CNeg_hor_incr < 0) { param.CNeg_incr = 0; } else { param.CNeg_incr = CNeg_hor_incr; }
            param.Speed = aveSpeed;
            param.maxSpeed = maxSpeed;
            if (minSpeed == 100) { param.minSpeed = 0; } else { param.minSpeed = minSpeed; }
            param.AverageUpSpeed = aveSpeed_up;
            param.AverageDownSfpeed = aveSpeed_down;
            param.maxSpeed_up = maxSpeed_up;
            param.maxSpeed_down = maxSpeed_down;
            if (minSpeed_up == 100) { param.minSpeed_up = 0; } else { param.minSpeed_up = minSpeed_up; }
            if (minSpeed_down == 100) { param.minSpeed_down = 0; } else { param.minSpeed_down = minSpeed_down; }
            param.Density = aveDesity;
            param.maxDesity = maxDesity;
            if (minDesity == 100) { param.minDesity = 0; } else { param.minDesity = minDesity; }
            param.DetectTime = DateTime.Now;
            string table = "pflow_tb_data_day";
            mysqlPersistance.insertData_Hor(param, table);

            sumSpeed = 0;
            sumSpeed_down = 0;
            sumSpeed_up = 0;
            sumDesity = 0;
            maxSpeed = 0;
            maxSpeed_up = 0;
            maxSpeed_down = 0;
            maxDesity = 0;
            minSpeed = 100;
            minSpeed_up = 100;
            minSpeed_down = 100;
            minDesity = 1;
        }

        public void Sta(int CamID, CollectData cData,string table)
        {
            int CPos_incr = 0;
            int CNeg_incr = 0;
            double sumSpeed_up = 0;
            double aveSpeed_up = 0;
            double sumSpeed_down = 0;
            double aveSpeed_down = 0;
            double sumSpeed = 0;
            double aveSpeed = 0;
            double sumDensity = 0;
            double aveDensity = 0;
            //上行增量
            if (cData.list_hor_CPos_incr.Count > 1)
            {
                for(int i=0;i<cData.list_hor_CPos_incr.Count;i++)
                {
                    CPos_incr += cData.list_hor_CPos_incr[i];
                }
            }
            else
            {
                CPos_incr = 0;
            }
            //下行增量
            if (cData.list_hor_CNeg_incr.Count > 1)
            {
                for (int i = 0; i < cData.list_hor_CNeg_incr.Count; i++)
                {
                    CNeg_incr += cData.list_hor_CNeg_incr[i];
                }
            }
            else
            {
                CPos_incr = 0;
            }
            //平均速度
            for (int i = 0; i < cData.list_hor_Speed.Count; i++)
            {
                if (cData.list_hor_Speed[i] != 0)
                {
                    sumSpeed += cData.list_hor_Speed[i];
                }
            }
            if (cData.list_hor_Speed.Count != 0)
            {
                aveSpeed = sumSpeed / cData.list_hor_Speed.Count;
            }
            else
            {
                aveSpeed = 0;
            }
            //平均上行速度
            for (int i = 0; i < cData.list_hor_Speed_up.Count; i++)
            {
                if (cData.list_hor_Speed_up[i] != 0)
                {
                    sumSpeed_up += cData.list_hor_Speed_up[i];
                }
            }
            if (cData.list_hor_Speed_up.Count != 0)
            {
                aveSpeed_up = sumSpeed_up / cData.list_hor_Speed_up.Count;
            }
            else
            {
                aveSpeed_up = 0;
            }
            //平均下行速度
            for (int i = 0; i < cData.list_hor_Speed_down.Count; i++)
            {
                if (cData.list_hor_Speed_down[i] != 0)
                {
                    sumSpeed_down += cData.list_hor_Speed_down[i];
                }
            }
            if (cData.list_hor_Speed_down.Count != 0)
            {
                aveSpeed_down = sumSpeed_down / cData.list_hor_Speed_down.Count;
            }
            else
            {
                aveSpeed_down = 0;
            }
            //平均密度
            for (int i = 0; i < cData.list_hor_Density.Count; i++)
            {
                if (cData.list_hor_Density[i] != 0)
                {
                    sumDensity += cData.list_hor_Density[i];
                }
            }
            if (cData.list_hor_Density.Count != 0)
            {
                aveDensity = sumDensity / cData.list_hor_Density.Count;
            }
            else
            {
                aveDensity = 0;
            }
            MysqlPersistance mysqlPersistance = new MysqlPersistance();
            ParamHor param = new ParamHor();
            param.CamID = CamID;
            if (cData.list_hor_CPos.Count > 0) { param.CPos = cData.list_hor_CPos[cData.list_hor_CPos.Count - 1]; }
            else { param.CPos = 0; }
            if (cData.list_hor_CNeg.Count > 0) { param.CNeg = cData.list_hor_CNeg[cData.list_hor_CNeg.Count - 1]; }
            else { param.CNeg = 0; }
            if (CPos_incr < 0) { param.CPos_incr = 0; } else { param.CPos_incr = CPos_incr; }
            if (CNeg_incr < 0) { param.CNeg_incr = 0; } else { param.CNeg_incr = CNeg_incr; }
            param.Speed = aveSpeed;
            param.AverageUpSpeed = aveSpeed_up;
            param.AverageDownSfpeed = aveSpeed_down;
            param.Density = aveDesity;
            param.DetectTime = DateTime.Now;

            
            mysqlPersistance.insertData_Hor(param, table);
        }

    }
}
