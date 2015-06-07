using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;

namespace Fasm
{
    [ServiceContract]

    public interface IFasmService
    {
        [OperationContract]
        byte[] Assemble(string szCode);

        [OperationContract]
        bool Heartbeat();
    }
}
