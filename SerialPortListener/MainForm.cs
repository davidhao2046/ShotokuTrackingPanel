using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPortListener.Serial;
using System.IO;
using System.Net.Sockets;

namespace SerialPortListener
{
    
    public partial class MainForm : Form
    {
        SerialPortManager _spManager;
        public MainForm()
        {
            InitializeComponent();

            UserInitialization();
        }

      
        private void UserInitialization()
        {
            _spManager = new SerialPortManager();
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }

        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();   
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (tbData.TextLength > maxTextLength)
                tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

       
            Console.WriteLine(e.Data.Length.ToString());
            if (e.Data.Length != 29) return;
            int pan_e = _HexToInt(e.Data[2], e.Data[3], e.Data[4]);
            int tilt_e = _HexToInt(e.Data[5], e.Data[6], e.Data[7]);
            int zoom = _HexToInt(e.Data[20], e.Data[21], e.Data[22]);
            int focus = _HexToInt(e.Data[23], e.Data[24], e.Data[25]);
            float pan = (float)pan_e / 32768;
            float tilt = (float)tilt_e / 32768;
            float zoom_min = 524352;
            float zoom_max = 598444;
            float focus_min = 524447;
            float focus_max = 646137;
            float zoom_per = (zoom - zoom_min) / (zoom_max - zoom_min);
            float focus_per = (focus - focus_min) / (focus_max - focus_min);
            tbData.Text = "pan =" + pan + "\n " + "tilt=" + tilt;
            UpdateUI(tb_pan, dou2str(pan, "F", 2));
            UpdateUI(tb_tilt, dou2str(tilt, "F", 2));
            UpdateUI(tb_zoom, zoom.ToString() + "(" + dou2str(zoom_per * 100, "F", 1) + "%)");
            UpdateUI(tb_focus, focus.ToString() + "(" + dou2str(focus_per * 100, "F", 1) + "%)");
            // tbData.ScrollToCaret();

        }

        public string dou2str(float va, string specifier, int precision)
        {
            string pSpecifier= String.Format("{0}{1}", specifier, precision);
            return va.ToString(pSpecifier);
        }
        int _HexToInt(byte D1, byte D2, byte D3)
        {
            int I_DAT;
            I_DAT = (int)(SClass.RotateLeft(D1, 24) & 0xff000000 |
                         SClass.RotateLeft(D2, 16) & 0x00ff0000 |
                          SClass.RotateLeft(D3, 8) & 0x0000ff00);
            return I_DAT / 0x100;
        }


       


        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            _spManager.StartListening();
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            _spManager.StopListening();
        }


        static UdpClient c = new UdpClient(6700);
        //CInit cinit;
        //private void SendUdp(string cmd)
        //{
        //    try
        //    {
        //        UpdateUI(this.tbRunningStatus, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + "发送命令到在线引擎" + cmd + "\r\n");
        //        Byte[] sendBytes = Encoding.UTF8.GetBytes(cmd + "\0");
        //        c.Send(sendBytes, sendBytes.Length, cinit.VizEngineHost, Convert.ToInt32(cinit.VizEnginePort));
        //        if (cinit.useBackup == 1)
        //        {
        //            c.Send(sendBytes, sendBytes.Length, cinit.VizEngineHostBackup, Convert.ToInt32(cinit.VizEnginePort));
        //        }
        //        CLog.WriteLog(CInit.LogPath, DateTime.Now.ToString("yyyyMMdd") + "SendingLog", " SendUdp  " + cmd);
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateUI(this.tbRunningStatus, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + "SendUdp错误" + ex.ToString() + "\r\n");
        //        CLog.WriteLog(CInit.LogPath, DateTime.Now.ToString("yyyyMMdd") + "SendingLog", "SendUdp  " + ex.ToString());
        //    }
        //}
        //private void LoadScene(string scenePath)
        //{
        //    try
        //    {
        //        UpdateUI(this.tbRunningStatus, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + "加载场景" + scenePath + "\r\n");
        //        Byte[] sendBytes = Encoding.UTF8.GetBytes("0 REND*MAIN_LAYER SET_OBJECT " + scenePath + "\0");
        //        c.Send(sendBytes, sendBytes.Length, cinit.VizEngineHost, Convert.ToInt32(cinit.VizEnginePort));
        //        if (cinit.useBackup == 1)
        //        {
        //            c.Send(sendBytes, sendBytes.Length, cinit.VizEngineHostBackup, Convert.ToInt32(cinit.VizEnginePort));
        //        }
        //        CLog.WriteLog(CInit.LogPath, DateTime.Now.ToString("yyyyMMdd") + "SendingLog", " LoadScene  " + scenePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateUI(this.tbRunningStatus, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + "LoadScene错误" + ex.ToString() + "\r\n");
        //        CLog.WriteLog(CInit.LogPath, DateTime.Now.ToString("yyyyMMdd") + "SendingLog", "LoadScene  " + ex.ToString());
        //    }
        //}


        private void UpdateUI(Control o, object args)
        {
            if (o.InvokeRequired)
            {
                if (typeof(RichTextBox).Equals(o.GetType()))
                {
                    o.BeginInvoke(new EventHandler(delegate
                    {
                        ((RichTextBox)o).SelectionColor = Color.White;
                    }), args);
                    o.BeginInvoke(new EventHandler(delegate
                    {
                        ((RichTextBox)o).Text = ((RichTextBox)o).Text.Insert(0, args.ToString());
                    }), args);
                }
                else if (typeof(PictureBox).Equals(o.GetType()))
                {
                    o.BeginInvoke(new EventHandler(delegate
                    {
                        ((PictureBox)o).Image = (Image)args;
                    }), args);
                }
                else if (typeof(Label).Equals(o.GetType()))
                {
                    o.BeginInvoke(new EventHandler(delegate
                    {
                        ((Label)o).Text = args.ToString();
                    }), args);
                }
                else if (typeof(TextBox).Equals(o.GetType()))
                {
                    o.BeginInvoke(new EventHandler(delegate
                    {
                        ((TextBox)o).Text = args.ToString();
                    }), args);
                }
            }
            else
            {
                if (typeof(RichTextBox).Equals(o.GetType()))
                {
                    ((RichTextBox)o).Text = ((RichTextBox)o).Text.Insert(0, args.ToString());
                }
                else if (typeof(PictureBox).Equals(o.GetType()))
                {
                    ((PictureBox)o).Image = (Image)args;
                }
                else if (typeof(Label).Equals(o.GetType()))
                {
                    ((Label)o).Text = args.ToString();
                }
                else if (typeof(TextBox).Equals(o.GetType()))
                {
                    ((TextBox)o).Text = args.ToString();
                }
            }
        }
    }
}
