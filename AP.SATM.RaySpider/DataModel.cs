using System;
using System.Collections.Generic;

namespace AP.SATM.RaySpider
{
    public class IntellectOutput
    {
        public string owner
        {
            get;
            set;
        }
        public string objId
        {
            get;
            set;
        }
        public bool action
        {
            get;
            set;
        }
        public DateTime date
        {
            get;
            set;
        }
    }

    public class IOConfiguration
    {
        public string deviceIp { get; set; }
        public int ioId { get; set; }
        public int rayId { get; set; }
        public bool ioState { get; set; }
    }

    static class Variables
    {
        public static List<IOConfiguration> Config
        {
            get { return config; }
            set { config = value; }
        }
        public static List<IntellectOutput> Buffer
        {
            get { return buffer; }
            set { buffer = value; }
        }

        private static List<IOConfiguration> config = new List<IOConfiguration>();
        private static List<IntellectOutput> buffer = new List<IntellectOutput>();
    }

    /*
    public class Protocol
    {
        public string objtype { get; set; }

        public string objid { get; set; }

        public string action { get; set; }

        public string region_id { get; set; }

        public string param0 { get; set; }

        public string param1 { get; set; }

        public string param2 { get; set; }

        public string param3 { get; set; }

        public float? user_param_double { get; set; }

        public DateTime? date { get; set; }

        public DateTime? time { get; set; }

        public DateTime? time2 { get; set; }

        public string owner { get; set; }

        public Guid pk { get; set; }

    }
    */
}
