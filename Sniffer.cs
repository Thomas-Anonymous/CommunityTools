using SharpPcap;
using SharpPcap.LibPcap;

namespace Test_Sniffeur {
    class Sniffer {

        Packets packets;
        public ICaptureDevice device;


        Primaire _i;
        public Sniffer(Primaire instance, ICaptureDevice _device) {
            _i = instance;
            packets = new Packets(instance);
            device = _device;
        }

        public void Lecture() {
            device.OnPacketArrival += new PacketArrivalEventHandler(packets.device_OnPacketArrival);            
            device.Open(DeviceModes.MaxResponsiveness);
            device.Filter = "ip host " + _i.ipSniff;
            device.Capture();
        }
    }
}
