using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace HST_Server
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HST_Result
    {
        public Int32 center_x;
        public Int32 center_y;
    }

    public struct Point                        //存储轨迹点坐标
    {
        public int x;
        public int y;
    }

    public struct Human                        //保存人员信息，用于行人轨迹接续存储
    {
        public int camid;                      //摄像头ID号码
        public int id;                         //人员ID号码
        public int direction;                  //移动方向
        public int flag;                       //标志位，用于判别当前人员是否经过区域统计计算，0表示未进行统计计算，1表示已经进行过统计计算
        public double speed;                   //移动速度
        public List<Point> points;             //行人轨迹点集
    }

    //public struct Humans
    //{
    //    public int camid;
    //    public List<Human> humans;
    //}

    public struct Speed                        //保存人眼速度信息，利用ID号进行不同行人的区分
    {
        public int camid;                      //摄像头ID号码
        public int id;                         //人员ID号码
        public int direction;                  //行走方向
        public double speed;                   //行人速度
    }

    public struct HumansCensus                 //单个摄像头的单帧数据
    {
        public int camid;
        public int totalHumanVolume;           //总量式区域所有人员数量
        public int upHumanVolume;              //总量式区域上行人员数量
        public int downHumanVolume;            //总量式区域下行人员数量
        public int deltHuman;                  //增量式区域所有人员数量
        public int deltUpHuman;                //增量式区域上行人员数量
        public int deltDownHuman;              //增量式区域下行人员数量
    }

    public struct OutputData                   //单路视频相关数据
    {
        public int CamID;                      //摄像头编号
        public bool AverageSpeedFlag;          //总平均速度变量标志位，真则区域平均速度不为空，假则区域平均速度为空，空表示无人而非速度为0
        public bool AverageUpSpeedFlag;        //上行平均速度标志位
        public bool AverageDownSpeedFlag;      //下行平均速度标志位
        public double AverageSpeed;            //区域行人平均速度
        public double AverageUpSpeed;
        public double AverageDownSfpeed;
        public int TotalHumanVolume;           //总量式区域所有人员数量
        public int UpHumanVolume;              //总量式区域上行人员数量
        public int DownHumanVolume;            //总量式区域下行人员数量
        public int DeltHuman;                  //增量式区域所有人员数量
        public int DeltUpHuman;                //增量式区域上行人员数量
        public int DeltDownHuman;              //增量式区域下行人员数量
        public int AverageDensity;             //平均密度
    }

    public struct Density                      //密度
    {
        public int camid;
        public List<int> density;              //当前帧位于检测区内的人员数量
    }

    public class Parameters
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //需要从数据库读取相关计算参数以及相应的路段信息
        public string CamID = "1";
        public string Road = "";
        public string StreamAddress = "";
        public int VideoWidth = 640;
        public int VideoHeight = 480;
        public int FrameRate = 25;
        public int CompressVideoWidth = 640;
        public int CompressVideoHeight = 480;
        public int ROIX = 0;
        public int ROIY = 0;
        public int ROIWidth = 640;
        public int ROIHeight = 480;
        public int UpCountLine = 0;
        public int DownCountLine = 0;
        public int LeftCountLine = 0;
        public int RightCountLine = 0;
        public int CamHeight = 8;
        public double CamInclineAngle = 45;
        public double CamTransverseAngle = 58;
        public double CamVerticalAngle = 43;
        public int SplitLine = 0;                                                       //分隔线用于分隔检测区域  2017年9月13日13:30:45
        public int SPPoint1x = 0;                                                           //斜线点靠近x轴，检测框上部的坐标位置
        public int SPPoint2x = 0;                                                           //斜线点远离x轴，检测框底部的坐标位置
        public int SPPoint1y = 0;                                                           //斜线点靠近x轴，检测框上部的坐标位置
        public int SPPoint2y = 0;                                                           //斜线点远离x轴，检测框底部的坐标位置
        public int DataStatisticTime = 5;                                              //动态统计时间调节以秒为单位 2017年9月13日13:34:29

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //将以上由数据库读取的信息赋值给下面对应的信息，还需要将部分数据库读取的信息通过命名管道发送给C++用作检测跟踪处理的参数
        public static double pi = 3.14159;                                               //默认摄像机参数，可调整为从设置文件或者数据库读取相应参数
        public static double TiltAngle = 45;                                            //摄像机倾斜角度
        public static double camHigh = 8;                                               //摄像机高度
        public static double carHeight = 0.8;                                            //车辆高度
        public static double alpha = (43 * pi) / 180;                                    //横向拍摄角度转换为弧度
        public static double beta = (58 * pi) / 180;                                     //纵向拍摄角度转换为弧度
        public static double timeDif = 1.0 / 25;
        public static int frameWidth = 640;                                              //服务器端C#处理视频分辨率
        public static int frameHeight = 480;
        public static int splitLine = 0;                                                 //分隔线  2017年9月13日15:25:04用于分隔检测区域
        public static int dataStatisticFrame = 125;                                      //动态统计时间调节以帧为单位 2017年9月13日15:25:11
        public static int roix = 0;
        public static int roiy = 0;
        public static int roiWidth = 640;
        public static int roiHeight = 480;

        //public Parameters(CamInfoParam _param)  //默认构造函数初始化参数值
        //{
        //    CamID = _param.CamID;
        //    Road = _param.Road;
        //    StreamAddress = _param.StreamAddress;
        //    TiltAngle = _param.CamInclineAngle;
        //    camHigh = _param.CamHeight;        
        //    alpha = (_param.CamVerticalAngle * pi) / 180;
        //    beta = (_param.CamTransverseAngle * pi) / 180;
        //    timeDif = 1.0 / _param.FrameRate;
        //    frameWidth = _param.VideoWidth;
        //    frameHeight = _param.VideoHeight;
        //    roix = _param.ROIX;
        //    roiy = _param.ROIY;
        //    roiWidth = _param.ROIWidth;
        //    roiHeight = _param.ROIHeight;
        //    roiArea = _param.ROIWidth * _param.ROIHeight;
        //    //splitLine = _param.SpliteLine;                                        //分隔线用于分隔检测区域  2017年9月13日15:27:50
        //    splitLine = (_param.SPPoint1x + _param.SPPoint2x) / 2;
        //    sPPoint1x = _param.SPPoint1x;                                         //用于确定斜线分隔线
        //    sPPoint2x = _param.SPPoint2x;
        //    sPPoint1y = _param.SPPoint1y;
        //    sPPoint2y = _param.SPPoint2y;
        //    dataStatisticFrame = _param.DataStatisticTime * _param.FrameRate;    //动态统计时间调节以帧为单位  2017年9月13日15:27:55
        //}
    }

    public class DisCalculate                                       //计算距离
    {
        public double xDis(Point p1, Point p2)  //返回x方向的距离
        {
            int startx = p1.x;
            int starty = p1.y;
            int endx = p2.x;
            int endy = p2.y;
            double gama1 = Math.Atan((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / (starty + (Parameters.frameHeight - starty) * Math.Cos(Parameters.alpha)));  //距离计算
            double gama2 = Math.Atan((Parameters.frameHeight - endy) * Math.Sin(Parameters.alpha) / (endy + (Parameters.frameHeight - endy) * Math.Cos(Parameters.alpha)));
            double dy = 3 * (Parameters.camHigh - Parameters.carHeight) * (Math.Sin(gama1) / Math.Cos((Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama1)) - Math.Sin(gama2) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama2)) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2);
            double seta = Math.Asin((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / Parameters.frameHeight);
            double L = (Parameters.camHigh - Parameters.carHeight) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + seta);
            double dx = 4 * L * (endx - startx) / Math.Sqrt((starty - Parameters.frameHeight / 2) * (starty - Parameters.frameHeight / 2) + ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)) * ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)));
            //double distance = Math.Sqrt(dx * dx + dy * dy);
            return dx;
        }

        public double yDis(Point p1, Point p2)  //返回y方向的距离
        {
            int startx = p1.x;
            int starty = p1.y;
            int endx = p2.x;
            int endy = p2.y;
            double gama1 = Math.Atan((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / (starty + (Parameters.frameHeight - starty) * Math.Cos(Parameters.alpha)));  //距离计算
            double gama2 = Math.Atan((Parameters.frameHeight - endy) * Math.Sin(Parameters.alpha) / (endy + (Parameters.frameHeight - endy) * Math.Cos(Parameters.alpha)));
            double dy = 3 * (Parameters.camHigh - Parameters.carHeight) * (Math.Sin(gama1) / Math.Cos((Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama1)) - Math.Sin(gama2) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama2)) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2);
            double seta = Math.Asin((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / Parameters.frameHeight);
            double L = (Parameters.camHigh - Parameters.carHeight) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + seta);
            double dx = 4 * L * (endx - startx) / Math.Sqrt((starty - Parameters.frameHeight / 2) * (starty - Parameters.frameHeight / 2) + ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)) * ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)));
            //double distance = Math.Sqrt(dx * dx + dy * dy);
            return dy;
        }

        public double Dis(Point p1, Point p2)  //返回总体的距离
        {
            int startx = p1.x;
            int starty = p1.y;
            int endx = p2.x;
            int endy = p2.y;
            double gama1 = Math.Atan((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / (starty + (Parameters.frameHeight - starty) * Math.Cos(Parameters.alpha)));  //距离计算
            double gama2 = Math.Atan((Parameters.frameHeight - endy) * Math.Sin(Parameters.alpha) / (endy + (Parameters.frameHeight - endy) * Math.Cos(Parameters.alpha)));
            double dy = 3 * (Parameters.camHigh - Parameters.carHeight) * (Math.Sin(gama1) / Math.Cos((Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama1)) - Math.Sin(gama2) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + gama2)) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2);
            double seta = Math.Asin((Parameters.frameHeight - starty) * Math.Sin(Parameters.alpha) / Parameters.frameHeight);
            double L = (Parameters.camHigh - Parameters.carHeight) / Math.Cos(Parameters.TiltAngle * Parameters.pi / 180 - Parameters.alpha / 2 + seta);
            double dx = 4 * L * (endx - startx) / Math.Sqrt((starty - Parameters.frameHeight / 2) * (starty - Parameters.frameHeight / 2) + ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)) * ((Parameters.frameWidth / 2) / Math.Sin(Parameters.beta / 2)));
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;
        }
    }

    public class ParamsCalculate
    {
        public bool AverageSpeedFlag = false;  //总平均速度变量标志位，真则区域平均速度不为空，假则区域平均速度为空，空表示无人而非速度为0
        public bool AverageUpSpeedFlag = false;//上行平均速度标志位
        public bool AverageDownSpeedFlag = false;//下行平均速度标志位
        public double AverageSpeed = 0;        //区域行人平均速度
        public double AverageUpSpeed = 0;
        public double AverageDownSfpeed = 0;
        public int TotalHumanVolume = 0;       //总量式区域所有人员数量
        public int UpHumanVolume = 0;          //总量式区域上行人员数量
        public int DownHumanVolume = 0;        //总量式区域下行人员数量
        public int DeltHuman = 0;              //增量式区域所有人员数量
        public int DeltUpHuman = 0;            //增量式区域上行人员数量
        public int DeltDownHuman = 0;          //增量式区域下行人员数量
        public int AverageDensity = 0;         //平均密度

        public int totalHumanVolume = 0;       //总量式区域所有人员数量
        public int upHumanVolume = 0;          //总量式区域上行人员数量
        public int downHumanVolume = 0;        //总量式区域下行人员数量
        public int deltHuman = 0;              //增量式区域所有人员数量
        public int deltUpHuman = 0;            //增量式区域上行人员数量
        public int deltDownHuman = 0;          //增量式区域下行人员数量

        public int frameNum = 1;               //当前处理帧号
        public int censusFrameNum = 1;         //数据统计帧号
        public bool timeRemapFlag = true;      //数据清零标志位
        
        List<List<Human>> HumanAux = new List<List<Human>>();//用于前后两个统计时间间隔内数据的更替
        List<List<Human>> AllHumans = new List<List<Human>>();//存放所有摄像头检测出的人员数据
        //List<List<Human>> AllHumansNull = new List<List<Human>>(1);//空指向
        List<List<Speed>> AllSpeedData = new List<List<Speed>>();//存放所有摄像头速度数据
        //List<List<Speed>> AllSpeedDataNull = new List<List<Speed>>(1);//空指向
        public List<OutputData> AllOutputData = new List<OutputData>();//所有摄像头的输出数据，按照内部CamID区分
        List<HumansCensus> AllHumansCensus = new List<HumansCensus>();//所有摄像头各自的人数统计
        List<Density> AllDensity = new List<Density>();//全部摄像头密度

        public DisCalculate disCalculate = new DisCalculate();

        public ParamsCalculate()  //默认构造函数，用于对参数计算过程所用到的摄像头参数进行初始化
        {
            //结合摄像头的相关信息进行数据的初始化
        }

        public bool calculate(SHstPipeInfo sPipeInfo)  //进行计算的主要函数，数据输入格式为管道接收数据的格式
        {
            Density density = new Density();//单路摄像头密度
            Point point = new Point();
            Point point2 = new Point();
            //Human humanzero = new Human() {camid=-2, id=-2, flag=-2, direction=-2, speed=-2};  //指向默认初始化的值
            //humanzero.points = new List<Point>();
            
            //List<Human> HumansNull = new List<Human>();
            //if(AllHumansNull.Count() < 1)
            //{
            //    HumansNull.Add(human);
            //    AllHumansNull.Add(HumansNull);  //空指向无效时取消注释
            //}
            for (int i = 0; i < sPipeInfo.iVideoCount; i++)//Debug 第i个摄像头  修改为摄像头的实际数目IVideoCount，该数目为固定的数字10，不表示实际开启的摄像头路数
            {
                //密度，每一路视频对应各帧的人数
                List<int> densityNumber = new List<int>();
                density.camid = sPipeInfo.szVideoInfo[i].iVideoID;  //摄像头编号
                densityNumber.Add(sPipeInfo.szVideoInfo[i].iObjCount);  //人数                    
                density.density = densityNumber;
                if (AllDensity.Count() > 0)
                {
                    int adty = 0;
                    for (; adty < AllDensity.Count(); adty++)
                    {
                        if (sPipeInfo.szVideoInfo[i].iVideoID == AllDensity[adty].camid)  //Debug sPipeInfo.szVideoInfo[i].iVideoID
                        {
                            AllDensity[adty].density.Add(sPipeInfo.szVideoInfo[i].iObjCount);
                            break;  //跳出循环，避免后续继续执行，使其执行当前camid所对应的行人信息的采集提取
                        }
                    }
                    if (adty >= AllDensity.Count()) //新摄像头的密度
                    {
                        AllDensity.Add(density);
                    }
                }
                else  //首次添加数据
                {
                    AllDensity.Add(density);  //当前摄像头的当前帧的人数
                }

                int j = 0;
                for (; j < sPipeInfo.szVideoInfo[i].iObjCount; j++)//第j个轨迹点
                {
                    Human human = new Human();  //定义、实例、初始化
                    human.camid = -1;
                    human.id = -1;  //数据初始化，使其不为空
                    human.flag = -1;
                    human.direction = -1;
                    human.speed = -1;
                    human.points = new List<Point>();  //存放行人轨迹
                    human.camid = sPipeInfo.szVideoInfo[i].iVideoID;    //摄像头编号
                    human.id = sPipeInfo.szVideoInfo[i].szObjInfo[j].ID;
                    point.x = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_x;
                    point.y = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_y;
                    human.points.Add(point);   //用于首次添加行人信息，使得存储结构中轨迹点不为空
                    //List<Human> HumansMatch = new List<Human>();  //用于匹配比较
                    if (AllHumans.Count() > 0)  //存放所有人员的容器不为空
                    {
                        int ah = 0;
                        for (; ah < AllHumans.Count(); ah++)  //先遍历寻找匹配的第i路摄像头能否匹配
                        {
                            //HumansMatch = AllHumans[ah];  //其中的ah下标，可用于新行人信息添加，因为后面如果可以匹配成功则该下标ah必定有效；匹配使用HumansMatch
                            if (AllHumans[ah].Count() < 1)//添加对Humans[0]的判断
                            {
                                continue;  //如果Humans[0]不存在则跳过循环的后续部分，转而查询下一个camid是否满足条件
                            }
                            if (sPipeInfo.szVideoInfo[i].iVideoID == AllHumans[ah][0].camid)  //Debug 可能存在 Hunmans[0]不存在的情况；仅判断摄像头号码是否匹配，等价于i == AllHumans[ah][0].camid     sPipeInfo.szVideoInfo[i].iVideoID
                            {
                                int hi = 0;
                                for (; hi < AllHumans[ah].Count(); hi++)  //遍历容器寻找ID号相同的行人
                                {
                                    if (AllHumans[ah][hi].id == sPipeInfo.szVideoInfo[i].szObjInfo[j].ID)  //匹配成功，在该轨迹之后添加轨迹点
                                    {
                                        point.x = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_x;
                                        point.y = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_y;
                                        AllHumans[ah][hi].points.Add(point);  //Debug 添加当前行人轨迹点

                                        //1.轨迹点接续存储
                                        if (human.points.Count() > 0)
                                        {
                                            human.points.Clear();       //清空存放的当前camid的摄像头的当前帧一个形心点的坐标数据
                                        }
                                        human.flag = AllHumans[ah][hi].flag;   //由行人信息存储结构中取出当前人的信息
                                        human.direction = AllHumans[ah][hi].direction;
                                        human.speed = AllHumans[ah][hi].speed;
                                        human.points = AllHumans[ah][hi].points;  //Debug
                                        //for (int pi = 0; pi < Humans[hi].points.Count(); pi++)
                                        //{
                                        //    point.x = Humans[hi].points[pi].x;
                                        //    point.y = Humans[hi].points[pi].y;
                                        //    human.points.Add(point);
                                        //}

                                        //2.方向确认
                                        //位于此处原因：满足指定点数要求的轨迹可按照流程执行到此处，达到指定的点数需要多帧数据的累积
                                        if (AllHumans[ah][hi].points.Count() >= 20)  //Debug  允许进行方向确认的轨迹点数需要的最小数值
                                        {
                                            if (-1 == AllHumans[ah][hi].direction)  //表明此前未进行过方向判别，首次执行
                                            {
                                                if (AllHumans[ah][hi].points[0].y - AllHumans[ah][hi].points[AllHumans[ah][hi].points.Count() - 1].y >= 0)
                                                {
                                                    human.direction = 1;     //上行
                                                }
                                                else
                                                {
                                                    human.direction = 2;     //下行
                                                }
                                            }
                                            AllHumans[ah][hi] = human;              //当前行人信息更新  direction
                                        }

                                        //3.人数统计
                                        if (AllHumans[ah][hi].points.Count() >= 20)  //Debug 进行人数统计的条件
                                        {
                                            if (-1 == human.flag)             //此前该轨迹未进行数量统计
                                            {
                                                //统计变量每次递增，使用匹配替换增加原则
                                                totalHumanVolume = 0;  //重新初始化
                                                upHumanVolume = 0;
                                                downHumanVolume = 0;
                                                deltHuman = 0;
                                                deltUpHuman = 0;
                                                deltDownHuman = 0;
                                                HumansCensus humansCensus = new HumansCensus();
                                                if (1 == human.direction)
                                                {
                                                    totalHumanVolume += 1;
                                                    upHumanVolume += 1;
                                                    deltHuman += 1;
                                                    deltUpHuman += 1;
                                                }
                                                else
                                                {
                                                    totalHumanVolume += 1; ;
                                                    downHumanVolume += 1;
                                                    deltHuman += 1;
                                                    deltDownHuman += 1;
                                                }

                                                int hc = 0;
                                                if (AllHumansCensus.Count() > 0)
                                                {
                                                    for (; hc < AllHumansCensus.Count(); hc++)
                                                    {
                                                        if (sPipeInfo.szVideoInfo[i].iVideoID == AllHumansCensus[hc].camid)  //Debug 查询到该路段保存的历史统计数据  sPipeInfo.szVideoInfo[i].iVideoID
                                                        {
                                                            totalHumanVolume += AllHumansCensus[hc].totalHumanVolume;
                                                            upHumanVolume += AllHumansCensus[hc].upHumanVolume;
                                                            downHumanVolume += AllHumansCensus[hc].downHumanVolume;
                                                            deltHuman += AllHumansCensus[hc].deltHuman;
                                                            deltUpHuman += AllHumansCensus[hc].deltUpHuman;
                                                            deltDownHuman += AllHumansCensus[hc].deltDownHuman;
                                                            break;
                                                        }
                                                    }//此处，超出界限的处理见新摄像头数据                                                    
                                                }
                                                else
                                                {
                                                    //无
                                                }

                                                human.flag = 1;  //置位，作为新出现人员，人数加一
                                                AllHumans[ah][hi] = human;          //当前行人信息更新  flag

                                                humansCensus.camid = sPipeInfo.szVideoInfo[i].iVideoID;    //Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                humansCensus.totalHumanVolume = totalHumanVolume;
                                                humansCensus.upHumanVolume = upHumanVolume;
                                                humansCensus.downHumanVolume = downHumanVolume;
                                                humansCensus.deltHuman = deltHuman;
                                                humansCensus.deltUpHuman = deltUpHuman;
                                                humansCensus.deltDownHuman = deltDownHuman;

                                                if (hc >= AllHumansCensus.Count())  //Debug 与后续匹配的情形多重复执行一次，未匹配，有新摄像头的数据
                                                {
                                                    AllHumansCensus.Add(humansCensus);
                                                }
                                                if (AllHumansCensus.Count() > 0)  //有关于不同摄像头的人数统计
                                                {
                                                    AllHumansCensus[hc] = humansCensus;  //匹配成功的添加增量并替换
                                                }
                                                else  //首次，此前无不同摄像头的人数统计
                                                {
                                                    AllHumansCensus.Add(humansCensus);
                                                }
                                            }
                                        }

                                        //4.个体速度
                                        Speed speed = new Speed();
                                        speed.camid = -1;  //数据初始化
                                        speed.direction = -1;
                                        speed.id = -1;
                                        speed.speed = -1;
                                        //List<Speed> SpeedNull = new List<Speed>();
                                        //if(AllSpeedDataNull.Count() < 1)
                                        //{
                                        //    SpeedNull.Add(speed);
                                        //    AllSpeedDataNull.Add(SpeedNull);
                                        //}
                                        if (AllHumans[ah][hi].points.Count() >= 20)  //Debug 速度计算条件
                                        {
                                            int speedCalculateFunc = 1;      //计算方法
                                            switch (speedCalculateFunc)
                                            {
                                                case 1:                      //按照每两个点构成的组进行进行个体速度计算
                                                    {
                                                        double deltDis = 0;
                                                        double humanMeanSpeed = 0;                                //该轨迹平均速度                                                   
                                                        double secHumanSpeed = 0;                                 //该轨迹某相邻两点间的速度
                                                        double sumHumanSpeed = 0;                                 //该轨迹所有瞬时速度总和
                                                        double selectSumSpeed = 0;                                //筛选速度总和
                                                        int selectSpeedNumber = 0;                                //筛选速度组数
                                                        int groupSpeedNumber = 0;                                 //每两点构成计算速度的有效组数

                                                        for (int si = 0; si < AllHumans[ah][hi].points.Count() - 1; si++)//计算该轨迹平均速度，两点构成的速度组数
                                                        {
                                                            if ((AllHumans[ah][hi].points.Count() - 1) >= 16)  //Debug
                                                            {
                                                                //该跳过语句的判断条件是方法1速度筛选条件的非形式
                                                                if (((AllHumans[ah][hi].points.Count() - 1) / 2 - 4 > si) || ((AllHumans[ah][hi].points.Count() - 1) / 2 + 4 < si))  //如果组数大于16组，跳过未被选中的组
                                                                {
                                                                    if (si != (AllHumans[ah][hi].points.Count() - 2))
                                                                    {
                                                                        continue;
                                                                    }

                                                                }
                                                            }

                                                            point = AllHumans[ah][hi].points[si];                            //提取轨迹点1进行计算
                                                            point2 = AllHumans[ah][hi].points[si + 1];                       //提取轨迹点2进行计算
                                                            deltDis = disCalculate.Dis(point, point2);                //距离计算                                                    
                                                            secHumanSpeed = deltDis / Parameters.timeDif;             //通过两轨迹点的速度m/s
                                                            if (si != (AllHumans[ah][hi].points.Count() - 2))
                                                            {
                                                                if ((AllHumans[ah][hi].points.Count() - 1) >= 16)            //Debug 筛选速度，速度组数大于等于阈值，选取速度不为0的组数（轨迹点数 - 1）/ 2组计算筛选速度
                                                                {
                                                                    if (((AllHumans[ah][hi].points.Count() - 1) / 2 - 4 <= si) && ((AllHumans[ah][hi].points.Count() - 1) / 2 + 4 >= si))  //动态筛选轨迹中间一半的轨迹进行筛选速度的计算
                                                                    {
                                                                        if (secHumanSpeed != 0)                       //筛选速度中每组的速度不为零
                                                                        {
                                                                            selectSpeedNumber++;                      //筛选速度不为0组数加1
                                                                            selectSumSpeed += secHumanSpeed;          //筛选速度的速度总和
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if ((AllHumans[ah][hi].points.Count() - 1) < 16)                 //Debug 不足组数按照常规方法求基础平均速度
                                                            {
                                                                if (secHumanSpeed != 0)
                                                                {
                                                                    groupSpeedNumber++;                               //方法1求平均速度不为0的组数加1
                                                                    sumHumanSpeed += secHumanSpeed;                   //该轨迹当前帧速度总和
                                                                }
                                                            }
                                                        }

                                                        if ((AllHumans[ah][hi].points.Count() - 1) >= 16)                    //Debug 满足组数要求，筛选速度替换基础速度
                                                        {
                                                            if (0 == selectSpeedNumber)
                                                            {
                                                                selectSpeedNumber = 1;
                                                            }
                                                            humanMeanSpeed = 1.0 * selectSumSpeed / selectSpeedNumber;
                                                        }
                                                        else
                                                        {
                                                            if (0 == groupSpeedNumber)
                                                            {
                                                                groupSpeedNumber = 1;
                                                            }
                                                            humanMeanSpeed = 1.0 * sumHumanSpeed / groupSpeedNumber;  //基础速度；对应容器内部包含的轨迹点，构成的计算速度的组数为(轨迹点数 - 1)组，speedGroupNum为速度不为0组数
                                                        }
                                                        human.speed = humanMeanSpeed;
                                                        AllHumans[ah][hi] = human;                                           //当前行人信息更新  speed
                                                        speed.camid = sPipeInfo.szVideoInfo[i].iVideoID;  //Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                        speed.id = human.id;
                                                        speed.direction = human.direction;
                                                        speed.speed = humanMeanSpeed;

                                                        if (AllSpeedData.Count > 0)
                                                        {
                                                            int asd = 0;
                                                            for (; asd < AllSpeedData.Count; asd++)
                                                            {
                                                                //SpeedData = AllSpeedData[asd];
                                                                if(AllSpeedData[asd].Count() < 1)  //跳过当前循环的后续执行步骤，避免出现下标越界
                                                                {
                                                                    continue;
                                                                }
                                                                if (sPipeInfo.szVideoInfo[i].iVideoID == AllSpeedData[asd][0].camid)  //同一个摄像头  Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                                {
                                                                    int asdi = 0;
                                                                    for (; asdi < AllSpeedData[asd].Count(); asdi++)  //某个摄像头信息匹配
                                                                    {
                                                                        if (speed.id == AllSpeedData[asd][asdi].id)
                                                                        {
                                                                            AllSpeedData[asd][asdi] = speed;
                                                                            //SpeedData = AllSpeedDataNull[0];//空指向
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (asdi < AllSpeedData[asd].Count()) //已经匹配成功，直接跳过
                                                                    {
                                                                        break;
                                                                    }
                                                                    else  //未匹配成功，但是为同一个摄像头内新增加的数据
                                                                    {
                                                                        AllSpeedData[asd].Add(speed);
                                                                        //SpeedData = AllSpeedDataNull[0];//空指向  后面添加break  Debug
                                                                        break;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    continue;  //从剩余的路段中选取数据
                                                                }
                                                            }
                                                            if (asd >= AllSpeedData.Count())  //新摄像头数据
                                                            {
                                                                List<Speed> NewSpeed = new List<Speed>();
                                                                NewSpeed.Add(speed);
                                                                AllSpeedData.Add(NewSpeed);
                                                            }
                                                        }
                                                        else  //首次存放某路摄像头的数据信息
                                                        {
                                                            List<Speed> FirstSpeed = new List<Speed>();
                                                            FirstSpeed.Add(speed);                                         //将该人信息放入速度容器
                                                            AllSpeedData.Add(FirstSpeed);
                                                        }
                                                    }
                                                    break;

                                                case 2:                                                               //按照首尾轨迹点构成的组进行个体速度计算
                                                    {
                                                        int pointNum = 0;                                             //轨迹点数
                                                        double deltDis = 0;
                                                        double humanMeanSpeed = 0;                                    //该轨迹平均速度                                                   

                                                        if (AllHumans[ah][hi].points.Count() >= 17)                          //轨迹点大于等于17个（对应法1的16组），筛选中间的轨迹点
                                                        {
                                                            point = AllHumans[ah][hi].points[(AllHumans[ah][hi].points.Count() / 2) - (AllHumans[ah][hi].points.Count() / 4)];  //提取轨迹点1进行计算
                                                            point2 = AllHumans[ah][hi].points[(AllHumans[ah][hi].points.Count() / 2) + (AllHumans[ah][hi].points.Count() / 4)]; //提取轨迹点2进行计算
                                                            pointNum = 2 * (AllHumans[ah][hi].points.Count() / 4) + 1;    //实际的点数
                                                        }
                                                        else                                                       //点数低于17个选择首尾点
                                                        {
                                                            point = AllHumans[ah][hi].points[0];                             //轨迹的第一个轨迹点
                                                            point2 = AllHumans[ah][hi].points[AllHumans[ah][hi].points.Count() - 1];//轨迹的最后一个轨迹点
                                                            pointNum = AllHumans[ah][hi].points.Count();                  //实际的点数
                                                        }
                                                        deltDis = disCalculate.Dis(point, point2);                 //距离计算
                                                        humanMeanSpeed = deltDis / (Parameters.timeDif * (pointNum - 1));  //通过两轨迹点的速度m/s
                                                        human.speed = humanMeanSpeed;
                                                        AllHumans[ah][hi] = human;                                        //当前行人信息更新  speed
                                                        speed.id = human.id;
                                                        speed.direction = human.direction;
                                                        speed.speed = humanMeanSpeed;

                                                        if (AllSpeedData.Count > 0)
                                                        {
                                                            int asd = 0;
                                                            for (; asd < AllSpeedData.Count; asd++)
                                                            {
                                                                //SpeedData = AllSpeedData[asd];
                                                                if(AllSpeedData[asd].Count() < 1)
                                                                {
                                                                    continue;
                                                                }
                                                                if (sPipeInfo.szVideoInfo[i].iVideoID == AllSpeedData[asd][0].camid)  //同一个摄像头  Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                                {
                                                                    int asdi = 0;
                                                                    for (; asdi < AllSpeedData[asd].Count(); asdi++)  //某个摄像头信息匹配
                                                                    {
                                                                        if (speed.id == AllSpeedData[asd][asdi].id)
                                                                        {
                                                                            AllSpeedData[asd][asdi] = speed;
                                                                            //SpeedData = AllSpeedDataNull[0];//空指向
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (asdi < AllSpeedData[asd].Count()) //已经匹配成功，直接跳过
                                                                    {
                                                                        break;
                                                                    }
                                                                    else  //未匹配成功，但是为同一个摄像头内新增加的数据
                                                                    {
                                                                        AllSpeedData[asd].Add(speed);
                                                                        //SpeedData = AllSpeedDataNull[0];//空指向  后面添加break  Debug
                                                                        break;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    continue;
                                                                }
                                                            }
                                                            if (asd >= AllSpeedData.Count())  //新摄像头数据
                                                            {
                                                                List<Speed> NewSpeed = new List<Speed>();
                                                                NewSpeed.Add(speed);
                                                                AllSpeedData.Add(NewSpeed);
                                                            }
                                                        }
                                                        else  //首次存放某路摄像头的数据信息
                                                        {
                                                            List<Speed> FirstSpeed = new List<Speed>();
                                                            FirstSpeed.Add(speed);                                         //将该人信息放入速度容器
                                                            AllSpeedData.Add(FirstSpeed);
                                                        }
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                        break;                         //匹配成功后，轨迹点添加执行完毕，退出不执行后续循环
                                    }
                                    else
                                    {
                                        continue;  //继续后次执行，直至行人的ID号码匹配或者跳出循环使得hi >= Humans.Count()
                                    }
                                }
                                if (hi >= AllHumans[ah].Count())  //在存放第i路摄像头所对应的容器内无轨迹号相同的，添加到当前存储行人信息的容器中,HumansMatch仅仅用于判别，不参与存储信息的实际更变
                                {
                                    AllHumans[ah].Add(human);   //直接在存储结构中添加
                                    //Humans = AllHumansNull[0];//空指向  其后添加break  Debug
                                    break;
                                }
                                else
                                {
                                    break;  //跳出在后续的camid编号所对应的摄像头存储信息的匹配查找
                                }
                            }
                            else
                            {
                                continue;  //继续执行直至摄像头camid相同或者ah >= AllHumans.Count()
                            }
                        }
                        if (ah >= AllHumans.Count())  //遍历存储结构，未能匹配，视作新摄像头；此时包含存储结构为空，导致无法匹配进而增加的可能性 Debug
                        {
                            List<Human> NewHumans = new List<Human>(); //AllHumansNull[0];//空指向
                            NewHumans.Add(human);
                            AllHumans.Add(NewHumans);  //其后进行空指向操作后，依旧指向AllHumansNull[0]并且此时的AllHumansNull[0]存放
                        }
                        else
                        {                            
                            continue;//表示在某个camid所对应的摄像头中的行人ID匹配成功，继续进行下一个行人检测信息的匹配操作
                        }
                    }
                    else  //首次执行，存放所有人员信息的容器为空
                    {
                        List<Human> FirstHumans = new List<Human>();//用于首次存储人员信息的容器为空时向内部放数据
                        FirstHumans.Add(human);
                        AllHumans.Add(FirstHumans);
                        //Humans = AllHumansNull[0];//空指向
                    }
                }
            }

            //5.数据统计  //推送数据为当前统计时间内的数据
            if (Parameters.dataStatisticFrame == censusFrameNum)
            {
                censusFrameNum = 0;
                OutputData outputData = new OutputData();//存放单个摄像头的输出数据
                int camNumID = 0;  //摄像头编号
                int camNumber = 0;
                for (; camNumber < sPipeInfo.iVideoCount; camNumber++)  //Debug  sPipeInfo.szVideoInfo.Length -》 sPipeInfo.iVideoCount
                {
                    camNumID = sPipeInfo.szVideoInfo[camNumber].iVideoID;
                    //5.1人数统计
                    if (AllHumansCensus.Count() > 0)  //存放所有人员信息容器非空，以该程序中的camID编号为序，由此向下依次计算各个摄像头的相关数据，数据计算完毕后送至该摄像头的所对应的数据输出部分，随后执行下一个摄像头数据的计算
                    {
                        int ahc = 0;
                        for (; ahc < AllHumansCensus.Count(); ahc++)
                        {
                            if (camNumID == AllHumansCensus[ahc].camid)//找到编号为camNumID的摄像头，取人数统计数据
                            {
                                outputData.CamID = camNumID;
                                outputData.TotalHumanVolume = AllHumansCensus[ahc].totalHumanVolume;
                                outputData.UpHumanVolume = AllHumansCensus[ahc].upHumanVolume;
                                outputData.DownHumanVolume = AllHumansCensus[ahc].downHumanVolume;
                                outputData.DeltHuman = AllHumansCensus[ahc].deltHuman;
                                outputData.DeltUpHuman = AllHumansCensus[ahc].deltUpHuman;
                                outputData.DeltDownHuman = AllHumansCensus[ahc].deltDownHuman;

                                //对应5.5部分的数据零，主要用于增量式的数据归零重计
                                HumansCensus humansCensus = new HumansCensus();
                                humansCensus.camid = AllHumansCensus[ahc].camid;
                                humansCensus.totalHumanVolume = AllHumansCensus[ahc].totalHumanVolume;
                                humansCensus.upHumanVolume = AllHumansCensus[ahc].upHumanVolume;
                                humansCensus.downHumanVolume = AllHumansCensus[ahc].downHumanVolume;
                                humansCensus.deltHuman = 0; //增量式区域总人数清零
                                humansCensus.deltUpHuman = 0;
                                humansCensus.deltDownHuman = 0;
                                AllHumansCensus[ahc] = humansCensus;//数据更新，增量式数据清零
                                break;
                            }
                        }
                        if (ahc >= AllHumansCensus.Count())
                        {
                            //Console.WriteLine("Current Track Length is too Short!");
                            outputData.CamID = camNumID;
                            outputData.TotalHumanVolume = 0;  //Debug
                            outputData.UpHumanVolume = 0;
                            outputData.DownHumanVolume = 0;
                            outputData.DeltHuman = 0;
                            outputData.DeltUpHuman = 0;
                            outputData.DeltDownHuman = 0;
                        }
                    }
                    else //空，当前摄像头的人数统计信息人工置位
                    {
                        outputData.CamID = camNumID;
                        outputData.TotalHumanVolume = 0;  //Debug
                        outputData.UpHumanVolume = 0;
                        outputData.DownHumanVolume = 0;
                        outputData.DeltHuman = 0;
                        outputData.DeltUpHuman = 0;
                        outputData.DeltDownHuman = 0;
                    }

                    //5.2速度统计
                    if (AllSpeedData.Count() > 0)
                    {
                        int aspd = 0;
                        for (; aspd < AllSpeedData.Count(); aspd++)
                        {
                            int aspdi = 0;
                            for (; aspdi < AllSpeedData[aspd].Count(); aspdi++)
                            {
                                if (camNumID == AllSpeedData[aspd][aspdi].camid)  //匹配，速度存储为同摄像头存储
                                {
                                    //curSpeed = AllSpeedData[aspd];
                                    if (AllSpeedData[aspd].Count() > 0)
                                    {
                                        double totalspeed = 0;
                                        double upspeed = 0;
                                        double downspeed = 0;
                                        int total = 0;
                                        int up = 0;
                                        int down = 0;

                                        for (int spd = 0; spd < AllSpeedData[aspd].Count(); spd++)
                                        {
                                            if (1 == AllSpeedData[aspd][spd].direction)        //上行
                                            {
                                                //if (0 < AllSpeedData[aspd][spd].speed && AllSpeedData[aspd][spd].speed < 3)
                                                {
                                                    total++;
                                                    up++;
                                                    totalspeed += AllSpeedData[aspd][spd].speed;
                                                    upspeed += AllSpeedData[aspd][spd].speed;
                                                }
                                            }
                                            else                                     //下行
                                            {
                                                //if (0 < AllSpeedData[aspd][spd].speed && AllSpeedData[aspd][spd].speed < 3)
                                                {
                                                    total++;
                                                    down++;
                                                    totalspeed += AllSpeedData[aspd][spd].speed;
                                                    downspeed += AllSpeedData[aspd][spd].speed;
                                                }
                                            }
                                        }

                                        if (total != 0)
                                        {
                                            this.AverageSpeedFlag = true;
                                            this.AverageSpeed = totalspeed / total;
                                        }
                                        else
                                        {
                                            this.AverageSpeedFlag = false;
                                            this.AverageSpeed = 0;
                                        }
                                        if (up != 0)
                                        {
                                            this.AverageUpSpeedFlag = true;
                                            this.AverageUpSpeed = upspeed / up;
                                        }
                                        else
                                        {
                                            this.AverageUpSpeedFlag = false;
                                            this.AverageUpSpeed = 0;
                                        }
                                        if (down != 0)
                                        {
                                            this.AverageDownSpeedFlag = true;
                                            this.AverageDownSfpeed = downspeed / down;
                                        }
                                        else
                                        {
                                            this.AverageDownSpeedFlag = false;
                                            this.AverageDownSfpeed = 0;
                                        }
                                    }
                                    else
                                    {
                                        this.AverageSpeedFlag = false;              //表明对应类型的速度为空
                                        this.AverageUpSpeedFlag = false;
                                        this.AverageDownSpeedFlag = false;
                                        this.AverageSpeed = 0;
                                        this.AverageUpSpeed = 0;
                                        this.AverageDownSfpeed = 0;
                                    }
                                    outputData.AverageSpeedFlag = this.AverageSpeedFlag;
                                    outputData.AverageSpeed = this.AverageSpeed;
                                    outputData.AverageUpSpeedFlag = this.AverageUpSpeedFlag;
                                    outputData.AverageUpSpeed = this.AverageUpSpeed;
                                    outputData.AverageDownSpeedFlag = this.AverageDownSpeedFlag;
                                    outputData.AverageDownSfpeed = this.AverageDownSfpeed;
                                }
                            }
                            if (aspdi >= AllSpeedData[aspd].Count()) //未找到与当前摄像头标号相同的数据，换摄像头查询
                            {
                                continue;
                            }
                        }
                        if (aspd >= AllSpeedData.Count())
                        {
                            //Console.WriteLine("This step is meaningless, Object is too Short!");
                            outputData.CamID = camNumID;//存在重复赋值，但是可以避免重复添加摄像头
                            outputData.AverageSpeedFlag = false;
                            outputData.AverageSpeed = 0;
                            outputData.AverageUpSpeedFlag = false;
                            outputData.AverageUpSpeed = 0;
                            outputData.AverageDownSpeedFlag = false;
                            outputData.AverageDownSfpeed = 0;
                        }
                    }
                    else //存储所有摄像头人员速度容器为空
                    {
                        outputData.CamID = camNumID;//存在重复赋值，但是可以避免重复添加摄像头
                        outputData.AverageSpeedFlag = false;
                        outputData.AverageSpeed = 0;
                        outputData.AverageUpSpeedFlag = false;
                        outputData.AverageUpSpeed = 0;
                        outputData.AverageDownSpeedFlag = false;
                        outputData.AverageDownSfpeed = 0;
                    }

                    //5.3平均密度
                    if (AllDensity.Count() > 0)
                    {
                        int adst = 0;
                        for (; adst < AllDensity.Count(); adst++)
                        {
                            if (camNumID == AllDensity[adst].camid) //摄像头配备成功
                            {
                                int validDst = 0;
                                int sum = 0;
                                if (AllDensity[adst].density.Count() > 0) //可按照人数不为0的视频帧数量计算平均密度
                                {
                                    for (int dstNum = 0; dstNum < AllDensity[adst].density.Count(); dstNum++)
                                    {
                                        if (AllDensity[adst].density[dstNum] > 0)
                                        {
                                            validDst += 1;
                                            sum += AllDensity[adst].density[dstNum];
                                        }
                                    }
                                    if (validDst > 0)
                                    {
                                        outputData.AverageDensity = (int)(1.0 * sum / validDst);
                                        break;
                                    }
                                    else
                                    {
                                        outputData.AverageDensity = 0;
                                    }
                                }
                                else //空，置零
                                {
                                    outputData.AverageDensity = 0;
                                }
                            }
                        }
                        if (adst > AllDensity.Count())  //容器不为空，但是内部没有与当前摄像头关联匹配的信息
                        {
                            //Console.WriteLine("TThis step Won't be Executed or Object is too Short!");
                            outputData.AverageDensity = 0;  //Debug
                        }
                    }
                    else  //容器为空
                    {
                        outputData.AverageDensity = 0;
                    }

                    //5.4数据输出
                    if (AllOutputData.Count() > 0)
                    {
                        int aod = 0;
                        for (; aod < AllOutputData.Count(); aod++)
                        {
                            if (camNumID == AllOutputData[aod].CamID)  //匹配，更新其中的流量统计数据
                            {
                                AllOutputData[aod] = outputData;//数据更新操作，更新人数、速度、密度
                                break;
                            }
                        }
                        if (aod >= AllOutputData.Count())
                        {
                            AllOutputData.Add(outputData);  //Debug
                        }
                    }
                    else  //首次执行
                    {
                        AllOutputData.Add(outputData);
                    }

                  
                    //5.5间隔接续
                    int alterNumber = 0;//设置在两个统计时间间隔内需要更新的数据，避免容器自增
                    bool breakFlag = false;  //结合判断用于跳出循环
                    //List<Human> humansaux = new List<Human>();
                    //List<Human> HumansContinue = new List<Human>();
                    //HumansContinue = AllHumansNull[0];//空指向
                    int ah = 0;
                    for (; ah < AllHumans.Count(); ah++)
                    {
                        if (breakFlag)
                        {
                            break;
                        }
                        if (AllHumans[ah].Count() > 0)
                        {
                            int ahi = 0;
                            for (; ahi < AllHumans[ah].Count(); ahi++)
                            {
                                if (breakFlag)  //跳出
                                {
                                    break;
                                }
                                if (camNumID == AllHumans[ah][ahi].camid)  //至此，在AllHumans中找到需要进行数据更替的摄像头
                                {
                                    Human humansContinue = new Human();  //更新替换需要保留的数据
                                    if (HumanAux.Count() > 0)
                                    {
                                        int ha = 0;
                                        for (; ha < HumanAux.Count(); ha++)  //在HumansAux中寻找与camNumID标号相同的摄像头所对应的数据，若找到相同的则将内部保存的数据量
                                        {
                                            if (breakFlag)
                                            {
                                                break;
                                            }
                                            for (int hai = 0; hai < HumanAux[ha].Count(); hai++)  //HumansAux中第ha个camid的行人数据
                                            {
                                                if (breakFlag)
                                                {
                                                    break;
                                                }
                                                if (camNumID == HumanAux[ha][hai].camid)  //摄像头匹配成功  //获取该摄像头在HumanAux中存储的人数个数并清空该摄像头的数据，然后重新放入（当前统计时间间隔所有数据 - 历史统计时间间隔所有数据）
                                                { 
                                                    alterNumber = AllHumans[ah].Count(); //该摄像头在历史统计时间间隔保存的数据（行人信息）个数
                                                    //HumanAux[ha].Clear(); //该摄像头在历史统计时间间隔内的数据清空
                                                    //此处存在问题，将行人的信息删除Humans.RemoveRange(0, alterNumber);  //删除该摄像头在当前统计时间间隔内所包含的历史统计时间间隔内的数据
                                                    //List<Human> deltHumans = new List<Human>();  //临时盛放行人信息
                                                    List<Human> NewHumans_2 = new List<Human>();  //针对新添加的行人
                                                    for (int an = 0; an < alterNumber; an++)  //将某个摄像头内部去除历史所包含的行人信息，剩余的行人信息全部提取出来，通过humansaux更新替换到HumansAux结构中
                                                    {
                                                        int ani = 0;
                                                        for (; ani < HumanAux[ha].Count(); ani++ ) //在辅助存储内部查找与Humans中行人ID号相同且点数较少的行人ID，以及只在Humans中新增的行人ID
                                                        {

                                                            if ( (AllHumans[ah][an].id == HumanAux[ha][ani].id) && (AllHumans[ah][an].points.Count() > HumanAux[ha][ani].points.Count()) )
                                                            {
                                                                humansContinue.camid = AllHumans[ah][an].camid;
                                                                humansContinue.direction = AllHumans[ah][an].direction;
                                                                humansContinue.flag = AllHumans[ah][an].flag;
                                                                humansContinue.id = AllHumans[ah][an].id;
                                                                humansContinue.speed = AllHumans[ah][an].speed;
                                                                humansContinue.points = AllHumans[ah][an].points;
                                                                //for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                                                //{
                                                                //    point.x = Humans[an].points[pts].x;
                                                                //    point.y = Humans[an].points[pts].y;
                                                                //    human.points.Add(point);
                                                                //}
                                                                NewHumans_2.Add(humansContinue);  //Humans[an] -》 human 或者采用从存储结构中直接添加的方式，即将该for循环所包括的信息全部替换为：humansaux.Add(Humans[an]);  Debug
                                                                break;  //退出，不执行后续的无效比较以避免ani >= HumanAux[ha].Count()造成再次添加数据
                                                            }
                                                            else if ( (AllHumans[ah][an].id == HumanAux[ha][ani].id) && (AllHumans[ah][an].points.Count() <= HumanAux[ha][ani].points.Count()) )  //当前的点数小于等于辅助存储结构中的点数，break跳出，避免ani >= HumanAux[ha].Count()造成再次添加数据
                                                            {
                                                                break;  //跳出比较当前的下一个行人的信息，同时避免满足其后的if(ani >= HumanAux[ha].Count())条件造成多添加行人信息
                                                            }
                                                        }
                                                        if(ani >= HumanAux[ha].Count())
                                                        {
                                                            humansContinue.camid = AllHumans[ah][an].camid;
                                                            humansContinue.direction = AllHumans[ah][an].direction;
                                                            humansContinue.flag = AllHumans[ah][an].flag;
                                                            humansContinue.id = AllHumans[ah][an].id;
                                                            humansContinue.speed = AllHumans[ah][an].speed;
                                                            humansContinue.points = AllHumans[ah][an].points;
                                                            //for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                                            //{
                                                            //    point.x = Humans[an].points[pts].x;
                                                            //    point.y = Humans[an].points[pts].y;
                                                            //    human.points.Add(point);
                                                            //}
                                                            NewHumans_2.Add(humansContinue);  //Humans[an] -》 human 或者采用从存储结构中直接添加的方式，即将该for循环所包括的信息全部替换为：humansaux.Add(Humans[an]);  Debug
                                                        }
                                                    }
                                                    AllHumans[ah].Clear();
                                                    AllHumans[ah] = NewHumans_2;  //跟新当前摄像头的行人信息，去除已经消失行人信息，保留新增的行人信息
                                                    HumanAux[ha].Clear();
                                                    HumanAux[ha] = NewHumans_2;  //辅助数据结构进行数据更新替换
                                                    breakFlag = true;
                                                }
                                                else
                                                {
                                                    break;  //camid号码不匹配，不是同一摄像头，直接跳出，不执行后续无意义的匹配比较循环
                                                }
                                            }
                                        }
                                        if (ha >= HumanAux.Count()) //对HumanAux是新摄像头，此前未添加过
                                        {
                                            alterNumber = AllHumans[ah].Count();//新摄像头首次添加到HumansAux中，辅助存储结构中保存的上一个指定统计时间间隔内的行人信息个数为0个
                                            List<Human> NewHumansAux = new List<Human>();  //针对新摄像头
                                            for (int an = 0; an < alterNumber; an++)  //将某个摄像头内部的行人信息全部提取出来，通过humansaux放入到HumansAux结构中
                                            {
                                                humansContinue.camid = AllHumans[ah][an].camid;
                                                humansContinue.direction = AllHumans[ah][an].direction;
                                                humansContinue.flag = AllHumans[ah][an].flag;
                                                humansContinue.id = AllHumans[ah][an].id;
                                                humansContinue.speed = AllHumans[ah][an].speed;
                                                humansContinue.points = AllHumans[ah][an].points;
                                                //for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                                //{
                                                //    point.x = Humans[an].points[pts].x;
                                                //    point.y = Humans[an].points[pts].y;
                                                //    human.points.Add(point);
                                                //}
                                                NewHumansAux.Add(humansContinue);  //Humans[an] -》 human 或者采用从存储结构中直接添加的方式，即将该for循环所包括的信息全部替换为：humansaux.Add(Humans[an]);  Debug
                                            }
                                            HumanAux.Add(NewHumansAux);
                                            breakFlag = true;
                                        }
                                    }
                                    else  //首次向辅助存储结构中添加信息
                                    {
                                        alterNumber = AllHumans[ah].Count();//首次添加，辅助存储结构中保存的上一个指定统计时间间隔内的行人信息个数为0个
                                        List<Human> FirstHumansAux = new List<Human>();  //首次存放的数据
                                        for (int an = 0; an < alterNumber; an++)  //将某个摄像头内部的行人信息全部提取出来，通过humansaux放入到HumansAux结构中
                                        {
                                            humansContinue.camid = AllHumans[ah][an].camid;
                                            humansContinue.direction = AllHumans[ah][an].direction;
                                            humansContinue.flag = AllHumans[ah][an].flag;
                                            humansContinue.id = AllHumans[ah][an].id;
                                            humansContinue.speed = AllHumans[ah][an].speed;
                                            humansContinue.points = AllHumans[ah][an].points;
                                            //for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                            //{
                                            //    point.x = Humans[an].points[pts].x;
                                            //    point.y = Humans[an].points[pts].y;
                                            //    human.points.Add(point);
                                            //}
                                            FirstHumansAux.Add(humansContinue);  //Humans[an] -》 human 或者采用从存储结构中直接添加的方式，即将该for循环所包括的信息全部替换为：humansaux.Add(Humans[an]);  Debug
                                        }
                                        HumanAux.Add(FirstHumansAux);
                                        breakFlag = true;
                                    }
                                }
                                else  //与当前camid所对应的摄像头编号不匹配，直接跳出
                                {
                                    break;
                                }
                            }
                            if (ahi >= AllHumans[ah].Count())
                            {
                                //查询存储的摄像头数据未找到camid与当前摄像机编号相同的设备
                            }
                        }
                        else //当前摄像头的数据为空，但是不确定是否为所要查找的摄像头
                        {
                            //Debug 当前摄像头无数据；赋空的值可能会增加容器中的数量，移除会改变此处参与循环的个体数据
                        }
                    }
                    if (ah > AllHumans.Count())
                    {
                        Console.WriteLine("This step Won't be Executed!");
                    }
                    //HumansContinue = AllHumansNull[0];//空指向
                    //HumansContinue.Clear();
                    //humansaux = AllHumansNull[0];//空指向
                    //humansaux.Clear();
                    //humanscontinue = humanzero;                    
                }

                //5.6数据清零  包含所有摄像头人员数量统计信息中的增量式数据的清零
                AllDensity.Clear();
                AllSpeedData.Clear();
                //if(AllHumansNull.Count() > 0)
                //{
                //    AllHumansNull.RemoveRange(1, (AllHumansNull.Count() - 1) >= 0 ? (AllHumansNull.Count() - 1) : 0);//移除下标为0之后的所有数据
                //}
                //if(AllHumansNull.Count() > 0)
                //{
                //    AllHumansNull[0].Clear();//对应上面未被移除的下标为0的数据
                //}
                //if (AllSpeedDataNull.Count() > 0)
                //{
                //    AllSpeedDataNull.RemoveRange(1, (AllSpeedDataNull.Count() - 1) >= 0 ? (AllSpeedDataNull.Count() - 1) : 0);
                //}
                //if(AllSpeedDataNull.Count() > 0)
                //{
                //    AllSpeedDataNull[0].Clear();
                //}

                //5.7零点总量式数据统计清零
                frameNum++;  //一天内的总帧数增加
                censusFrameNum++;  //统计计算帧数增加                
                if (timeRemapFlag)                                                   //允许置位
                {
                    if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 1)            //判断时间是否介于00：00分到1：00分，到达则重置计数帧数，时间标志位置位为1，在规定的时间间隔内不能再次重置
                    {
                        frameNum = 0;
                        timeRemapFlag = false;                                       //不允许置位
                        HumansCensus humanscensus = new HumansCensus();
                        for (int hcs = 0; hcs < AllHumansCensus.Count(); hcs++)       //总量式数据清零
                        {
                            humanscensus.camid = AllHumansCensus[hcs].camid;
                            humanscensus.totalHumanVolume = 0;
                            humanscensus.upHumanVolume = 0;
                            humanscensus.downHumanVolume = 0;
                            humanscensus.deltHuman = AllHumansCensus[hcs].deltHuman;  //增量式数据在5.1数量统计完毕并将相应的数值赋值给输出数据单元后已经清空，此时等价于0
                            humanscensus.deltUpHuman = AllHumansCensus[hcs].deltUpHuman;
                            humanscensus.deltDownHuman = AllHumansCensus[hcs].deltDownHuman;
                            AllHumansCensus[hcs] = humanscensus; //更新替换
                        }
                    }
                }
                if (DateTime.Now.Hour > 2)                                           //判断时间是否已经过2：00分，到达则重置计数帧数，时间标志位置位为0，可以在再次到达零点时进行清零置位
                {
                    timeRemapFlag = true;                                            //允许置位
                }
                return true;
            }

            frameNum++;
            censusFrameNum++;
            if (timeRemapFlag)                                                       //允许置位
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 1)                //判断时间是否介于00：00分到1：00分，到达则重置计数帧数，时间标志位置位为1，在规定的时间间隔内不能再次重置
                {
                    frameNum = 0;
                    timeRemapFlag = false;                                           //不允许置位
                    HumansCensus humanscensus = new HumansCensus();
                    for (int hcs = 0; hcs < AllHumansCensus.Count(); hcs++)          //总量式数据清零
                    {
                        humanscensus.camid = AllHumansCensus[hcs].camid;
                        humanscensus.totalHumanVolume = 0;
                        humanscensus.upHumanVolume = 0;
                        humanscensus.downHumanVolume = 0;
                        humanscensus.deltHuman = AllHumansCensus[hcs].deltHuman;
                        humanscensus.deltUpHuman = AllHumansCensus[hcs].deltUpHuman;
                        humanscensus.deltDownHuman = AllHumansCensus[hcs].deltDownHuman;
                        AllHumansCensus[hcs] = humanscensus; //更新替换
                    }
                }
            }
            if (DateTime.Now.Hour > 2)                                               //判断时间是否已经过2：00分，到达则重置计数帧数，时间标志位置位为0，可以在再次到达零点时进行清零置位
            {
                timeRemapFlag = true;                                                //允许置位
            }
            return false;
        }
    }
}
