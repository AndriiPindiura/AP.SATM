using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.SATM.Heart
{
    internal static class SessionStorage
    {
        private static Dictionary<string, string> Names { get; set; }

        /*public SessionStorage()
        {
            Names = new Dictionary<string, string>();
        }*/

        public static void Activate(string sessionId, string name)
        {
            if (Names == null)
            {
                Names = new Dictionary<string, string>();
            }
            Names[name] = sessionId;
        }

        public static  void Deactivate(string sessionId)
        {
            if (Names == null)
            {
                Names = new Dictionary<string, string>();
            }
            string name = (from n in Names where n.Value == sessionId select n.Key).FirstOrDefault();
            if (name == null)
                return;
            /*foreach (string result in Names.Keys)
            {
                if (Names.Values[result] == sessionId)
                //if (result == name) Names.Remove(name);
            }*/
            //Names.Remove()
            Names.Remove(name);
        }

        public static bool IsActive(string name, string suuid)
        {
            if (Names == null)
            {
                Names = new Dictionary<string, string>();
                return false;
            }
            if (Names.ContainsKey(name))
            {
                string suid = (from n in Names where n.Key == name select n.Value).FirstOrDefault();
                if (suuid == suid)
                {
                    return false;
                }
                else
                    return true;
            }
            else
            {
                return false;
            }
            //return Names.ContainsKey(name);
        }
    }
}
