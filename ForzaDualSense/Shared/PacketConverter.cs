using Newtonsoft.Json;

namespace ForzaDualSense.Shared
{
    //Needed to communicate with DualSenseX
    public static class PacketConverter
    {
        //Parses data from Forza into a DataPacket
                
        public static string PacketToJson(DSXInstructions packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static DSXInstructions JsonToPacket(string json)
        {
            return JsonConvert.DeserializeObject<DSXInstructions>(json);
        }
    }
}
