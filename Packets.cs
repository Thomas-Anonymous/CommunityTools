using System;
using SharpPcap;
using System.Text;
using PacketDotNet;
using System.Net.NetworkInformation;
using System.Linq;

namespace CommunityTools {
    public class Packets {

        byte[] mac;
        byte[] packet;

        public Packets() {
            Primaire.traite.Start();
            mac = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
                .FirstOrDefault();
        }

        public void device_OnPacketArrival(object sender, PacketCapture pack) {
            //string test = BitConverter.ToString(pack.Data.ToArray());
            packet = pack.Data.ToArray();

            // ### Base: ###
            byte[] firstMac = getB(6); // MAC du receveur
            byte[] secondMac = getB(6); // MAC de l'envoyeur
            byte[] IPV = getB(2); // IPV4 / IPV6

            // ### Protocole : ###
            byte[] protocole = getB(20); // On passe la partie protocole

            // ### Control Protocole : ###
            byte[] controlProtocole = getB(20); // On passe la partie controle protocole


            if (mac.SequenceEqual(firstMac)) { // Si le receveur c'est nous ça vient du serveur
                Primaire.traite.addMSReceive(Encoding.ASCII.GetString(packet));
            } else if (mac.SequenceEqual(secondMac)) { // Sinon c'est nous qui l'envoyons
                Primaire.traite.addMSWrite(Encoding.ASCII.GetString(packet));
            }
        }

        byte[] getB(int nbr) {
            byte[] temp = packet.Take(nbr).ToArray();
            packet = packet.Skip(nbr).ToArray();
            return temp;
        }
    }
}
