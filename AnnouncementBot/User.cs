using System;
using System.Collections.Generic;

namespace AnnouncementBot
{
    [Serializable]
    public class User
    {
        public string username { get; set; }
        public ulong clientID { get; set; }
        public List<Warning> warnings { get; set; }
    }
}
