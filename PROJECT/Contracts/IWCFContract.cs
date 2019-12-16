using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public enum PovratnaVrijednost
    {
        USPJEH = 0,
        VECOTV = 1,
        NEMADOZ = 2,
        DOS = 3,
        NIJEOTV = 4
    }
    [ServiceContract]
    public interface IWCFContract
    {
        [OperationContract]
        string Connect();

        [OperationContract]
        PovratnaVrijednost OpenApp(byte[] encrypted);

        [OperationContract]
        PovratnaVrijednost CloseApp(byte[] encrypted);

        [OperationContract]
        bool IsBlackListValid();

        [OperationContract]
        byte[] ReturnBlackList();

        [OperationContract]
        bool EditBlackList(byte[] crypted);
    }
}
