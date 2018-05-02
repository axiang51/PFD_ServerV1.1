using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HST_Server
{
    public class SHstPipeVideoInfo
    {
        public char[] czFcc = new char[4];
        public int iVideoID;
        public int iWidth;
        public int iHeight;
        public System.Int64 i64Time;
        public int iObjCount;
        public SHstObjInfo[] szObjInfo = new SHstObjInfo[1000];

        public SHstPipeVideoInfo()
        {
            czFcc[0] = 'V';
            czFcc[1] = 'I';
            czFcc[2] = 'D';
            czFcc[3] = 'O';

            iVideoID = 0;
            iWidth = 0;
            iHeight = 0;
            i64Time = 0;
            iObjCount = 0;

            for (int i = 0; i < 1000; i++)
            {
                szObjInfo[i] = new SHstObjInfo();
            }
        }
    }

    public class SHstPipeInfo
    {
        public char[] czFcc = new char[4];
        public int iVideoCount;//iVideoCount 共有几路视频  
        SHstPipeVideoInfo shst = new SHstPipeVideoInfo();
       public SHstPipeVideoInfo[] szVideoInfo=new SHstPipeVideoInfo[10];//每路视频，当前帧的数据
        

        public SHstPipeInfo()
        {
            for(int i=0;i<10;i++)
            {
                //szVideoInfo[i] = shst;
                szVideoInfo[i] = new SHstPipeVideoInfo();
             }
            czFcc[0] = 'H';
            czFcc[1] = 'S';
            czFcc[2] = 'T';
            czFcc[3] = 'P';

            iVideoCount = 0;
        }
    }
    public class SHstObjInfo
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public int center_x;			//目标框中心点x
        public int center_y; 			//目标框中心点y
        public int ID;					//	目标唯一ID，同一ID为同一目标
        public double confidence;		//	置信度
    }
    public class MyPipPersistane
    {
/*
管道接口定义
*/

const string g_czPipName_Base = "\\\\.\\pipe\\HST_Output_Pip_Gpu_%d"; //0~3

int MAX_PIPE_VIDEO_COUNT= 10;
int MAX_PIPE_OBJ_COUNT = 1000;
/*
数据格式：
每分析一次，发送一组下列数据。

'HSTP'			4byte fcc 同步标记
video count		4byte int	视频有几路	max 10
	//以下共有video count组数据
	'VIDO'			4byte fcc 同步标记
	videoID			4byte int	视频ID
	width			4byte int	视频宽度
	height			4byte int	视频高度
	time			8byte int64	时间，毫秒
	obj count		4byte int	分析行人数量	可能为0 max 1000
		//以下共有obj count组数据
		left		4byte int
		top			4byte int
		right		4byte int
		bottom		4byte int
		center_x	4byte int			//目标框中心点x
		center_y	4byte int			//目标框中心点y
		ID			4byte int			//	目标唯一ID，同一ID为同一目标
		confidence	8byte double		//	置信度

*/


/// <summary>
/// ///////////////////////////////////////////////////////////

//public string ReadString(IntPtr HPipe)
//{
//    string Val = "";
//    byte[] bytes = ReadBytes(HPipe);
//    if (bytes != null)
//    {
//        Val = System.Text.Encoding.UTF8.GetString(bytes);
//    }
//    return Val;
//}

//public byte[] ReadBytes(IntPtr HPipe)
//{
//    //传字节流
//    byte[] szMsg = null;
//    //byte[] dwBytesRead = new byte[4];
//    uint dwBytesRead = 0;
//    byte[] lenBytes = new byte[4];
//    int len;
//    if (PipeNative.ReadFile(HPipe, lenBytes, (uint)4, out dwBytesRead, IntPtr.Zero))//先读大小
//    {
//        len = System.BitConverter.ToInt32(lenBytes, 0);
//        szMsg = new byte[len];
//        if (!NamedPipeNative.ReadFile(HPipe, szMsg, (uint)len, dwBytesRead, 0))
//        {
//            //frmServer.ActivityRef.AppendText("读取数据失败");
//            return null;
//        }
//    }
//    return szMsg;
//}

//public float ReadFloat(IntPtr HPipe)
//{
//    float Val = 0;
//    byte[] bytes = new byte[4]; //单精度需4个字节存储
//    byte[] dwBytesRead = new byte[4];
//    if (!NamedPipeNative.ReadFile(HPipe, bytes, 4, dwBytesRead, 0))
//    {
//        //frmServer.ActivityRef.AppendText("读取数据失败");
//        return 0;
//    }
//    Val = System.BitConverter.ToSingle(bytes, 0);
//    return Val;
//}

//public double ReadDouble(IntPtr HPipe)
//{
//    double Val = 0;
//    byte[] bytes = new byte[8]; //双精度需8个字节存储
//    byte[] dwBytesRead = new byte[4];

//    if (!NamedPipeNative.ReadFile(HPipe, bytes, 8, dwBytesRead, 0))
//    {
//        //frmServer.ActivityRef.AppendText("读取数据失败");
//        return 0;
//    }
//    Val = System.BitConverter.ToDouble(bytes, 0);
//    return Val;
//}

//public int ReadInt(IntPtr HPipe)
//{
//    int Val = 0;
//    byte[] bytes = new byte[4]; //整型需4个字节存储
//    byte[] dwBytesRead = new byte[4];

//    if (!NamedPipeNative.ReadFile(HPipe, bytes, 4, dwBytesRead, 0))
//    {
//        //frmServer.ActivityRef.AppendText("读取数据失败");
//        return 0;
//    }
//    Val = System.BitConverter.ToInt32(bytes, 0);
//    return Val;
//}

/// ///////////////////////////////////////////////////////////
/// </summary>

int ReadPipe_Char(IntPtr in_hPipe, ref byte[] out_pcFcc)
{
    int iRes = 0;
    uint dwReaded = 0;
    bool bSuc = PipeNative.ReadFile(
        in_hPipe,
        out_pcFcc,
         1,
        out dwReaded,
        IntPtr.Zero);
    if (!bSuc)
    {
        iRes = -1;
    }
    return iRes;
}


int ReadPipe_Fcc(IntPtr in_hPipe, ref byte[] out_pcFcc)
{
	int iRes = 0;
    uint dwReaded = 0;
    bool bSuc = PipeNative.ReadFile(
        in_hPipe,
        out_pcFcc,
         4,
        out dwReaded,
        IntPtr.Zero);
	if (!bSuc)
	{
		iRes = -1;
	}
	return iRes;
}

int ReadPipe_Int32Lsb(IntPtr in_hPipe, ref int out_piVal)
{
	int iRes = 0;

    System.Byte[] uczBuffer = new Byte[4];

    uint dwReaded = 0;
    bool bSuc = PipeNative.ReadFile(
        in_hPipe,
        uczBuffer,
         4,
        out dwReaded,
        IntPtr.Zero);
	if (!bSuc)
	{
		iRes = -1;
	}

	out_piVal = 0;
	for (int i = 0; i < 4; i++)
	{
		out_piVal <<= 8;
		out_piVal += uczBuffer[4-i-1];
	}

	return iRes;
}

int ReadPipe_Int64Lsb(IntPtr in_hPipe, ref System.Int64 out_pi64Val)
{
	int iRes = 0;

    //unsigned char uczBuffer[8];
    byte[] uczBuffer=new byte[8];
	//DWORD dwReaded = 0;
    uint dwReaded = 0;
	//BOOL bSuc = ReadFile(in_hPipe, uczBuffer, 8, &dwReaded, NULL);
    bool bSuc = PipeNative.ReadFile(
       in_hPipe,
       uczBuffer,
        8,
       out dwReaded,
       IntPtr.Zero);
	if (!bSuc)
	{
		iRes = -1;
	}

	out_pi64Val = 0;
	for (int i = 0; i < 8; i++)
	{
		out_pi64Val <<= 8;
		out_pi64Val += uczBuffer[8-i-1];
	}

	return iRes;
}

int ReadPipe_Double(IntPtr in_hPipe, ref byte[] out_pdbVal)
{
	int iRes = 0;
	//out_pdbVal = 0;
    uint dwReaded = 0;
	//BOOL bSuc = ReadFile(in_hPipe, out_pdbVal, 8, &dwReaded, NULL);
    bool bSuc = PipeNative.ReadFile(
      in_hPipe,
      out_pdbVal,
       8,
      out dwReaded,
      IntPtr.Zero);
	if (!bSuc)
	{
		iRes = -1;
	}

	return iRes;
}

   // struct SHstPipeVideoInfo
   // {
   //     public char[] czFcc = new char[4];
   //     public int iVideoID;
   //     public int iWidth;
   //     public int iHeight;
   //     public System.Int64 i64Time;
   //     public int iObjCount;
   //     public SHstObjInfo[] szObjInfo=new SHstObjInfo[1000];

   //     SHstPipeVideoInfo()
   //     {
   //         czFcc[0] = 'V';
   //         czFcc[1] = 'I';
   //         czFcc[2] = 'D';
   //         czFcc[3] = 'O';

   //         iVideoID = 0;
   //         iWidth = 0;
   //         iHeight = 0;
   //         i64Time = 0;
   //         iObjCount = 0;
   //     }
   // }

   // public struct SHstPipeInfo
   // {
   //     public char[] czFcc = new char[4];
   //     public int iVideoCount;//iVideoCount 共有几路视频  
   //     public SHstPipeVideoInfo[] szVideoInfo = new SHstPipeVideoInfo[10];//每路视频，当前帧的数据

   //     SHstPipeInfo()
   //     {
   //         czFcc[0] = 'H';
   //         czFcc[1] = 'S';
   //         czFcc[2] = 'T';
   //         czFcc[3] = 'P';

   //         iVideoCount = 0;
   //     }
   // }
   //struct SHstObjInfo
   // {
   //     public int left;
   //     public int top;
   //     public int right;
   //     public int bottom;
   //     public int center_x;			//目标框中心点x
   //     public int center_y; 			//目标框中心点y
   //     public int ID;					//	目标唯一ID，同一ID为同一目标
   //     public double confidence;		//	置信度
   // }
    public int ReadPipeInfo(IntPtr in_hPipe, ref SHstPipeInfo out_psInfo)
{
	int iRes = 0;
    Char[] czFcc_char = new Char[4];
    //if (iRes == 0)
    //{
    //    Byte[] czFcc_byte = Encoding.Default.GetBytes(out_psInfo.czFcc);
    //    iRes = ReadPipe_Fcc(in_hPipe, ref czFcc_byte);
    //    czFcc_char = Encoding.ASCII.GetChars(czFcc_byte);
    //    out_psInfo.czFcc = czFcc_char;
    //}

    ////if (out_psInfo->czFcc[0] != 'H'
    ////    || out_psInfo->czFcc[1] != 'S'
    ////    || out_psInfo->czFcc[2] != 'T'
    ////    || out_psInfo->czFcc[3] != 'P'
    ////    )
    //if (czFcc_char[0] != 'H'
    // || czFcc_char[1] != 'S'
    // || czFcc_char[2] != 'T'
    // || czFcc_char[3] != 'P'
    // )
    //{
    //    iRes = -1;
    //}

    if (iRes == 0)
    {
        Byte[] czFcc_byte = Encoding.Default.GetBytes(out_psInfo.czFcc);
        iRes = ReadPipe_Char(in_hPipe, ref czFcc_byte);
        if(czFcc_byte[0] != 'H')
        {
            iRes = -1;
        }
    }

    if (iRes == 0)
    {
        Byte[] czFcc_byte = Encoding.Default.GetBytes(out_psInfo.czFcc);
        iRes = ReadPipe_Char(in_hPipe, ref czFcc_byte);
        if (czFcc_byte[0] != 'S')
        {
            iRes = -1;
        }
    }

    if (iRes == 0)
    {
        Byte[] czFcc_byte = Encoding.Default.GetBytes(out_psInfo.czFcc);
        iRes = ReadPipe_Char(in_hPipe, ref czFcc_byte);
        if (czFcc_byte[0] != 'T')
        {
            iRes = -1;
        }
    }

    if (iRes == 0)
    {
        Byte[] czFcc_byte = Encoding.Default.GetBytes(out_psInfo.czFcc);
        iRes = ReadPipe_Char(in_hPipe, ref czFcc_byte);
        if (czFcc_byte[0] != 'P')
        {
            iRes = -1;
        }
    }

	if (iRes == 0)
    {
        iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.iVideoCount);
	}

	if (out_psInfo.iVideoCount > MAX_PIPE_VIDEO_COUNT)
	{
		iRes = -1;
		out_psInfo.iVideoCount = 0;
	}

	for (int iVideoIdx = 0; iVideoIdx < out_psInfo.iVideoCount; iVideoIdx++)
	{
        Char[] czFcc_char2 = new Char[4];
		if (iRes == 0)
		{
            Byte[] czFcc_byte2 =new Byte[4];
            //Byte[] czFcc_byte2 = Encoding.Default.GetBytes(out_psInfo.szVideoInfo[iVideoIdx].czFcc);
            iRes = ReadPipe_Fcc(in_hPipe, ref czFcc_byte2);
            czFcc_char2 = Encoding.ASCII.GetChars(czFcc_byte2);
            for (int i = 0; i < czFcc_char2.Length; i++)
            {
                out_psInfo.szVideoInfo[iVideoIdx].czFcc[i] = czFcc_char2[i];
            }
		}

        if (   czFcc_char2[0] != 'V'
            || czFcc_char2[1] != 'I'
            || czFcc_char2[2] != 'D'
            || czFcc_char2[3] != 'O'
			)
		{
			iRes = -1;
		}

		if (iRes == 0)
		{
            iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].iVideoID);
		}

		if (iRes == 0)
		{
            iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].iWidth);
		}

		if (iRes == 0)
		{
            iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].iHeight);
		}

		if (iRes == 0)
		{
            iRes = ReadPipe_Int64Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].i64Time);
		}

		if (iRes == 0)
		{
            iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].iObjCount);
		}
		
		if (out_psInfo.szVideoInfo[iVideoIdx].iObjCount > MAX_PIPE_OBJ_COUNT)
		{
			iRes = -1;
			out_psInfo.szVideoInfo[iVideoIdx].iObjCount = 0;
		}
		
		for (int iObjIdx = 0; iObjIdx < out_psInfo.szVideoInfo[iVideoIdx].iObjCount; iObjIdx++)
		{
			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].left);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].top);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].right);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].bottom);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].center_x);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].center_y);
			}

			if (iRes == 0)
			{
                iRes = ReadPipe_Int32Lsb(in_hPipe, ref out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].ID);
			}

			if (iRes == 0)
            {
                byte[] _confidence = BitConverter.GetBytes(out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].confidence);
                iRes = ReadPipe_Double(in_hPipe, ref _confidence);
                out_psInfo.szVideoInfo[iVideoIdx].szObjInfo[iObjIdx].confidence = BitConverter.ToDouble(_confidence, 0);  
			}
		}
	}

	return iRes;
    }
    }

}
