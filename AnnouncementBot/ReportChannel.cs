using System;

namespace AnnouncementBot
{
    [Serializable]
    public class ReportChannel
    {
        public ulong channelID { get; set; }
        public string channelName { get; set; }
        public string type { get; set; }
    }
}
