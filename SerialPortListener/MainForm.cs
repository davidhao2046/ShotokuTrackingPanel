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
           
            int pan_e = _HexToInt(e.Data[2], e.Data[3], e.Data[4]);
            int tilt_e = _HexToInt(e.Data[5], e.Data[6], e.Data[7]);
            float pan = (float)pan_e / 32768;
            float tilt = (float)tilt_e / 32768;

          
            Console.WriteLine(" ");
          // Console.WriteLine(e.Data.Length.ToString());
           // string str1=BitConverter.ToString(Bytes);
            tbData.Text="pan =" + pan+"\n "+"tilt="+tilt;
           // tbData.ScrollToCaret();

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
    }
}
