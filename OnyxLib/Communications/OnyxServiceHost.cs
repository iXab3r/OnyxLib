#region Usings

using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;

using XMLib;
using XMLib.Log;

#endregion

namespace OnyxLib.Communications
{
    public class OnyxServiceHost<T>
        where T : class
    {
        private readonly T m_instance;

        private ServiceHost m_host;

        private Process m_currentProcess = Process.GetCurrentProcess();

        public OnyxServiceHost(T _instance)
        {
	        if (_instance == null)
	        {
		        throw new ArgumentNullException(nameof(_instance));
	        }
	        if (!typeof(T).IsInterface)
            {
                throw new ArgumentException(string.Format("Expected {0} to be of Interface type", typeof(T)));
            }
            m_instance = _instance;
        }

        /// <summary>
        ///   Generates URI for chosone ProcessId and InterfaceType
        /// </summary>
        /// <param name="_processId"></param>
        /// <returns></returns>
        public static string GeneratePipeUri(int _processId)
        {
            return String.Format("net.pipe://localhost/{0}", GenerateServiceName(_processId));
        }

        public static string GenerateServiceName(int _processId)
        {
            return String.Format("OnyxLib/{0}/{1}", _processId, typeof(T).Name);
        }

        public static string GenerateHttpUri(int _processId)
        {
            return String.Format("http://localhost:8004/{0}", GenerateServiceName(_processId));
        }

        /// <summary>
        ///   Creates appropriate binding for OnyxServiceHost
        /// </summary>
        /// <returns></returns>
        public static Binding GeneratePipeBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        }

        /// <summary>
        ///     Starts WCF host in process
        /// </summary>
        /// <returns>Launcher WCF Service name</returns>
        public void Start()
        {
            if (m_host != null)
            {
                throw new InvalidOperationException("Server already started");
            }

            m_host = new ServiceHost(m_instance);

            // named-pipe 
            var pipeUri = GeneratePipeUri(m_currentProcess.Id);
            var httpUri = GenerateHttpUri(m_currentProcess.Id);
            Logger.InfoFormat("Starting WCFServer @ {0} + {1}...", pipeUri, httpUri);
            m_host.AddServiceEndpoint(typeof(T), GeneratePipeBinding(), pipeUri);

            var httpBinding = new WebHttpBinding(WebHttpSecurityMode.None);//; new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.None, Encoding.UTF8), new HttpTransportBindingElement());
            var webEndpoint = m_host.AddServiceEndpoint(typeof(T), httpBinding, httpUri);
            webEndpoint.Behaviors.Add(new WebHttpBehavior()
            {
                FaultExceptionEnabled = true,
                HelpEnabled = true,
            });
            m_host.Description.Behaviors.Add(new ServiceMetadataBehavior()
            {
                HttpGetUrl = new Uri(httpUri),
                HttpGetEnabled = true,
            });
            m_host.Description.Behaviors.Add(new ServiceThrottlingBehavior { MaxConcurrentCalls = 1000, MaxConcurrentInstances = 1000, MaxConcurrentSessions = 1000 });
            m_host.Open();
            Logger.InfoFormat("Listening @ {0} + {1}", pipeUri,httpUri);
        }

        /// <summary>
        ///   Terminates WCF host
        /// </summary>
        public void Stop()
        {
            if (m_host != null)
            {
                m_host.Abort();
            }
            m_host = null;
        }
        #region IOnyxManagerContract implementation


        #endregion
    }
}