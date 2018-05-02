using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Collections;

namespace HST_Server
{
    public delegate void TimesUp(DataRec dr);
    public delegate void Method(DataRec dr);
    
    public class DataRec
    {
        public byte[] buffer = new byte[20480];
        public uint buffer_len = 0;
        public uint out_len = 0;
        public bool rResult;
        public IntPtr hPipe;
        public bool PipeFlag = true;
        public bool missingflag = false;
        public int CamID;
        public DataRec(int _CamID)
        {
            this.CamID = _CamID;
            buffer_len = (uint)buffer.Length;
        }
    }

    public class FuncTimeout
    {
        public ManualResetEvent manu = new ManualResetEvent(false);
        public bool isGetSignal;
        public int timeout;
        public Method FunctionNeedRun;
        public int CamID;
        public FuncTimeout(Method _action, int _timeout, int _camID)
        {
            FunctionNeedRun = _action;
            timeout = _timeout;
            CamID = _camID;
        }
        public void MyAsyncCallback(IAsyncResult ar)
        {

        }
        public void doAction(DataRec dr)
        {
            Method WhatTodo = CombineActionAndManuset;

            var r = WhatTodo.BeginInvoke(dr, MyAsyncCallback, null);
            isGetSignal = manu.WaitOne(timeout);

            if (isGetSignal)
            {
                return;

            }
            else
            {
                dr.missingflag = true;
                Process[] p_arry = Process.GetProcesses();//得到系统所有进程
                for (int i = 0; i < p_arry.Length; i++)//遍历每个进程
                {
                    if (p_arry[i].ProcessName == "CarDetect" + CamID)//发现进程
                    {
                        p_arry[i].Kill();//就结束它。
                        break;
                    }
                }

                RebootPipe(dr);
            }
        }
        public void RebootPipe(DataRec dr)
        {
            Process pr = new Process();//启动管道
            pr.StartInfo.WorkingDirectory = @"..\..\..\..\..\..\C++\" + CamID.ToString() + @"\Release";
            pr.StartInfo.FileName = "CarDetect" + CamID.ToString() + ".exe";
            pr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            pr.Start();

            TimesUp tu = new TimesUp(reboot);
            tu.BeginInvoke(dr, MyAsyncCallback, null);

        }
        private void reboot(DataRec dr)
        {
            dr.PipeFlag = false;
            Thread.Sleep(2000);
            dr.PipeFlag = true;
        }

        public void CombineActionAndManuset(DataRec dr)
        {
            FunctionNeedRun(dr);
            manu.Set();
        }
    }

    class run
    {
        bool simulation_flag = false;//模拟计算及参数为true，实际计算及参数为false
        bool i64time_flag = true;//同步机制标志位
        ParamsCalculate pcalculate = new ParamsCalculate();
        MysqlPersistance mp = new MysqlPersistance();
        Param resultParam = new Param();
        Int32[] data1 = new int[2000];
        public Int32[] data;
        //Calculate cal = new Calculate();
        public int lastCarId = 999;
        List<List<HST_Result>> tracker = new List<List<HST_Result>>();//存储数组
        HST_Result result = new HST_Result();//存储数组
        List<int> List_ID = new List<int>();
        List<HST_Result> listinfo = new List<HST_Result>();
        List<CollectData> collectData = new List<CollectData>();
        statisticalData statistical = new statisticalData();
        List<long> last_i64time = new List<long>();
        List<int> last_detetime = new List<int>();
        List<int> last_detetime_min = new List<int>();
        ///索引标志位
        List<int> last_onehr_index_CPos = new List<int>();
        List<int> last_onehr_index_CNeg = new List<int>();
        List<int> last_onehr_index_Speed = new List<int>();
        List<int> last_onehr_index_Speed_up = new List<int>();
        List<int> last_onehr_index_Speed_down = new List<int>();
        List<int> last_onehr_index_Density = new List<int>();

        //读取管道线程
        bool m_bExitReadPipe = true;
        Object m_oReadPipeLock = new Object();
        Queue m_pipeInfoQueue = new Queue();
        int m_iMaxPipeInfoCount = 5;    //最大缓冲数量，超过此值，将丢包
        IntPtr m_hPipe;

        string str = "";

