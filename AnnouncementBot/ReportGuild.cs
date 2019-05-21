using System;

namespace AnnouncementBot
{
    [Serializable]
    public class ReportGuild
    {
        public string guildName { get; set; }
        public ulong guildID { get; set; }
        public string guildIconURL { get; set; }
        public int guildChannelCount { get; set; }
    }
}
