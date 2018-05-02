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
namespace HST_Server
{
    public delegate void TimesUp(DataRec dr);
    public delegate void Method(DataRec dr);
    
    public class DataRec
    {
        //public byte[] buffer = new byte[20480];
        public byte[] buffer = new byte[20480];
        //SHstPipeInfo buffer;
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
        string str = "";

        void exep_Exited(object sender, EventArgs e) 
        { 
            Console.WriteLine(".exe运行完毕"); 
        } 
        public void runing(object param)
        {
            try
            {
                bool calculate_flag = true;
                byte[] writebuffer = new byte[20480];
                //String strPipeName = @"\\.\pipe\myPipe2";
                string g_czPipName_Base ="\\\\.\\pipe\\HST_Output_Pip_Gpu_%d"; //0~3
                string strPipeName = "\\\\.\\pipe\\HST_Output_Pip_Gpu_0";
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
                        
                        while (dr.PipeFlag)
                        {
                            #region[接收数据]

                            SHstPipeInfo sPipeInfo = new SHstPipeInfo();
                            MyPipPersistane myPipPersistane = new MyPipPersistane();

                            int iRes = myPipPersistane.ReadPipeInfo(dr.hPipe, ref sPipeInfo);
                            if (iRes == 0)
                            {
                                //todo 参数处理
                                Console.WriteLine("\r ReadPipeInfo videocount： {0} time： {1} \t v0:{2}:{3} \t  v1:{4}:{5} \t  v2:{6}:{7} \t  v3:{8}:{9} \t"
                                    , sPipeInfo.iVideoCount, sPipeInfo.szVideoInfo[0].i64Time
                                    , sPipeInfo.szVideoInfo[0].iVideoID, sPipeInfo.szVideoInfo[0].iObjCount
                                    , sPipeInfo.szVideoInfo[1].iVideoID, sPipeInfo.szVideoInfo[1].iObjCount
                                    , sPipeInfo.szVideoInfo[2].iVideoID, sPipeInfo.szVideoInfo[2].iObjCount
                                    , sPipeInfo.szVideoInfo[3].iVideoID, sPipeInfo.szVideoInfo[3].iObjCount
                                    );

                                continue; //测试用，需删除 吴超

                                //Console.WriteLine("{0},{1}", sPipeInfo.szVideoInfo[0].iVideoID, sPipeInfo.szVideoInfo[0].szObjInfo[0].center_x);
                                //Console.WriteLine("***********{0}************",sPipeInfo.szVideoInfo[0].szObjInfo[0].ID);
 
                                bool caculateflag = true; 
                                //Console.WriteLine("{0} ready to caculate", param.ToString());
                                if (simulation_flag)
                                {
                                    //模拟计算
                                    calculate2();//模拟数据计算
                                }
                                else
                                {
                                    //实际计算
                                    //if (sPipeInfo.szVideoInfo[0].iObjCount != 0)
                                    //{
                                    //    Console.WriteLine("");
                                    //    caculateflag = pcalculate.calculate(sPipeInfo);
                                    //}

                                    //for (int i = 0; i < pcalculate.AllOutputData.Count; i++)
                                    //{
                                    //    resultParam.CamID = pcalculate.AllOutputData[i].CamID;
                                    //    //if (pcalculate.AllOutputData[i].CamID==)

                                    //    resultParam.CPos = pcalculate.AllOutputData[i].UpHumanVolume;
                                    //    resultParam.CNeg = pcalculate.AllOutputData[i].DownHumanVolume;
                                    //    resultParam.CPos_incr = pcalculate.AllOutputData[i].DeltUpHuman;
                                    //    resultParam.CNeg_incr = pcalculate.AllOutputData[i].DeltDownHuman;
                                    //    resultParam.DetectTime = DateTime.Now;
                                    //    resultParam.Speed = pcalculate.AllOutputData[i].AverageSpeed;
                                    //    resultParam.Density = pcalculate.AllOutputData[i].AverageDensity;
                                    //}
                                }
                                
                                if (caculateflag)
                                {
                                    if (simulation_flag)
                                    {
                                        //模拟参数插入数据库
                                        resultParam.CamID = 123;
                                        resultParam.CPos += 1;
                                        resultParam.CNeg += 1;
                                        resultParam.CPos_incr = 1;
                                        resultParam.CNeg_incr = 1;
                                        Random rad = new Random();
                                        double d = rad.NextDouble();//double本来就是产生0-1之间小数的
                                        resultParam.Density = Convert.ToDouble(d.ToString("#0.00"));//这里输出是控制输出几位数，0.00表示小数点后两位！
                                        resultParam.Speed = rad.Next(5, 10); 
                                        resultParam.DetectTime = DateTime.Now;
                                    }
                                    mp.insertData(resultParam);
                                }
                                ///////////////////////////插入原始数据////////////////////////
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
                                                point.points = str;
                                                point.DetectTime = DateTime.Now;
                                                point.track_id = lastCarId;
                                                if (str != "")
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
                            else
                            {
                                Console.WriteLine("参数接收失败");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.WriteLog(ex, "");//往TXT写入异常信息
                throw;
            }
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
        int last_min = 0;
        public bool calculate2()
        {
            if (DateTime.Now.Second % 5 == 0 && last_min != DateTime.Now.Second)
            {
                last_min = DateTime.Now.Second;
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
            //SHstPipeInfo sPipeInfo = new SHstPipeInfo();
            //MyPipPersistane myPipPersistane = new MyPipPersistane();

            //int iRes = myPipPersistane.ReadPipeInfo(dr.hPipe, ref sPipeInfo);

            //if (iRes == 0)
            //{
            //    //todo 后续处理
            //   // printf("\r ReadPipeInfo videocount %d time %I64d \t", sPipeInfo.iVideoCount, sPipeInfo.szVideoInfo[0].i64Time);
            //}		
        }
    }
}
#endregion