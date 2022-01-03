using System;
using SharpPcap;
using System.Text;
using PacketDotNet;
using System.Collections.Generic;

namespace Test_Sniffeur {
    class Packets {

        TraitementPackets traite;

        Primaire _i;
        public Packets(Primaire instance) {
            _i = instance;
            traite = new TraitementPackets(instance);
        }

        public void device_OnPacketArrival(object sender, PacketCapture packet) {
            Packet parsePacket = Packet.ParsePacket(packet.GetPacket().LinkLayerType, packet.GetPacket().Data);
            byte[] getPacket = ((IPPacket)((EthernetPacket)parsePacket).PayloadPacket).PayloadPacket.PayloadData;

            if (getPacket.Length > 0 || (getPacket.Length == 1 && getPacket[0] != 0x00)) {
                string ipDestination = ((IPv4Packet)((EthernetPacket)parsePacket).PayloadPacket).DestinationAddress.ToString();
                if (ipDestination == _i.ipSniff) traite.TraitementEnvoie(getPacket); else traite.TraitementRecu(getPacket);
            }
        }
    }
}
