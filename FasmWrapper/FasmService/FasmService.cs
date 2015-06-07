using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;
using Fasm;
using System.Threading;

namespace Fasm
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single,IncludeExceptionDetailInFaults = true)]
    public class FasmService : IFasmService
    {
        public FasmService()
        {

        }



        public byte[] Assemble(string szCode)
        {
            lock (this)
            {
                ManagedFasm fasm = new ManagedFasm();
                fasm.Add(szCode);
                return fasm.Assemble();
            }
        }

        bool IFasmService.Heartbeat()
        {
            return true;
        }
    }    

    class Program
    {
        static ServiceHost host = null;
        static EventWaitHandle ServiceAvailable;
        static string ServiceName;

        static void Main(string[] args)
        {
            try
            {
                Process ParentProcess = Native.ParentProcessUtilities.GetParentProcess(Process.GetCurrentProcess().Id);
                ServiceName = "FasmService" + ParentProcess.Id;
                host = new ServiceHost(new FasmService());
                host.AddServiceEndpoint(typeof(IFasmService), 
                    new NetNamedPipeBinding(NetNamedPipeSecurityMode.None), 
                    new Uri(@"net.pipe://localhost/"+ServiceName));

                ServiceThrottlingBehavior throttling = new ServiceThrottlingBehavior();
                throttling.MaxConcurrentCalls = 1000;
                throttling.MaxConcurrentInstances = 1000;
                throttling.MaxConcurrentSessions = 1000;
                host.Description.Behaviors.Add(throttling);

                host.Open();
                if (host.State != CommunicationState.Opened) throw new Exception("Could not create FasmService");

                try
                {
                    ServiceAvailable = EventWaitHandle.OpenExisting(ServiceName);
                }
                catch (System.Threading.WaitHandleCannotBeOpenedException)
                { // что-то пошло не так и нет ивента запустившего нас приложения -> работать нет смысла
                    throw new Exception("Something gone wrong with sync. event !");
                }
                
                ServiceAvailable.Set();
                while (!ParentProcess.HasExited)
                {
                    System.Threading.Thread.Sleep(500);
                }
                host.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (host != null && host.State == CommunicationState.Opened) host.Abort(); 
            }
            
        }
    }
}
