using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HST_Server
{
    public class Param
    {
        public int CamID;
        public int CPos;
        public int CNeg;
        public int CPos_incr;
        public int CNeg_incr;
        public DateTime DetectTime;
        public double Speed;
        public double AverageUpSpeed;
        public double AverageDownSfpeed;
        public double Density;
    }
    public class ParamHor
    {
        public int CamID;
        public int CPos;
        public int CNeg;
        public int CPos_incr;
        public int CNeg_incr;
        public DateTime DetectTime;
        public double Speed;
        public double AverageUpSpeed;
        public double AverageDownSfpeed;
        public double Density;

        public double maxSpeed;
        public double maxSpeed_up;
        public double maxSpeed_down;
        public double maxDesity;
        public double minSpeed;
        public double minSpeed_up;
        public double minSpeed_down;
        public double minDesity;
    }
    public class Points
    {
        //Point
        public int CamID;
        public DateTime DetectTime;
        public int track_id;
        public string points;
    }
    public class CollectData
    {
        ///历史数据存储容器
        public List<int> list_sec_CPos = new List<int>();
        public List<int> list_sec_CNeg = new List<int>();
        public List<double> list_sec_Speed = new List<double>();
        public List<double> list_sec_Speed_up = new List<double>();
        public List<double> list_sec_Speed_down = new List<double>();
        public List<double> list_sec_Density = new List<double>();

        public List<int> list_hor_CPos = new List<int>();
        public List<int> list_hor_CNeg = new List<int>();
        public List<int> list_hor_CPos_incr = new List<int>();
        public List<int> list_hor_CNeg_incr = new List<int>();
        public List<double> list_hor_Speed = new List<double>();
        public List<double> list_hor_Speed_up = new List<double>();
        public List<double> list_hor_Speed_down = new List<double>();
        public List<double> list_hor_Density = new List<double>();
    }
}
