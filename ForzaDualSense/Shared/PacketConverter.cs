using System.Net;
using Newtonsoft.Json;

namespace ForzaDualSense.Shared
{
    //Needed to communicate with DualSenseX
    public static class PacketConverter
    {        
        public static string PacketToJson(Packet packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static Packet JsonToPacket(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
