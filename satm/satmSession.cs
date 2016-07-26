using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;


namespace satm
{
    public class satmSession : ServiceAuthorizationManager
    {
        private satmSessionStorage Storage { get; set; }

        public satmSession()
        {
            Storage = new satmSessionStorage();
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            string name = operationContext.ServiceSecurityContext.PrimaryIdentity.Name;

            if (Storage.IsActive(name))
                return false;
            Storage.Activate(operationContext.SessionId, name);
            operationContext.Channel.Closed += new EventHandler(Channel_Closed);
            return true;
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Storage.Deactivate((sender as IContextChannel).SessionId);
        }    
    }
}
