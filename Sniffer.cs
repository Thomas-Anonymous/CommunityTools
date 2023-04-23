using SharpPcap;
using SharpPcap.LibPcap;

namespace CommunityTools {
    class Sniffer {

        Packets packets;
        public ILiveDevice device;

        public Sniffer(ILiveDevice _device) {
            packets = new Packets();
            device = _device;
        }

        public void Lecture() {
            device.OnPacketArrival += new PacketArrivalEventHandler(packets.device_OnPacketArrival);
            device.Open(DeviceModes.MaxResponsiveness);
            device.Filter = "ip host " + Primaire.ipSniff;
            try { device.Capture(); } catch { }
        }
    }
}
