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
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace HST_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            run runing = new run();
            runing.runing(1);
        }
    }
    //public class Test
    //{
    //    Thread t;
    //    Thread thread;
    //    private string path;
    //    private string name;
    //    private Int32 gpuid;
    //    private Int32 ProcessMinL;
    //    private Int32 th;
    //    private Int32[] data;
    //    private Int32 length;

    //    Point start_pos = new Point();
    //    Point end_pos = new Point();
    //    Point start_neg = new Point();
    //    Point end_neg = new Point();
    //    int cPos = 0;//计数-从画面上方走到下方
    //    int cNeg = 0;//计数-从画面下方走到上方
    //    int cPos_repalce = 0;
    //    int cNeg_repalce = 0;
    //    float density;//密度
    //    double speed;//速度
    //    double camerahigh = 30000;//
    //    double measureangle = 60;//
    //    double TiltAngle = 60;//
            
            
    //    [DllImport("VLC2Mat.dll", EntryPoint = "play", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    //    private static extern void play(char[] viedoname, char[] name, Int32 gpuid, Int32 ProcessMinL, Int32 th,
    //    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] Int32[] data, Int32 length);

    //    public Test(string _path, string _name, Int32 _gpuid, Int32 _ProcessMinL, Int32 _th, Int32[] _data, Int32 _length)
    //    {
    //        this.path = _path;
    //        this.name = _name;
    //        this.gpuid = _gpuid;
    //        this.ProcessMinL = _ProcessMinL;
    //        this.th = _th;
    //        this.data = _data;
    //        this.length = _length;
    //        t = new Thread(show);
    //        t.IsBackground = true;
    //        t.Start();            //this.label1.Text = result1.center_x.ToString();
    //        //thread = new Thread(track);
    //        //thread.Name = _name;
    //        //thread.IsBackground = true;
    //        //thread.Start();

    //    }
    //    [DllImport("user32.dll", SetLastError = true)]
    //    private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    //    [DllImport("user32.dll", EntryPoint = "SetWindowLongA", SetLastError = true)]
    //    private static extern long SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);

    //    [DllImport("user32.dll", SetLastError = true)]
    //    private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

    //    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    //    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    //    private int calDirectionValue(int y1, int y2)
    //    {
    //        int mark = y2 - y1;
    //        if (mark >= 0)
    //        {
    //            return 0;
    //        }
    //        else
    //        {
    //            return 1;
    //        }

    //    }
    //    private void show()
    //    {
    //            play(path.ToArray(), name.ToArray(), gpuid, ProcessMinL, th,data,length);  
    //    }

    //    public void track()
    //    {
    //        Console.WriteLine("START");
    //        start_pos.X = 0;
    //        start_pos.Y = 720 / 3;
    //        end_pos.X = 1280;
    //        end_pos.Y = 720 / 3;
    //        start_neg.X = 0;
    //        start_neg.Y = 2 * 720 / 3;
    //        end_neg.X = 1280;
    //        end_neg.Y = 2 * 720 / 3;
    //        while (true)
    //        {
    //            Int32[] Data=new Int32[2000];
    //            data.CopyTo(Data,0);
    //            List<List<HST_Result>> tracker = new List<List<HST_Result>>();
    //            HST_Result result = new HST_Result();
    //            Int32 tracksNum = Data[0];
    //            Int32[] data_replace = new Int32[2000];
    //            for (int k = tracksNum + 3; k < Data.Length; k++)
    //            {
    //                data_replace[k - tracksNum - 3] = Data[k];
    //            }
    //            int tracknum = 0;
    //            for (int m = 0; m < tracksNum; m++)
    //            {
    //                //Console.WriteLine("m is {0}, tracknum is {1}", m, Data[m + 2]);
    //                List<HST_Result> listinfo = new List<HST_Result>();
    //                tracknum = tracknum + Data[m + 1];
    //                int tracknum_next = Data[m + 2];
    //                for (int j = 2 * tracknum; j < 2 * tracknum + 2 * tracknum_next; j = j + 2)
    //                {
    //                    result.center_x = data_replace[j];
    //                    result.center_y = data_replace[j + 1];
    //                    listinfo.Add(result);
    //                }
    //                tracker.Add(listinfo);
    //            }
    //            //for (int i = 0; i < tracker.Count(); i++)
    //            //{
    //            //    for (int j = 0; j < tracker[i].Count(); j++)
    //            //    {
    //            //        Console.WriteLine("第{0}条轨迹，检测框x:{1},y:{2}",i,tracker[i][j].center_x,tracker[i][j].center_y);
    //            //    }
    //            //}
    //            #region#计数#
    //            for (int k = 0; k < tracker.Count; k++)
    //            {
    //                int m = tracker[k].Count;
    //                if (m == 30)
    //                {
    //                    int x_now = tracker[k][m - 1].center_x;
    //                    int y_now = tracker[k][m - 1].center_y;
    //                    int x_next = tracker[k][0].center_x;
    //                    int y_next = tracker[k][0].center_y;
    //                    if ((y_now >= start_pos.Y && y_now <= start_neg.Y) || (y_next >= start_pos.Y && y_next <= start_neg.Y))
    //                    {
    //                        if (x_now >= start_pos.X && x_now <= end_pos.X)
    //                        {
    //                            int mark = calDirectionValue(y_now, y_next);
    //                            if (1 == mark)
    //                            {
    //                                cPos++;
    //                            }
    //                            if (0 == mark)
    //                            {
    //                                cNeg++;
    //                            }
    //                        }
    //                    }
    //                }
    //                if (m > 30)
    //                {
    //                    int x_now = tracker[k][m - 1].center_x;
    //                    int y_now = tracker[k][m - 1].center_y;
    //                    int x_before = tracker[k][m - 2].center_x; ;
    //                    int y_before = tracker[k][m - 2].center_y;

    //                    if ((y_now >= start_pos.Y && y_now <= start_neg.Y) && (y_before < start_pos.Y || y_before > start_neg.Y))
    //                    {
    //                        if (x_now >= start_pos.X && x_now <= end_pos.X)
    //                        {
    //                            int mark = calDirectionValue(y_now, y_before);
    //                            if (1 == mark)
    //                            {
    //                                cPos++;
    //                            }
    //                            if (0 == mark)
    //                            {
    //                                cNeg++;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            // form1.label1.Text = cPos.ToString();
    //            // form1.label2.Text = cNeg.ToString();

    //            if (cNeg > cNeg_repalce || cPos > cPos_repalce)
    //            {
    //                cPos_repalce = cPos;
    //                cNeg_repalce = cNeg;
    //                Console.WriteLine("{0} cPos{1},cNeg{2}.",thread.Name,cPos,cNeg);
    //                //Console.WriteLine("cPos{0}", cPos);
    //                //Console.WriteLine("cNeg{0}", cNeg);
    //                Thread.Sleep(1000);
    //            }

    //            #endregion
    //            #region#密度#
    //            int p = 0;//当前帧处于密度计算区域内的人数
    //            float s = 0;//单位面积
    //            for (int k = 0; k < tracker.Count; k++)
    //            {
    //                int m = tracker[k].Count;
    //                if (m >= 7)
    //                {
    //                    //int m = tracker[k].listinfo.Count; 
    //                    int x = tracker[k][m - 1].center_x;
    //                    int y = tracker[k][m - 1].center_y;
    //                    if ((y >= start_pos.Y && y <= start_neg.Y) && (x >= start_pos.X && x <= end_pos.X))
    //                    {
    //                        p++;
    //                    }
    //                }
    //            }
    //            if (tracker.Count < p || p < 0)
    //            {
    //                p = 0;
    //            }
    //            s = 1;//单位面积
    //            density = p / s;
    //            //this.label3.Text = density.ToString();
    //            #endregion
    //            #region#速度#
    //            int valid_n = 0;
    //            if (tracker.Count == 0)
    //            {
    //                speed = 0;
    //            }
    //            double sum = 0;
    //            for (int k = 0; k < tracker.Count; k++)
    //            {
    //                double tmp = calObiSpeed(tracker[k], ref valid_n);
    //                sum += tmp;
    //            }
    //            if (0 == valid_n)
    //            {
    //                speed = 0;
    //            }
    //            speed = sum / valid_n;
    //            //this.label4.Text = speed.ToString();
    //            #endregion
    //        }

    //    }
    //    private double calObiSpeed(List<HST_Result> listinfo, ref int number)
    //    {
    //        int n = listinfo.Count;
    //        int minlen = 2;
    //        double time = 0.11;
    //        if (n < minlen)
    //        {
    //            return 0;
    //        }
    //        number++;
    //        double k = 0;
    //        double b = 0;
    //        List<Point> points = new List<Point>();
    //        for (int i = n - minlen; i < n; i++)
    //        {
    //            int x = listinfo[i].center_x;
    //            int y = listinfo[i].center_y;
    //            points.Add(new Point(x, y));
    //        }
    //        lsm(points, ref k, ref b);
    //        double start_x = 0;
    //        double start_y = 0;
    //        double end_x = 0;
    //        double end_y = 0;
    //        PointRefine(k, b, points[0].X, points[0].Y, ref start_x, ref start_y);
    //        PointRefine(k, b, points[minlen - 1].X, points[minlen - 1].Y, ref end_x, ref end_y);
    //        double val = 0;
    //        if ((end_y >= start_pos.Y && end_y <= start_neg.Y) && (start_y >= start_pos.Y && start_y <= start_neg.Y))
    //        {
    //            double high = camerahigh * 0.001;  //h为摄像机高度，转化为米为单位
    //            double a1 = (200 - start_y) * Math.Sin(measureangle * Math.PI / 180) / (200 * (Math.Cos(measureangle * Math.PI / 180) - Math.Sin(TiltAngle * Math.PI / 180)) / 2);
    //            double s1 = (high - 1.60) * Math.Sqrt((a1 + Math.Tan((TiltAngle - measureangle / 2) * Math.PI / 180)) * (a1 + Math.Tan((TiltAngle - measureangle / 2) * Math.PI / 180)) + 1);
    //            double dx = 2 * s1 * Math.Tan((measureangle / 2) * Math.PI / 180) * (end_x - start_x) / 300;
    //            double b1 = Math.Atan(start_y * Math.Sin(measureangle * Math.PI / 180) / (200 - start_y + start_y * Math.Cos(measureangle * Math.PI / 180))) * Math.PI / 180;
    //            double b2 = Math.Atan(end_y * Math.Sin(measureangle * Math.PI / 180) / (200 - end_y + end_y * Math.Cos(measureangle * Math.PI / 180))) * Math.PI / 180;
    //            double c1 = (high - 1.60) / (Math.Cos((TiltAngle - measureangle / 2) * Math.PI / 180) * Math.Tan((TiltAngle + measureangle / 2) * Math.PI / 180));
    //            double d1 = Math.Sin(b1) / Math.Cos(((TiltAngle - measureangle / 2) * Math.PI / 180) + b1);
    //            double d2 = Math.Sin(b2) / Math.Cos(((TiltAngle - measureangle / 2) * Math.PI / 180) + b2);
    //            double dy = c1 * (d1 - d2);
    //            val = Math.Sqrt(dx * dx + dy * dy) / time;

    //        }
    //        return val;
    //    }
    //    private void lsm(List<Point> points, ref double k, ref double b)
    //    {
    //        int num = points.Count;
    //        double A, B, C, D;
    //        double tmp = 0;
    //        k = 0;
    //        b = 0;
    //        Matrix<double> mat1 = new Matrix<double>(1, num);
    //        Matrix<double> mat2 = new Matrix<double>(1, num);
    //        Matrix<double> mattmp = new Matrix<double>(1, num);
    //        for (int i = 0; i < num; i++)
    //        {
    //            mat1[0, i] = points[i].X;
    //            mat2[0, i] = points[i].Y;
    //        }
    //        CvInvoke.cvMul(mat1, mat1, mattmp, 1);
    //        A = CvInvoke.cvSum(mattmp).v0;
    //        B = CvInvoke.cvSum(mat1).v0;
    //        CvInvoke.cvMul(mat1, mat2, mattmp, 1);
    //        C = CvInvoke.cvSum(mattmp).v0;
    //        D = CvInvoke.cvSum(mat2).v0;
    //        tmp = A * mat1.Cols - B * B;
    //        if (Math.Abs(tmp) < 0.001)
    //        {
    //            k = 0;
    //            b = 0;
    //            return;
    //        }
    //        k = (C * mat1.Cols - B * D) / tmp;
    //        b = (A * D - C * B) / tmp;
    //        mat1.Dispose();
    //        mat2.Dispose();

    //    }
    //    private void PointRefine(double k, double b, int x, int y, ref double RefineX, ref double RefineY)
    //    {
    //        if (0 == k)
    //        {
    //            RefineX = x;
    //            RefineY = b;
    //        }
    //        else
    //        {
    //            double u = (y * k + x - b * k) / (k * k + 1);
    //            RefineX = u;
    //            RefineY = k * u + b;
    //        }
    //    }

    //}
    //[StructLayout(LayoutKind.Sequential)]
    //public struct HST_Result
    //{
    //    public Int32 center_x;
    //    public Int32 center_y;
    //}
}
