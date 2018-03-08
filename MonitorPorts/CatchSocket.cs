using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonitorPorts
{
    class CatchSocket
    {
        public bool KeepCatching;
        public GeneratePcapFile genePcapFile;
        public GeneratePcapFile genePcapFile_Signaling;
        private static int receiveBufferLength;
        private byte[] receiveBufferBytes;
        private Socket socket;
        private string _fileSavePath;
        private string _fileName;
        private string _fileSavePath_Signaling;
        private string _fileName_Signaling;
        FileInfo File;
        //private delegate void UpdateState();
        //UpdateState Update;
        //public HandlePacket Handle = new HandlePacket();
        bool bStatus_Signal = false;//是否进行信号监测的标志位，FALSE表示否
        bool bStatus_Signaling = false;//是否进行信令监测的标志位，FALSE表示否
        public string FileSavePath
        {
            get { return _fileSavePath; }
            set { _fileSavePath = value; }
        }
        public string FileSavePath_Signaling
        {
            get { return _fileSavePath_Signaling; }
            set { _fileSavePath_Signaling = value; }
        }
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        public string FileName_Signaling
        {
            get { return _fileName_Signaling; }
            set { _fileName_Signaling = value; }
        }
        public CatchSocket(MainForm Main)
        {
            CreatPcapFile(System.DateTime.Now);       //林庆庆
            CreatPcapFile_Signaling(System.DateTime.Now);      //林庆庆


            Thread CBTCSaveToPcapThread = new Thread(CBTCOutputToPcap);             //林庆庆
            CBTCSaveToPcapThread.IsBackground = true;            //林庆庆
            CBTCSaveToPcapThread.Start();

            Thread SignallingSaveToPcapThread = new Thread(LteOutputToPcap);            //林庆庆
            SignallingSaveToPcapThread.IsBackground = true;              //林庆庆
            SignallingSaveToPcapThread.Start();

            HandlePacket.PacketArrival += Main.ShowInForm;
            receiveBufferLength = 1500;
            receiveBufferBytes = new byte[receiveBufferLength];
        }

        private void LteOutputToPcap()            //方法增加林庆庆
        {
            List<PacketProperties> OutputToPcapList = new List<PacketProperties>();
            while (true)
            {
                HiPerfTimer hip = new HiPerfTimer();
                hip.Start();
                hip.Interval(5000);
                lock (HandlePacket.LTEDataCache)
                {
                    OutputToPcapList = HandlePacket.LTEDataCache.Skip(0).Take(HandlePacket.LTEDataCache.Count).ToList();
                    HandlePacket.LTEDataCache.Clear();
                }
                LTEOutput(OutputToPcapList);
            }
        }

        private void LTEOutput(List<PacketProperties> outputToPcapList)        //方法增加林庆庆
        {
            foreach (var packet in outputToPcapList)
            {
                FileInfo File = new FileInfo(FileSavePath_Signaling + "\\" + FileName_Signaling + ".pcap");
                if (File.Length < FilterForm.PcapLengthLte)//FilterForm.PcapLengthLte
                {
                    genePcapFile_Signaling.WritePacketData(packet.CaptureTime, packet.Buf, packet.BufLength);
                }
                else
                {
                    CreatPcapFile_Signaling(packet.CaptureTime);
                    genePcapFile_Signaling.WritePacketData(packet.CaptureTime, packet.Buf, packet.BufLength);
                }
            }
        }

        private void CBTCOutputToPcap()      //方法增加林庆庆
        {
            List<PacketProperties> OutputToPcapList = new List<PacketProperties>();
            while (true)
            {
                HiPerfTimer hip = new HiPerfTimer();
                hip.Start();
                hip.Interval(5000);
                lock (HandlePacket.CBTCDataCache)
                {
                    OutputToPcapList = HandlePacket.CBTCDataCache.Skip(0).Take(HandlePacket.CBTCDataCache.Count).ToList();
                    HandlePacket.CBTCDataCache.Clear();
                }
                Output(OutputToPcapList);
            }
        }

        private void Output(List<PacketProperties> outputToPcapList)         //方法增加林庆庆
        {
            foreach (var packet in outputToPcapList)
            {
                File = new FileInfo(FileSavePath + "\\" + FileName + ".pcap");
                if (File.Length < FilterForm.PcapLengthNum)
                {
                     genePcapFile.WritePacketData(packet.CaptureTime, packet.Buf, packet.BufLength);
                }
                else
                {
                    CreatPcapFile(packet.CaptureTime);
                    genePcapFile.WritePacketData(packet.CaptureTime, packet.Buf, packet.BufLength);
                }
            }
        }

        public void CreatPcapFile(DateTime CreatTime)//创建信号pcap文件         //林庆庆
        {
            genePcapFile = new GeneratePcapFile();
            string fileName = "\\CBTC";//信号pcap文件夹的名称
            FileName = CreatTime.ToString("yyyy") + "." + CreatTime.ToString("MM") + "." + CreatTime.ToString("dd") + "  " + CreatTime.ToString("HH：mm");      //林庆庆
            FileSavePath = SetFileSavePath(fileName, CreatTime);
            genePcapFile.CreatPcap(FileSavePath, FileName);
        }

        public void CreatPcapFile_Signaling(DateTime CreatTime)//创建信令pcap文件     //林庆庆
        {
            genePcapFile_Signaling = new GeneratePcapFile();
            string fileName_Signaling = "\\LTE-M";//信令pcap文件夹的名称
            FileName_Signaling = CreatTime.ToString("yyyy") + "." + CreatTime.ToString("MM") + "." + CreatTime.ToString("dd") + "  " + CreatTime.ToString("HH：mm");      //林庆庆
            FileSavePath_Signaling = SetFileSavePath(fileName_Signaling, CreatTime);
            genePcapFile_Signaling.CreatPcap(FileSavePath_Signaling, FileName_Signaling);
        }

        private string SetFileSavePath(string fileName, DateTime CreatTime)      //林庆庆
        {
            string startPath = Application.StartupPath + fileName;
            string FilePath = startPath + "\\" + CreatTime.Year + "\\" + CreatTime.Month.ToString().PadLeft(2,'0') + "月" + "\\" + CreatTime.Day.ToString().PadLeft(2,'0') + "日";
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            return FilePath;
        }

        public void Start(bool bStatues_CBTC, bool bStatues_Signal, Socket socket)
        {
            if (MainForm.IsFirstClick)
            {
                bStatus_Signal = bStatues_CBTC;//信号的状态
                bStatus_Signaling = bStatues_Signal;//信令的状态
                this.socket = socket;
                KeepCatching = true;
                BeginReceive();
                MainForm.IsFirstClick = false;
            }
            else
            {
                HandlePacket.Pause = false;
            }

        }

        public void BeginReceive()
        {
            if (socket != null)
            {
                object state = null;
                state = socket;
                try                                            //lishuai
                {
                    IAsyncResult ar = socket.BeginReceive(receiveBufferBytes, 0, receiveBufferLength, SocketFlags.None, new AsyncCallback(CallReceive), state);
                }
                catch (SocketException e)
                {
                    MessageBox.Show(DateTime.Now + ":" + e.Message);
                }
                catch (ArgumentNullException e)
                {
                    MessageBox.Show(DateTime.Now + ":" + e.Message);
                }
                catch (ObjectDisposedException e)
                {
                    MessageBox.Show(DateTime.Now + ":" + e.Message);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    MessageBox.Show(DateTime.Now + ":" + e.Message);
                }
            }
        }

        HandlePacket handle;
        private void CallReceive(IAsyncResult ar)
        {
            int receivedBytes = socket.EndReceive(ar);
            if (KeepCatching == true)
            {
                MainForm.NumOfReceivePackets++;//signal
                //Update.BeginInvoke(null, null);
                handle = new HandlePacket();
                handle.Unpack(receiveBufferBytes, receivedBytes, this, bStatus_Signal, bStatus_Signaling);
                handle = null;
                Array.Clear(receiveBufferBytes, 0, receiveBufferBytes.Length);
            }
            Array.Clear(receiveBufferBytes, 0, receiveBufferBytes.Length);
            BeginReceive();
        }

        public void Stop()
        {
            KeepCatching = true;
            HandlePacket.Pause = true;
        }
    }
}
