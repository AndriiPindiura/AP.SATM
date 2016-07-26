using System.ServiceModel;
using System.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.IdentityModel.Claims;

namespace AP.SATM.Heart
{
    class Auth : ServiceAuthorizationManager

    {
        //private SessionStorage Storage { get; set; }

        public Auth()
        {
            //Storage = new SessionStorage();
        }

        private void EoFSession(object sender, EventArgs e)
        {
            //SessionStorage Storage = new SessionStorage();
            EventLog.WriteEntry("SATM Core", "Close Session ID: " + ((IContextChannel)sender).SessionId);
            SessionStorage.Deactivate(((IContextChannel)sender).SessionId);

        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
         
        #if (DEBUG)
            return true;
        #else
        {
            if (operationContext.ServiceSecurityContext.AuthorizationContext.ClaimSets == null) 
                throw new SecurityException ("No claimset service configured wrong");
            if (operationContext.ServiceSecurityContext.AuthorizationContext.ClaimSets.Count <= 0) 
                throw new SecurityException ("No claimset service configured wrong");
            var cert = ((X509CertificateClaimSet) operationContext.ServiceSecurityContext.AuthorizationContext.ClaimSets[0]).X509Certificate;

            if (SessionStorage.IsActive(cert.Subject, operationContext.SessionId))
            {
                //if Storage.
                //throw new FaultException("This user is currently connected by another instance!");
                return false;
            }
            System.Diagnostics.EventLog.WriteEntry("SATM Core", "New Session. User: " + cert.Subject + " ID: " + operationContext.SessionId);
            SessionStorage.Activate(operationContext.SessionId, cert.Subject);
            //OperationContext.Current.Channel.Closed += EoFSession;
            //operationContext.Channel.Closed += new EventHandler(EoFSession);
            //operationContext.Channel.Faulted += new EventHandler(EoFSession);
            //OperationContext.Current.Channel.Faulted += new EventHandler(EoFSession);
            //OperationContext.Current.Channel.Closed += new EventHandler(EoFSession);
            return true;
        }
#endif
        }

    }

}
