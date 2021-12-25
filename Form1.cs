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
        Thread _th;
        int numPacket = 1;

        public Form1() {
            InitializeComponent();
            // Pour le moment on test donc osef des respects des threads
            Control.CheckForIllegalCrossThreadCalls = false;

            // La création de mon thread
            _th = new Thread(bcl);
            _th.Start();
        }

        // La boucle de notre thread
        void bcl() {
            CaptureDeviceList devices = CaptureDeviceList.Instance; // On récupère les connexions actives
            foreach (LibPcapLiveDevice _devs in devices) if (_devs.Interface.FriendlyName == "Ethernet") device = _devs; // On cherche la connexion Ethernet
            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival); // On ajoute un évènement quand un packet est reçu

            device.Open(DeviceModes.MaxResponsiveness); // On ouvre la lecture de nos packets
            // dst host 172.65.226.26 or src host 172.65.226.26
            device.Filter = "src host 172.65.226.26"; // On filtre uniquement les packets venant du serveur Boune
            device.Capture(); // On lance la capture des packets
        }

        // Quand un packet est reçu ...
        void device_OnPacketArrival(object sender, PacketCapture packet) {
            // On récupère le packet et on parse la partie inutile (informations d'envoie)
            byte[] getPacket = parsePacket(Packet.ParsePacket(packet.GetPacket().LinkLayerType, packet.GetPacket().Data).Bytes);
            numPacket++;
            if (getPacket.Length > 0) {
                richTextBox1.AppendText(BitConverter.ToString(getPacket).Replace("-", " ") + "\n\n");
            }
        }

        // Pour retirer les infos de liaison avec le serveur
        byte[] parsePacket(byte[] packet) {
            int retraitInfos = 54; // Nombre de bytes inutiles (fixe ?)
            if (numPacket < 2) retraitInfos += 12; // 12 bytes supplémentaire sur le premier packet à retiré
            if (packet.Length <= retraitInfos) return new byte[] { }; // Si on est négatif ou vide on renvoie rien
            byte[] result = new byte[packet.Length - retraitInfos]; // On prépare le nouveau tableau pour récupérer l'échange
            Array.Copy(packet, retraitInfos, result, 0, packet.Length - retraitInfos); // on copie les informations reçu
            return result; // et on retourne le tableau
        }

        // Quand on ferme le logiciel
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            _th.Abort(); // On coupe le thread
            device.StopCapture(); // On coupe la capture
            device.Close(); // On ferme la capture
        }
    }
}