        void exep_Exited(object sender, EventArgs e) 
        { 
            Console.WriteLine(".exe运行完毕"); 
        } 
        public void runing(object param)
        {
            //try
            {
                ///初始化
                for (int n = 0; n < 20; n++)
                {
                    CollectData colle = new CollectData();
                    collectData.Add(colle);
                    last_onehr_index_CPos.Add(0);
                    last_onehr_index_CNeg.Add(0);
                    last_onehr_index_Speed.Add(0);
                    last_onehr_index_Speed_up.Add(0);
                    last_onehr_index_Speed_down.Add(0);
                    last_onehr_index_Density.Add(0);

                    last_i64time.Add(0);
                    last_detetime.Add(DateTime.Now.Second*1000+ DateTime.Now.Millisecond);
                    last_detetime_min.Add(DateTime.Now.Minute);
                }
                int last_min = DateTime.Now.Minute;
                bool calculate_flag = true;
                byte[] writebuffer = new byte[20480];
                string strPipeName = "\\\\.\\pipe\\HST_Output_Pip_Gpu_1";
                Console.WriteLine(strPipeName);

                DataRec dr = new DataRec(Convert.ToInt32(param));
                while (true)
                {
                    Thread.Sleep(1000);
                    if (dr.PipeFlag)
                    {
                        while (true)//连接管道
                        {
                            //dr.hPipe = PipeNative.WaitNamedPipe
                            dr.hPipe = PipeNative.CreateFile(
                                strPipeName,  //管道名称
                                FileDesiredAccess.GENERIC_READ | FileDesiredAccess.GENERIC_WRITE,  //访问模式，读模式或写模式
                                FileShareMode.Zero,  //0表示不共享，共享模式
                                IntPtr.Zero,  //一个只读字段，代表已初始化为零的指针或句柄。指向安全属性的指针
                                FileCreationDisposition.OPEN_EXISTING, //如何创建。文件必须已经存在。由设备提出要求
                                0,  //文件属性
                                0);  //用于复制文件句柄，不使用模板
                            if (dr.hPipe.ToInt32() != PipeNative.INVALID_HANDLE_VALUE) break;//PipeNative.INVALID_HANDLE_VALUE = -1.管道创建失败

                            if (PipeNative.GetLastError() != PipeNative.ERROR_PIPE_BUSY   //PipeNative.ERROR_PIPE_BUSY = 231
                                || PipeNative.WaitNamedPipe(strPipeName, 5 * 1000))       //在超时时间前管道的一个实例有效则返回非0，在超时时间内没有一个有效的实例，则返回0 
                            {
                                Console.WriteLine("无法连接管道：{0} ERROR:{1}", strPipeName, PipeNative.GetLastError());
                                continue;
                            }
                            else
                            {
                                Console.WriteLine("管道连接成功：{0}", strPipeName);
                            }
                        }

                        //start read pipe thread
                        ThreadStart childref = new ThreadStart(ReadPipeThread);
                        Thread childThread = new Thread(childref);
                        m_bExitReadPipe = false;
                        m_hPipe = dr.hPipe;
                        childThread.Start();
                        
                        while (dr.PipeFlag)
                        {
                            #region[接收数据]

                            //通过线程异步读取
                            SHstPipeInfo sPipeInfo = null;
                            int iRes = 0;

                            lock (m_oReadPipeLock)
                            {
                                if(m_pipeInfoQueue.Count > 0)
                                {
                                    sPipeInfo = (SHstPipeInfo)m_pipeInfoQueue.Dequeue();
                                }
                            }

                            if(sPipeInfo == null)
                            {
                                Thread.Sleep(10);
                                continue;
                            }
                            
                            ////////////////////////////
                            string mtime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                            Log log = new Log();
                            string msg = "ID:" + sPipeInfo.szVideoInfo[0].szObjInfo[0].ID + "读取时间：" + DateTime.Now + "毫秒:" + mtime;
                            //log.WriteLog(msg);
                            ////////////////////////////
                            //continue; //测试用，需删除 吴超

                            if (iRes == 0)
                            {
                                //todo 参数处理
                                //Console.WriteLine("\r ReadPipeInfo videocount： {0} time： {1} \t v0:{2}:{3} \t  v1:{4}:{5} \t  v2:{6}:{7} \t  v3:{8}:{9} \t"
                                //    , sPipeInfo.iVideoCount, sPipeInfo.szVideoInfo[0].i64Time
                                //    , sPipeInfo.szVideoInfo[0].iVideoID, sPipeInfo.szVideoInfo[0].iObjCount
                                //    , sPipeInfo.szVideoInfo[1].iVideoID, sPipeInfo.szVideoInfo[1].iObjCount
                                //    , sPipeInfo.szVideoInfo[2].iVideoID, sPipeInfo.szVideoInfo[2].iObjCount
                                //    , sPipeInfo.szVideoInfo[3].iVideoID, sPipeInfo.szVideoInfo[3].iObjCount
                                //    );

                                //管道收发同步机制
                                for(int l = 0;l<sPipeInfo.iVideoCount;l++)
                                {
                                    int misec = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                                    int dif_time_stamp =Convert.ToInt32( sPipeInfo.szVideoInfo[l].i64Time - last_i64time[l]);
                                    int dif_time_detect = misec - last_detetime[l];
                                    if (dif_time_detect - dif_time_stamp > 500)
                                    {
                                        i64time_flag = false;
                                        Console.WriteLine("计算时长超时，处理下一组数据");
                                        Console.WriteLine("时间间隔为:{0}", dif_time_detect - dif_time_stamp);
                                        last_i64time[l] = sPipeInfo.szVideoInfo[l].i64Time;
                                        last_detetime[l] = misec;
                                        last_detetime_min[l] = DateTime.Now.Minute;
                                        break;
                                    }
                                    else
                                    {
                                        i64time_flag = true;
                                    }
                                }
                               
                                if (i64time_flag)
                                {
                                    bool caculateflag = true; 
                                    if (simulation_flag)
                                    {
                                        //模拟计算
                                        calculate2();//模拟数据计算
                                    }
                                    else
                                    {
                                        //实际计算
                                        caculateflag = pcalculate.calculate(sPipeInfo);
                                    }
                                
                                    if (caculateflag)
                                    {
                                        bool statistical_flage = false;
                                        int now_min = DateTime.Now.Minute;
                                        int now_hour = DateTime.Now.Hour;
                                        if ((now_min == 0) && (now_min != last_min))
                                        {
                                            statistical_flage = true;
                                        }
                                        else
                                        {
                                            statistical_flage = false;
                                        }
                                        for (int i = 0; i < pcalculate.AllOutputData.Count; i++)
                                        {
                                            resultParam.CamID = pcalculate.AllOutputData[i].CamID;
                                            resultParam.CPos = pcalculate.AllOutputData[i].UpHumanVolume;
                                            resultParam.CNeg = pcalculate.AllOutputData[i].DownHumanVolume;
                                            resultParam.CPos_incr = pcalculate.AllOutputData[i].DeltUpHuman;
                                            resultParam.CNeg_incr = pcalculate.AllOutputData[i].DeltDownHuman;
                                            resultParam.DetectTime = DateTime.Now;
                                            resultParam.Speed = pcalculate.AllOutputData[i].AverageSpeed;
                                            resultParam.AverageUpSpeed = pcalculate.AllOutputData[i].AverageUpSpeed;
                                            resultParam.AverageDownSfpeed = pcalculate.AllOutputData[i].AverageDownSfpeed;
                                            resultParam.Density = pcalculate.AllOutputData[i].AverageDensity;
                                            if (resultParam.CamID != 0)
                                            {
                                                mp.insertData(resultParam);//5s插入数据库
                                            }

                                            //存储历史数据,第i路数据
                                            collectData[i].list_sec_CPos.Add(resultParam.CPos);
                                            collectData[i].list_sec_CNeg.Add(resultParam.CNeg);
                                            collectData[i].list_sec_Speed.Add(resultParam.Speed);
                                            collectData[i].list_sec_Speed_up.Add(resultParam.AverageUpSpeed);
                                            collectData[i].list_sec_Speed_down.Add(resultParam.AverageDownSfpeed);
                                            collectData[i].list_sec_Density.Add(resultParam.Density);

                                            ////////统计一小时数据////////
                                            if (statistical_flage)
                                            {
                                                if ((now_hour == 0) && (now_min == 0))
                                                {
                                                    statistical.oneHorSta(2, resultParam.CamID, collectData[i], last_onehr_index_CPos[i], last_onehr_index_CNeg[i], last_onehr_index_Speed[i], last_onehr_index_Speed_up[i], last_onehr_index_Speed_down[i], last_onehr_index_Density[i]);
                                                }
                                                else
                                                {
                                                    statistical.oneHorSta(1, resultParam.CamID, collectData[i], last_onehr_index_CPos[i], last_onehr_index_CNeg[i], last_onehr_index_Speed[i], last_onehr_index_Speed_up[i], last_onehr_index_Speed_down[i], last_onehr_index_Density[i]);
                                                }
                                                last_onehr_index_CPos[i] = collectData[i].list_sec_CPos.Count;
                                                last_onehr_index_CNeg[i] = collectData[i].list_sec_CNeg.Count;
                                                last_onehr_index_Speed[i] = collectData[i].list_sec_Speed.Count;
                                                last_onehr_index_Speed_up[i] = collectData[i].list_sec_Speed_up.Count;
                                                last_onehr_index_Speed_down[i] = collectData[i].list_sec_Speed_down.Count;
                                                last_onehr_index_Density[i] = collectData[i].list_sec_Density.Count;

                                                collectData[i].list_sec_CPos.Clear();
                                                collectData[i].list_sec_CNeg.Clear();
                                                collectData[i].list_sec_Speed.Clear();
                                                collectData[i].list_sec_Speed_up.Clear();
                                                collectData[i].list_sec_Speed_down.Clear();
                                                collectData[i].list_sec_Density.Clear();
                                                last_onehr_index_CPos[i] = 0;
                                                last_onehr_index_CNeg[i] = 0;
                                                last_onehr_index_Speed[i] = 0;
                                                last_onehr_index_Speed_up[i] = 0;
                                                last_onehr_index_Speed_down[i] = 0;
                                                last_onehr_index_Density[i] = 0;

                                            }
                                            ////////统计一天数据////////
                                            if ((now_hour == 23) && (now_min == 50) && (now_min != last_min))
                                            {
                                                CollectData cData = new CollectData();//不存储23-00数据
                                                mp.loadHorData(cData, resultParam.CamID);
                                                string table = "pflow_tb_data_day";
                                                statistical.Sta(resultParam.CamID, cData,table);

                                                //数据库表的状态检测
                                                CreatTable dt_man = new CreatTable();
                                                dt_man.creat_table();
                                                
                                               
                                                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)//周日汇总本周的数据
                                                {
                                                    CollectData cseveData = new CollectData();
                                                    mp.load7DayData(cseveData, resultParam.CamID);
                                                    table = "pflow_tb_data_week";
                                                    statistical.Sta(resultParam.CamID, cseveData, table);
                                                }
                                                if (DateTime.Now.AddDays(1).Day==1)//每月一号汇总前一个月的数据
                                                {
                                                    CollectData cseveData = new CollectData();
                                                    mp.load1MonthData(cseveData, resultParam.CamID);//会查询到后于当前时间的数据
                                                    table = "pflow_tb_data_month";
                                                    statistical.Sta(resultParam.CamID, cseveData, table);
                                                }
                                                if ((DateTime.Now.Month == 12) && (DateTime.Now.AddDays(1).Day == 1))//每年12月最后一天汇总当年的数据
                                                {
                                                    CollectData cseveData = new CollectData();
                                                    mp.load1YearData(cseveData, resultParam.CamID);
                                                    table = "pflow_tb_data_year";
                                                    statistical.Sta(resultParam.CamID, cseveData, table);
                                                }
                                            }
                                            last_min = DateTime.Now.Minute;//reset last time
                                        }
                                        
                                        if (simulation_flag)
                                        {
                                            //模拟参数插入数据库
                                            //resultParam.CamID = 123;
                                            //resultParam.CPos += 1;
                                            //resultParam.CNeg += 1;
                                            //resultParam.CPos_incr = 1;
                                            //resultParam.CNeg_incr = 1;
                                            //Random rad = new Random();
                                            //double d = rad.NextDouble();//double本来就是产生0-1之间小数的
                                            //resultParam.Density = Convert.ToDouble(d.ToString("#0.00"));//这里输出是控制输出几位数，0.00表示小数点后两位！
                                            //resultParam.Speed = rad.Next(5, 10);
                                            //resultParam.DetectTime = DateTime.Now;
                                            //mp.insertData(resultParam);//5s插入数据库
                                        }
                                    
                                    }
                                    /////////////////////////////插入原始数据////////////////////////
                                    for (int i = 0; i < sPipeInfo.szVideoInfo.Length; i++)//第i个摄像头
                                    {
                                        for (int j = 0; j < sPipeInfo.szVideoInfo[i].iObjCount; j++)//第j个轨迹点
                                        {
                                            if (lastCarId == sPipeInfo.szVideoInfo[i].szObjInfo[j].ID)
                                            {
                                                result.center_x = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_x;
                                                result.center_y = sPipeInfo.szVideoInfo[i].szObjInfo[j].center_y;
                                                //listinfo.Add(result);
                                                str += result.center_x.ToString() + "," + result.center_y.ToString() + "|";
                                            }
                                            if (lastCarId != 999 && lastCarId != sPipeInfo.szVideoInfo[i].szObjInfo[j].ID && sPipeInfo.szVideoInfo[i].szObjInfo[j].ID != 0)//不同ID表示轨迹信息为下一辆车的，则开始计算上一辆车的轨迹数据
                                            {
                                                //if (listinfo.Count != 0)
                                                //{
                                                    //tracker.Add(listinfo);
                                                    //listinfo.Clear();
                                                    //flag_2 = 1;
                                                    Points point = new Points();
                                                    point.CamID = sPipeInfo.szVideoInfo[i].iVideoID;
                                                    point.points = str;
                                                    point.DetectTime = DateTime.Now;
                                                    point.track_id = lastCarId;
                                                    if (str != "")//
                                                    {
                                                        //mp.insertPoint(point);
                                                    }

                                                    str = "";
                                                    tracker.Clear();
                                                //}
                                            }
                                            if (sPipeInfo.szVideoInfo[i].szObjInfo[j].ID != 0)
                                            {
                                                lastCarId = sPipeInfo.szVideoInfo[i].szObjInfo[j].ID;//替换上一个carID
                                            }
                                        }
                                    }
                                   }
                                }
                                else
                                {
                                    Console.WriteLine("参数接收失败");
                                }

                            sPipeInfo = null;
                        }

                        //stop read pipe thread
                        m_bExitReadPipe = true;
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    //Log.WriteLog(ex, "");//往TXT写入异常信息
            //    throw;
            //}
        }
        private void CommitSuicide(DataRec dr)
        {
            Process pr = new Process();//启动管道
            pr.StartInfo.WorkingDirectory = @"" + dr.CamID.ToString() + @"\Release";//..\..\..\..\..\..\C++\
            pr.StartInfo.FileName = "CarDetect" + dr.CamID.ToString() + ".exe";

            pr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            pr.Start();

            Thread.Sleep(3000);
            dr.PipeFlag = true;

        }
        private void MyAsyncCallBack(IAsyncResult ar)
        {

        }
        int last_min_ = 0;
        public bool calculate2()
        {
            if (DateTime.Now.Second % 5 == 0 && last_min_ != DateTime.Now.Second)
            {
                last_min_ = DateTime.Now.Second;
                Thread.Sleep(100);
                return true;
            }
            else
            {
                return false;
            }

        }
        private void dosth(DataRec dr)
        {	
        }

        private void ReadPipeThread()
        {
            while(!m_bExitReadPipe)
            {
                // read
                SHstPipeInfo sPipeInfo = new SHstPipeInfo();
                MyPipPersistane myPipPersistane = new MyPipPersistane();
                int iRes = myPipPersistane.ReadPipeInfo(m_hPipe, ref sPipeInfo);

                //todo add to queue
                lock (m_oReadPipeLock)
                {
                    while (m_pipeInfoQueue.Count > m_iMaxPipeInfoCount)
                    {
                        SHstPipeInfo sPipeInfoTmp = (SHstPipeInfo)m_pipeInfoQueue.Dequeue();
                        Console.WriteLine("丢包 {0}\n",sPipeInfo.szVideoInfo[0].i64Time);
                        sPipeInfoTmp = null;
                    }

                    m_pipeInfoQueue.Enqueue(sPipeInfo);
                }
            }
        }
       
    }
}
#endregion