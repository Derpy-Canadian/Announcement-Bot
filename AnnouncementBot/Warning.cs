using System;
namespace AnnouncementBot
{
    [Serializable]
    public class Warning
    {
        public string warnReason { get; set; }
        public int warnNum { get; set; }
        public ulong warnUserID { get; set; }
        public ulong warnAdminID { get; set; }
        public ulong warnGuildID { get; set; }
    }
}
