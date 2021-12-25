using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace Test_Sniffeur {
    public partial class Form1 : Form {

        ICaptureDevice device;
        int numPacket = 1;

        public Form1() {
            InitializeComponent();
            // Pour le moment on test donc osef des respects des threads
            Control.CheckForIllegalCrossThreadCalls = false;

            // La création de mon thread
            Thread _th = new Thread(bcl);
            _th.Start();
        }

        // La boucle de notre thread
        void bcl() {
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            foreach (LibPcapLiveDevice _devs in devices) if (_devs.Interface.FriendlyName == "Ethernet") device = _devs;
            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

            device.Open(DeviceModes.MaxResponsiveness);
            device.Filter = "src host 172.65.226.26"; // dst host 172.65.226.26 or src host 172.65.226.26
            device.Capture();
        }

        void device_OnPacketArrival(object sender, PacketCapture packet) {
            byte[] getPacket = parsePacket(Packet.ParsePacket(packet.GetPacket().LinkLayerType, packet.GetPacket().Data).Bytes);
            numPacket++;
            if (getPacket.Length > 0) {
                richTextBox1.AppendText(BitConverter.ToString(getPacket).Replace("-", " ") + "\n\n");
            }
        }

        byte[] parsePacket(byte[] packet) {
            int retraitInfos = 54;
            if (numPacket < 2) retraitInfos += 12;
            if (packet.Length <= retraitInfos) return new byte[] { };
            byte[] result = new byte[packet.Length - retraitInfos];
            Array.Copy(packet, retraitInfos, result, 0, packet.Length - retraitInfos);
            return result;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            device.StopCapture();
            device.Close();
        }
    }
}