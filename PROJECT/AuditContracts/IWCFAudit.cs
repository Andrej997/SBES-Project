using System.ServiceModel;

namespace AuditContracts
{
    [ServiceContract]
    public interface IWCFAudit
    {
        [OperationContract]
        string ConnectS(string msg);
    }
}
