using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AP.SATM.Frontend.Models
{
    [DataContract]
    public class LayoutModel
    {
        [DataMember]
        public int ocxWidth;

        [DataMember]
        public int ocxHeight;
    }
}
