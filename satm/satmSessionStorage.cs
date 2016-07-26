using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace satm
{
        public class satmSessionStorage
        {
            private Dictionary<string, string> Names { get; set; }

            public satmSessionStorage()
            {
                Names = new Dictionary<string, string>();
            }

            public void Activate(string sessionId, string name)
            {
                Names[name] = sessionId;
                File.AppendAllText(@"c:\sessions.log", "new session: " + Names[name].ToString());
            }

            public void Deactivate(string sessionId)
            {
                string name = (from n in Names where n.Value == sessionId select n.Key).FirstOrDefault();
                if (name == null)
                    return;
                
                File.AppendAllText(@"c:\sessions.log", "close session: " + Names[name].ToString());
                Names.Remove(name);
            }

            public bool IsActive(string name)
            {
                return Names.ContainsKey(name);
            }
        }
    
}
