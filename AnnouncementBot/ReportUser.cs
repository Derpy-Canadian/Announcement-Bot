using System;

namespace AnnouncementBot
{
    [Serializable]
    public class ReportUser
    {
        public string username { get; set; }
        public ulong CID { get; set; }
        public string avatarURL { get; set; }
        public bool isBot { get; set; }
    }
}
