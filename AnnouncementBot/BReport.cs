using System;
using System.Collections.Generic;

namespace AnnouncementBot
{
    [Serializable]
    public class BReport
    {
        public int modlogCount { get; set; }
        public int guildCount { get; set; }
        public int customPrefixes { get; set; }
        public int normalPrefixes { get; set; }
        public int totalUsers { get; set; }
        public int onlineUsers { get; set; }
        public int offlineUsers { get; set; }
        public int afkUsers { get; set; }
        public int dndUsers { get; set; }
        public int idleUsers { get; set; }
        public int totalChannels { get; set; }
        public int textChannels { get; set; }
        public int voiceChannels { get; set; }
        public int roles { get; set; }
        public List<ReportChannel> textChannelList { get; set; }
        public List<ReportChannel> voiceChannelList { get; set; }
        public List<ReportRole> roleList { get; set; }
        public List<ReportUser> userList { get; set; }
    }
}
