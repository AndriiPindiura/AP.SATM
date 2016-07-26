using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace AP.SATM.Heart
{

    [ServiceContract(SessionMode=SessionMode.Required)]
    public interface ICore
    {
        [OperationContract(IsInitiating=true)]
        bool SignIn();

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        bool SignOut();

        [OperationContract(IsInitiating = false)]
        string ARM();

        [OperationContract(IsInitiating = false)]
        void HeartBeat();

        [OperationContract(IsInitiating = false)]
        List<Events> GetEvents(List<Entries> entries, DateTime from, DateTime to, bool raw);

        [OperationContract(IsInitiating = false)]
        List<Events> GetEventsQuery(List<Entries> entries, DateTime from, DateTime to, bool raw);

        [OperationContract(IsInitiating = false)]
        List<Entries> GetEntries();

        [OperationContract(IsInitiating = false)]
        List<Cores> GetCores();

        [OperationContract(IsInitiating = false)]
        void UpdateEvent(string core, string uid);

    }

    /*[DataContract]
    public class satmData
    {
        [DataMember]
        public string owner { get; set; }
        [DataMember]
        public string entry { get; set; }
        [DataMember]
        public string direction { get; set; }
        [DataMember]
        public DateTime startDate { get; set; }
        [DataMember]
        public DateTime endDate { get; set; }
        [DataMember]
        public string ttn { get; set; }
        [DataMember]
        public string carID { get; set; }
        [DataMember]
        public string culture { get; set; }
        [DataMember]
        public string who { get; set; }
        [DataMember]
        public bool legal { get; set; }
        [DataMember]
        public string uid { get; set; }
        [DataMember]
        public string user { get; set; }
        [DataMember]
        public string ip { get; set; }
        [DataMember]
        public int upCam { get; set; }
        [DataMember]
        public DateTime upTime { get; set; }
        [DataMember]
        public int enterCam { get; set; }
        [DataMember]
        public DateTime enterTime { get; set; }
        [DataMember]
        public int exitCam { get; set; }
        [DataMember]
        public DateTime exitTime { get; set; }
    }*/
}
