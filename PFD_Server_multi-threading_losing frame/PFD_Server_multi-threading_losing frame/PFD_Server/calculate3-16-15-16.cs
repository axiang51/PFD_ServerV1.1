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
////////3-16-8：58
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

    //public class OutputData                    //输出数据类，到达指定统计时间间隔后输出各种类型的数据，注意速度参考标志位
    //{
    //    public bool AverageSpeedFlag;          //总平均速度变量标志位，真则区域平均速度不为空，假则区域平均速度为空，空表示无人而非速度为0
    //    public bool AverageUpSpeedFlag;        //上行平均速度标志位
    //    public bool AverageDownSpeedFlag;      //下行平均速度标志位
    //    public double AverageSpeed;            //区域行人平均速度
    //    public double AverageUpSpeed;
    //    public double AverageDownSfpeed;
    //    public int TotalHumanVolume;           //总量式区域所有人员数量
    //    public int UpHumanVolume;              //总量式区域上行人员数量
    //    public int DownHumanVolume;            //总量式区域下行人员数量
    //    public int DeltHuman;                  //增量式区域所有人员数量
    //    public int DeltUpHuman;                //增量式区域上行人员数量
    //    public int DeltDownHuman;              //增量式区域下行人员数量
    //    public int AverageDensity;             //平均密度
    //}

    public class Parameters
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //需要从数据库读取相关计算参数以及相应的路段信息
        public string CamID = "1";
        public string Road = "";
        public string StreamAddress = "";
        public int VideoWidth = 1280;
        public int VideoHeight = 720;
        public int FrameRate = 25;
        public int CompressVideoWidth = 640;
        public int CompressVideoHeight = 360;
        public int ROIX = 0;
        public int ROIY = 0;
        public int ROIWidth = 640;
        public int ROIHeight = 360;
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
        public static int frameHeight = 360;
        public static int splitLine = 0;                                                 //分隔线  2017年9月13日15:25:04用于分隔检测区域
        public static int dataStatisticFrame = 125;                                      //动态统计时间调节以帧为单位 2017年9月13日15:25:11
        public static int roix = 0;
        public static int roiy = 0;
        public static int roiWidth = 640;
        public static int roiHeight = 360;

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

        List<Human> Humans = new List<Human>();//存储人员信息的容器
        List<List<Human>> HumanAux = new List<List<Human>>();//用于前后两个统计时间间隔内数据的更替

        List<List<Human>> AllHumans = new List<List<Human>>();//存放所有摄像头检测出的人员数据
        List<List<Human>> AllHumansNull = new List<List<Human>>(1);//空指向
        List<List<Speed>> AllSpeedData = new List<List<Speed>>();//存放所有摄像头速度数据
        List<List<Speed>> AllSpeedDataNull = new List<List<Speed>>(1);//空指向
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
            List<Speed> SpeedData = new List<Speed>();//用于行人速度等数据保存
            Density density = new Density();//单路摄像头密度
            Speed speed = new Speed();
            Human human = new Human();
            Point point = new Point();
            Point point2 = new Point();

            human.camid = -1;
            human.id = -1;  //数据初始化，使其不为空
            human.flag = -1;
            human.direction = -1;
            human.speed = -1;
            human.points = new List<Point>();  //存放行人轨迹
            List<Human> HumansNull = new List<Human>();
            HumansNull.Add(human);
            AllHumansNull.Add(HumansNull);  //空指向无效时取消注释
            for (int i = 0; i < sPipeInfo.iVideoCount; i++)//Debug 第i个摄像头  修改为摄像头的实际数目IVideoCount
            {
                //密度
                List<int> densityNumber = new List<int>();
                densityNumber.Add(sPipeInfo.szVideoInfo[i].iObjCount);
                density.camid = sPipeInfo.szVideoInfo[i].iVideoID;   //Debug  修改为摄像头的实际int类型标号
                density.density = densityNumber;
                if (AllDensity.Count() > 0)
                {
                    int adty = 0;
                    for (; adty < AllDensity.Count(); adty++)
                    {
                        if (sPipeInfo.szVideoInfo[i].iVideoID == AllDensity[adty].camid)  //Debug sPipeInfo.szVideoInfo[i].iVideoID
                        {
                            AllDensity[adty].density.Add(sPipeInfo.szVideoInfo[i].iObjCount);
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
                    human.camid = sPipeInfo.szVideoInfo[i].iVideoID;    //摄像头编号
                    human.id = sPipeInfo.szVideoInfo[i].szObjInfo[j].ID;
                    if (AllHumans.Count() > 0)  //存放所有人员的容器不为空
                    {
                        int ah = 0;
                        for (; ah < AllHumans.Count(); ah++)  //先遍历寻找匹配的第i路摄像头能否匹配
                        {
                            Humans = AllHumans[ah];
                            if (sPipeInfo.szVideoInfo[i].iVideoID == Humans[0].camid)  //Debug 可能存在 Hunmans[0]不存在的情况；仅判断摄像头号码是否匹配，等价于i == AllHumans[ah][0].camid     sPipeInfo.szVideoInfo[i].iVideoID
                            {
                                Humans = AllHumans[ah];    //令Humans指向AllHumans中的第i个摄像头所对应的行人信息，视作直接对AllHumans[i]进行操作，其后操作类似
                                int hi = 0;
                                for (; hi < Humans.Count(); hi++)  //遍历容器寻找ID号相同的行人
                                {
                                    if (Humans[hi].id == sPipeInfo.szVideoInfo[i].szObjInfo[j].ID)  //匹配成功，在该轨迹之后添加轨迹点
                                    {
                                        point.x = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_x;
                                        point.y = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_y;
                                        Humans[hi].points.Add(point);  //Debug 添加当前行人轨迹点

                                        //1.轨迹点接续存储
                                        human.flag = Humans[hi].flag;   //由行人信息存储结构中取出当前人的信息
                                        human.direction = Humans[hi].direction;
                                        human.speed = Humans[hi].speed;
                                        for (int pi = 0; pi < Humans[hi].points.Count(); pi++)
                                        {
                                            point.x = Humans[hi].points[pi].x;
                                            point.y = Humans[hi].points[pi].y;
                                            human.points.Add(point);
                                        }

                                        //2.方向确认
                                        //位于此处原因：满足指定点数要求的轨迹可按照流程执行到此处，达到指定的点数需要多帧数据的累积
                                        if (Humans[hi].points.Count() >= 10)  //Debug  允许进行方向确认的轨迹点数需要的最小数值
                                        {
                                            if (-1 == Humans[hi].direction)  //表明此前未进行过方向判别，首次执行
                                            {
                                                if (Humans[hi].points[0].y - Humans[hi].points[Humans[hi].points.Count()].y >= 0)
                                                {
                                                    human.direction = 1;     //上行
                                                }
                                                else
                                                {
                                                    human.direction = 2;     //下行
                                                }
                                            }
                                            Humans[hi] = human;              //当前行人信息更新  direction
                                        }

                                        //3.人数统计
                                        if (Humans[hi].points.Count() >= 10)  //Debug 进行人数统计的条件
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

                                                human.flag = 1;
                                                Humans[hi] = human;          //当前行人信息更新  flag

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
                                        speed.camid = -1;  //数据初始化
                                        speed.direction = -1;
                                        speed.id = -1;
                                        speed.speed = -1;
                                        SpeedData.Add(speed);
                                        List<Speed> SpeedNull = new List<Speed>();
                                        SpeedNull.Add(speed);
                                        AllSpeedDataNull.Add(SpeedNull);
                                        if (Humans[hi].points.Count() >= 20)  //Debug 速度计算条件
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

                                                        for (int si = 0; si < Humans[hi].points.Count() - 1; si++)//计算该轨迹平均速度，两点构成的速度组数
                                                        {
                                                            if ((Humans[hi].points.Count() - 1) >= 16)  //Debug
                                                            {
                                                                //该跳过语句的判断条件是方法1速度筛选条件的非形式
                                                                if (((Humans[hi].points.Count() - 1) / 2 - 4 > si) || ((Humans[hi].points.Count() - 1) / 2 + 4 < si))  //如果组数大于16组，跳过未被选中的组
                                                                {
                                                                    if (si != (Humans[hi].points.Count() - 2))
                                                                    {
                                                                        continue;
                                                                    }

                                                                }
                                                            }

                                                            point = Humans[hi].points[si];                            //提取轨迹点1进行计算
                                                            point2 = Humans[hi].points[si + 1];                       //提取轨迹点2进行计算
                                                            deltDis = disCalculate.Dis(point, point2);                //距离计算                                                    
                                                            secHumanSpeed = deltDis / Parameters.timeDif;             //通过两轨迹点的速度m/s
                                                            if (si != (Humans[hi].points.Count() - 2))
                                                            {
                                                                if ((Humans[hi].points.Count() - 1) >= 16)            //Debug 筛选速度，速度组数大于等于阈值，选取速度不为0的组数（轨迹点数 - 1）/ 2组计算筛选速度
                                                                {
                                                                    if (((Humans[hi].points.Count() - 1) / 2 - 4 <= si) && ((Humans[hi].points.Count() - 1) / 2 + 4 >= si))  //动态筛选轨迹中间一半的轨迹进行筛选速度的计算
                                                                    {
                                                                        if (secHumanSpeed != 0)                       //筛选速度中每组的速度不为零
                                                                        {
                                                                            selectSpeedNumber++;                      //筛选速度不为0组数加1
                                                                            selectSumSpeed += secHumanSpeed;          //筛选速度的速度总和
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if ((Humans[hi].points.Count() - 1) < 16)                 //Debug 不足组数按照常规方法求基础平均速度
                                                            {
                                                                if (secHumanSpeed != 0)
                                                                {
                                                                    groupSpeedNumber++;                               //方法1求平均速度不为0的组数加1
                                                                    sumHumanSpeed += secHumanSpeed;                   //该轨迹当前帧速度总和
                                                                }
                                                            }
                                                        }

                                                        if ((Humans[hi].points.Count() - 1) >= 16)                    //Debug 满足组数要求，筛选速度替换基础速度
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
                                                        Humans[hi] = human;                                           //当前行人信息更新  speed
                                                        speed.camid = sPipeInfo.szVideoInfo[i].iVideoID;  //Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                        speed.id = human.id;
                                                        speed.direction = human.direction;
                                                        speed.speed = humanMeanSpeed;

                                                        if (AllSpeedData.Count > 0)
                                                        {
                                                            int asd = 0;
                                                            for (; asd < AllSpeedData.Count; asd++)
                                                            {
                                                                SpeedData = AllSpeedData[asd];
                                                                int asdi = 0;
                                                                if (sPipeInfo.szVideoInfo[i].iVideoID == speed.camid)  //同一个摄像头  Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                                {
                                                                    for (; asdi < SpeedData.Count(); asdi++)  //某个摄像头信息匹配
                                                                    {
                                                                        if (speed.id == SpeedData[asdi].id)
                                                                        {
                                                                            SpeedData[asdi] = speed;
                                                                            SpeedData = AllSpeedDataNull[0];//空指向
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (asdi < SpeedData.Count()) //已经匹配成功，直接跳过
                                                                    {
                                                                        break;
                                                                    }
                                                                    else  //未匹配成功，但是为同一个摄像头内新增加的数据
                                                                    {
                                                                        SpeedData.Add(speed);
                                                                        SpeedData = AllSpeedDataNull[0];//空指向
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //
                                                                }
                                                            }
                                                            if (asd >= AllSpeedData.Count())  //新摄像头数据
                                                            {
                                                                SpeedData = AllSpeedDataNull[0];  //空指向
                                                                SpeedData.Clear();
                                                                SpeedData.Add(speed);
                                                                AllSpeedData.Add(SpeedData);
                                                                SpeedData = AllSpeedDataNull[0];//空指向
                                                            }
                                                        }
                                                        else  //首次存放某路摄像头的数据信息
                                                        {
                                                            SpeedData.Add(speed);                                         //将该人信息放入速度容器
                                                            AllSpeedData.Add(SpeedData);
                                                            SpeedData = AllSpeedDataNull[0];  //空指向
                                                        }
                                                    }
                                                    break;

                                                case 2:                                                               //按照首尾轨迹点构成的组进行个体速度计算
                                                    {
                                                        int pointNum = 0;                                             //轨迹点数
                                                        double deltDis = 0;
                                                        double humanMeanSpeed = 0;                                    //该轨迹平均速度                                                   

                                                        if (Humans[hi].points.Count() >= 17)                          //轨迹点大于等于17个（对应法1的16组），筛选中间的轨迹点
                                                        {
                                                            point = Humans[hi].points[(Humans[hi].points.Count() / 2) - (Humans[hi].points.Count() / 4)];  //提取轨迹点1进行计算
                                                            point2 = Humans[hi].points[(Humans[hi].points.Count() / 2) + (Humans[hi].points.Count() / 4)]; //提取轨迹点2进行计算
                                                            pointNum = 2 * (Humans[hi].points.Count() / 4) + 1;    //实际的点数
                                                        }
                                                        else                                                       //点数低于17个选择首尾点
                                                        {
                                                            point = Humans[hi].points[0];                             //轨迹的第一个轨迹点
                                                            point2 = Humans[hi].points[Humans[hi].points.Count() - 1];//轨迹的最后一个轨迹点
                                                            pointNum = Humans[hi].points.Count();                  //实际的点数
                                                        }
                                                        deltDis = disCalculate.Dis(point, point2);                 //距离计算
                                                        humanMeanSpeed = deltDis / (Parameters.timeDif * (pointNum - 1));  //通过两轨迹点的速度m/s
                                                        human.speed = humanMeanSpeed;
                                                        Humans[hi] = human;                                        //当前行人信息更新  speed
                                                        speed.id = human.id;
                                                        speed.direction = human.direction;
                                                        speed.speed = humanMeanSpeed;

                                                        if (AllSpeedData.Count > 0)
                                                        {
                                                            int asd = 0;
                                                            for (; asd < AllSpeedData.Count; asd++)
                                                            {
                                                                SpeedData = AllSpeedData[asd];
                                                                int asdi = 0;
                                                                if (sPipeInfo.szVideoInfo[i].iVideoID == speed.camid)  //同一个摄像头  Debug    sPipeInfo.szVideoInfo[i].iVideoID
                                                                {
                                                                    for (; asdi < SpeedData.Count(); asdi++)  //某个摄像头信息匹配
                                                                    {
                                                                        if (speed.id == SpeedData[asdi].id)
                                                                        {
                                                                            SpeedData[asdi] = speed;
                                                                            SpeedData = AllSpeedDataNull[0];//空指向
                                                                            break;
                                                                        }
                                                                    }
                                                                    if (asdi < SpeedData.Count()) //已经匹配成功，直接跳过
                                                                    {
                                                                        break;
                                                                    }
                                                                    else  //未匹配成功，但是为同一个摄像头内新增加的数据
                                                                    {
                                                                        SpeedData.Add(speed);
                                                                        SpeedData = AllSpeedDataNull[0];//空指向
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //
                                                                }
                                                            }
                                                            if (asd >= AllSpeedData.Count())  //新摄像头数据
                                                            {
                                                                SpeedData = AllSpeedDataNull[0];  //空指向
                                                                SpeedData.Clear();
                                                                SpeedData.Add(speed);
                                                                AllSpeedData.Add(SpeedData);
                                                                SpeedData = AllSpeedDataNull[0];//空指向
                                                            }
                                                        }
                                                        else  //首次存放某路摄像头的数据信息
                                                        {
                                                            SpeedData.Add(speed);                                         //将该人信息放入速度容器
                                                            AllSpeedData.Add(SpeedData);
                                                            SpeedData = AllSpeedDataNull[0];  //空指向
                                                        }
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                        break;                         //匹配成功后，轨迹点添加执行完毕，退出不执行后续 循环

                                        //5.各帧数据密度统计


                                    }
                                }
                                if (hi >= Humans.Count())  //在存放第i路摄像头所对应的容器内无轨迹号相同的，添加到当前存储行人信息的容器中
                                {
                                    Humans.Add(human);
                                    Humans = AllHumansNull[0];//空指向
                                }
                            }
                        }
                        if (ah >= AllHumans.Count())  //遍历存储结构，未能匹配，视作新摄像头
                        {
                            Humans = AllHumansNull[0];//空指向
                            Humans.Clear();
                            Humans.Add(human);
                            AllHumans.Add(Humans);
                        }
                    }
                    else  //首次执行，存放所有人员信息的容器为空
                    {
                        Humans.Add(human);
                        AllHumans.Add(Humans);
                        Humans = AllHumansNull[0];//空指向
                    }
                }
            }

            //5.数据统计  //推送数据为当前统计时间内的数据
            if (Parameters.dataStatisticFrame == censusFrameNum)
            {
                censusFrameNum = 0;
                OutputData outputData = new OutputData();//存放单个摄像头的输出数据
                int camNumber = 0;  //摄像头编号
                for (; camNumber < sPipeInfo.iVideoCount; camNumber++)  //Debug  sPipeInfo.szVideoInfo.Length -》 sPipeInfo.iVideoCount
                {
                    //5.1人数统计
                    if (AllHumansCensus.Count() > 0)  //存放所有人员信息容器非空，以该程序中的camID编号为序，由此向下依次计算各个摄像头的相关数据，数据计算完毕后送至该摄像头的所对应的数据输出部分，随后执行下一个摄像头数据的计算
                    {
                        int ahc = 0;
                        for (; ahc < AllHumansCensus.Count(); ahc++)
                        {
                            if (camNumber == AllHumansCensus[ahc].camid)//找到编号为camNumber的摄像头，取人数统计数据
                            {
                                outputData.CamID = camNumber;
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
                            outputData.CamID = camNumber;
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
                        outputData.CamID = camNumber;
                        outputData.TotalHumanVolume = 0;  //Debug
                        outputData.UpHumanVolume = 0;
                        outputData.DownHumanVolume = 0;
                        outputData.DeltHuman = 0;
                        outputData.DeltUpHuman = 0;
                        outputData.DeltDownHuman = 0;
                    }

                    //5.2速度统计
                    List<Speed> curSpeed = new List<Speed>();
                    if (AllSpeedData.Count() > 0)
                    {
                        int aspd = 0;
                        for (; aspd < AllSpeedData.Count(); aspd++)
                        {
                            int aspdi = 0;
                            for (; aspdi < AllSpeedData[aspd].Count(); aspdi++)
                            {
                                if (camNumber == AllSpeedData[aspd][aspdi].camid)  //匹配，速度存储为同摄像头存储
                                {
                                    curSpeed = AllSpeedData[aspd];
                                    SpeedData = curSpeed;
                                    if (SpeedData.Count() > 0)
                                    {
                                        double totalspeed = 0;
                                        double upspeed = 0;
                                        double downspeed = 0;
                                        int total = 0;
                                        int up = 0;
                                        int down = 0;

                                        for (int spd = 0; spd < SpeedData.Count(); spd++)
                                        {
                                            if (1 == SpeedData[spd].direction)        //上行
                                            {
                                                if (0 < SpeedData[spd].speed && SpeedData[spd].speed < 3)
                                                {
                                                    total++;
                                                    up++;
                                                    totalspeed += SpeedData[spd].speed;
                                                    upspeed += SpeedData[spd].speed;
                                                }
                                            }
                                            else                                     //下行
                                            {
                                                if (0 < SpeedData[spd].speed && SpeedData[spd].speed < 3)
                                                {
                                                    total++;
                                                    down++;
                                                    totalspeed += SpeedData[spd].speed;
                                                    downspeed += SpeedData[spd].speed;
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
                                    SpeedData = AllSpeedDataNull[0];//空指向
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
                            outputData.CamID = camNumber;//存在重复赋值，但是可以避免重复添加摄像头
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
                        outputData.CamID = camNumber;//存在重复赋值，但是可以避免重复添加摄像头
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
                            if (camNumber == AllDensity[adst].camid) //摄像头配备成功
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
                        if (adst > AllDensity.Count())
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
                            if (camNumber == AllOutputData[aod].CamID)  //匹配，更新其中的流量统计数据
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
                    int ah = 0;
                    int alterNumber = 0;//设置在两个统计时间间隔内需要更新的数据
                    bool breakFlag = false;  //结合判断用于跳出循环
                    List<Human> humansaux = new List<Human>();
                    for (; ah < AllHumans.Count(); ah++)
                    {
                        if (breakFlag)
                        {
                            break;
                        }
                        Humans = AllHumans[ah];
                        if (Humans.Count() > 0)
                        {
                            int ahi = 0;
                            for (; ahi < Humans.Count(); ahi++)
                            {
                                if (breakFlag)  //跳出
                                {
                                    break;
                                }
                                if (camNumber == Humans[ahi].camid)
                                {
                                    //至此，在AllHumans中找到需要进行数据更替的摄像头
                                    if (HumanAux.Count() > 0)
                                    {
                                        //在HumansAux中寻找与camNumber标号相同的摄像头所对应的数据，若找到相同的则将内部保存的数据量
                                        int ha = 0;
                                        for (; ha < HumanAux.Count(); ha++)
                                        {
                                            if (breakFlag)
                                            {
                                                break;
                                            }
                                            for (int hai = 0; hai < HumanAux[ha].Count(); hai++)
                                            {
                                                if (breakFlag)
                                                {
                                                    break;
                                                }
                                                if (camNumber == HumanAux[ha][hai].camid)  //摄像头匹配成功
                                                {
                                                    //获取该摄像头在HumanAux中存储的人数个数并清空该摄像头的数据，然后重新放入（当前统计时间间隔所有数据 - 历史统计时间间隔所有数据）
                                                    alterNumber = HumanAux[ha].Count(); //该摄像头在历史统计时间间隔保存的数据（行人信息）个数
                                                    HumanAux[ha].Clear(); //该摄像头在历史统计时间间隔内的数据清空
                                                    Humans.RemoveRange(0, alterNumber);  //删除该摄像头在当前统计时间间隔内所包含的历史统计时间间隔内的数据

                                                    for (int an = 0; an < Humans.Count(); an++)  //将某个摄像头内部的行人信息全部提取出来，通过humansaux更新替换到HumansAux结构中
                                                    {
                                                        human.camid = Humans[an].camid;
                                                        human.direction = Humans[an].direction;
                                                        human.flag = Humans[an].flag;
                                                        human.id = Humans[an].id;
                                                        human.speed = Humans[an].speed;
                                                        for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                                        {
                                                            point.x = Humans[an].points[pts].x;
                                                            point.y = Humans[an].points[pts].y;
                                                            human.points.Add(point);
                                                        }
                                                        humansaux.Add(Humans[an]);
                                                    }
                                                    HumanAux[ha] = humansaux;  //更新替换
                                                    breakFlag = true;
                                                }
                                            }
                                        }
                                        if (ha >= HumanAux.Count()) //对HumanAux是新摄像头，此前未添加过
                                        {
                                            alterNumber = 0;//新摄像头首次添加到HumansAux中，辅助存储结构中保存的上一个指定统计时间间隔内的行人信息个数为0个
                                            for (int an = 0; an < Humans.Count(); an++)  //将某个摄像头内部的行人信息全部提取出来，通过humansaux放入到HumansAux结构中
                                            {
                                                human.camid = Humans[an].camid;
                                                human.direction = Humans[an].direction;
                                                human.flag = Humans[an].flag;
                                                human.id = Humans[an].id;
                                                human.speed = Humans[an].speed;
                                                for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                                {
                                                    point.x = Humans[an].points[pts].x;
                                                    point.y = Humans[an].points[pts].y;
                                                    human.points.Add(point);
                                                }
                                                humansaux.Add(Humans[an]);
                                            }
                                            HumanAux.Add(humansaux);
                                            breakFlag = true;
                                        }
                                    }
                                    else  //首次向辅助存储结构中添加信息
                                    {
                                        alterNumber = 0;//首次添加，辅助存储结构中保存的上一个指定统计时间间隔内的行人信息个数为0个
                                        for (int an = 0; an < Humans.Count(); an++)  //将某个摄像头内部的行人信息全部提取出来，通过humansaux放入到HumansAux结构中
                                        {
                                            human.camid = Humans[an].camid;
                                            human.direction = Humans[an].direction;
                                            human.flag = Humans[an].flag;
                                            human.id = Humans[an].id;
                                            human.speed = Humans[an].speed;
                                            for (int pts = 0; pts < Humans[an].points.Count(); pts++)
                                            {
                                                point.x = Humans[an].points[pts].x;
                                                point.y = Humans[an].points[pts].y;
                                                human.points.Add(point);
                                            }
                                            humansaux.Add(Humans[an]);
                                        }
                                        HumanAux.Add(humansaux);
                                        breakFlag = true;
                                    }
                                }
                            }
                            if (ahi >= Humans.Count())
                            {
                                //
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
                    Humans = AllHumansNull[0];//空指向

                }

                //5.6数据清零  包含所有摄像头人员数量统计信息中的增量式数据的清零
                AllDensity.Clear();
                AllHumansNull.RemoveRange(1, (AllHumansNull.Count() - 1) >= 0 ? (AllHumansNull.Count() - 1) : 0);//移除下标为0之后的所有数据
                AllHumansNull[0].Clear();//对应上面未被移除的下标为0的数据
                AllSpeedDataNull.RemoveRange(1, (AllSpeedDataNull.Count() - 1) >= 0 ? (AllSpeedDataNull.Count() - 1) : 0);
                AllSpeedDataNull[0].Clear();

                frameNum++;
                censusFrameNum++;

                //5.7零点总量式数据统计清零
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
                            humanscensus.deltHuman = AllHumansCensus[hcs].deltHuman;
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
