using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Announcement_Bot_PRW
{
    [Serializable]
    public class Server
    {
        public string name { get; set; }
        public string prefix { get; set; }
        public ulong ID { get; set; }
        public bool Modlog { get; set; }
        public ulong Modlogid { get; set; }
        public List<string> ModLogReasons { get; set; }
    }
}
