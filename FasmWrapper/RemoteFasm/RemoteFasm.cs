using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.Threading;
using System.Runtime.InteropServices;

namespace MFasm
{
    [Guid("46E7A2FF-CFB6-4A94-B41B-A09C6008690E")]
    public interface IFasmCOM
    {
        [OperationContract]
        byte[] Assemble(string szCode);

        [OperationContract]
        bool Heartbeat();
    }

    [Guid("39F67421-B36A-4A7C-BB65-94F6084CAA95"),
    ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IFasmCOM))]
    public class FasmCom : IFasmCOM
    {
        static FasmCom _FasmServiceInstance = null;
        static Process _FasmServiceProcess = null;
        static string ServiceName = "FasmService" + Process.GetCurrentProcess().Id;
        static EventWaitHandle ServiceAvailable = new EventWaitHandle(false, EventResetMode.ManualReset, "FasmService" + Process.GetCurrentProcess().Id);
        /*private static FasmCom Instance
        {
            get
            {
                if (_FasmServiceProcess == null || ((_FasmServiceProcess != null && _FasmServiceProcess.HasExited)))
                {
                    // Process does not exists or terminated

                    _FasmServiceProcess = new Process();
                    _FasmServiceProcess.StartInfo = new ProcessStartInfo(@"FasmService.exe");
                    ServiceAvailable.Reset();
                    if (!_FasmServiceProcess.Start()) throw new Exception("Error while launching FasmService.exe !");
                    // ждем инициализации сервера
                    ServiceAvailable.WaitOne(5000);
                }

                try
                {
                    // если все ок - мы получаем копию
                    _FasmServiceInstance = ChannelFactory<IFasmService>.CreateChannel(
                    new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                    new EndpointAddress(@"net.pipe://localhost/" + ServiceName));
                    _FasmServiceInstance.Heartbeat();
                }
                catch (Exception)
                {
                    ServiceAvailable.Reset();
                    throw new Exception("Something gone wrong with FasmService !");
                }
                

                return _FasmServiceInstance;
            }
        }*/

        private StringBuilder asmCode;

        public FasmCom()
        {
            this.Clear();
        }

        public void Clear()
        {
            asmCode = new StringBuilder("use32\n", 1024);
        }

        public void AddLine(string asmLine)
        {
            asmCode.AppendLine(asmLine);
        }

        public void AddLine(string asmLineFormat, params object[] args)
        {
            asmCode.AppendLine(String.Format(asmLineFormat,args));
        }       

        public byte[] Assemble()
        {
            return null;
        }

        public byte[] Assemble(string szCode)
        {
            throw new NotImplementedException();
        }

        public bool Heartbeat()
        {
            throw new NotImplementedException();
        }
    }
}
