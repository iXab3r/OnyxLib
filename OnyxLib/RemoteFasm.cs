#region Usings

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading;

#endregion

namespace OnyxLib
{
    [ServiceContract]
    public interface IFasmService
    {
        [OperationContract]
        byte[] Assemble(string szCode);

        [OperationContract]
        bool Heartbeat();
    }

    public class RemoteFasm
    {
        public static readonly string ServiceName = "FasmService" + Process.GetCurrentProcess().Id;

        private static EventWaitHandle serviceAvailable = new EventWaitHandle(false, EventResetMode.ManualReset, "FasmService" + Process.GetCurrentProcess().Id);

        private static Process _fasmServiceProcess = null;

        private static FasmWrapper.Fasm m_fasmInstance = new FasmWrapper.Fasm();

        private StringBuilder _asmCode;

        private byte[] _assembledBytes;

        public RemoteFasm()
        {
            Clear();
        }

        public string AsmCode
        {
            get
            {
                return _asmCode.ToString();
            }
            set
            {
                _asmCode = new StringBuilder(value);
            }
        }

        public byte[] AssembledBytes
        {
            get
            {
                if (_assembledBytes == null)
                {
                    _assembledBytes = Assemble();
                }
                return _assembledBytes;
            }
        }

        private static byte[] GetAssembledBytesFromFasmService(string szAsmCode)
        {
            lock (ServiceName)
            {
                if (_fasmServiceProcess == null || ((_fasmServiceProcess != null && _fasmServiceProcess.HasExited)))
                {
                    // если сервис не существует - запускаем его
                    _fasmServiceProcess = new Process();
                    _fasmServiceProcess.StartInfo = new ProcessStartInfo(@"FasmService.exe");
                    serviceAvailable.Reset();
                    if (!_fasmServiceProcess.Start())
                    {
                        throw new Exception("Error while launching FasmService.exe !");
                    }
                    // ждем инициализации сервис
                    if (!serviceAvailable.WaitOne(5000))
                    {
                        throw new Exception("Error while communicating with FasmService !");
                    }
                }
                try
                {
                    // если все ок - мы получаем копию
                    var fasmServiceInstance = ChannelFactory<IFasmService>.CreateChannel(
                        new NetNamedPipeBinding(NetNamedPipeSecurityMode.None),
                        new EndpointAddress(@"net.pipe://localhost/" + ServiceName));
                    return fasmServiceInstance.Assemble(szAsmCode);
                }
                catch (FaultException)
                {
                    serviceAvailable.Reset();
                    throw;
                }
                catch (Exception)
                {
                    serviceAvailable.Reset();
                    throw new Exception("Something gone wrong with FasmService !");
                }
            }
        }

        private static byte[] GetAssembledBytesFromAssembly(string szAsmCode)
        {
            return m_fasmInstance.Assemble(szAsmCode);
        }

        public void Clear()
        {
            _asmCode = new StringBuilder("", 1024);
        }

        public void AddLine(string asmLine)
        {
            _asmCode.AppendLine(asmLine);
        }

        public void AddLine(string asmLineFormat, params object[] args)
        {
            _asmCode.AppendLine(String.Format(asmLineFormat, args));
        }

        public byte[] Assemble()
        {
            return Assemble(_asmCode.ToString());
        }

        /// <summary>
        ///     Assembes mnemonics in AsmCode into bytes using either FasmService.exe for 64bit apps or ManagedFasm.dll for 32bit
        /// </summary>
        /// <param name="szAsmCode"></param>
        /// <returns></returns>
        public byte[] Assemble(string szAsmCode)
        {
            if (IntPtr.Size == 4) // 32 bit
            {
                _assembledBytes = GetAssembledBytesFromAssembly(szAsmCode);
            } else
            {
                _assembledBytes = GetAssembledBytesFromFasmService(szAsmCode);
            }

            return _assembledBytes;
        }

        public bool Inject(IntPtr hProcess, IntPtr injectionAddress)
        {
            var asmCode = _asmCode.ToString();
            var is64bitProcess = OnyxNative.Is64bitProcess(hProcess);
            if (hProcess == IntPtr.Zero)
            {
                throw new ArgumentException("Wrong process handle !");
            }
            if (injectionAddress == IntPtr.Zero)
            {
                throw new ArgumentException("Bad injection address !");
            }
            if (!asmCode.Contains("org "))
            {
                asmCode = asmCode.Insert(0, String.Format("org 0x{0:X08}\n", (long)injectionAddress));
            }
            if (!(asmCode.Contains("use32") || (asmCode.Contains("use64"))))
            {
                asmCode = asmCode.Insert(0, is64bitProcess ? "use64" : "use32");
            }
            if (is64bitProcess && asmCode.Contains("use32"))
            {
                throw new ArgumentException("Target process is 64bit, but you're trying to compile using use32 parameter !", "use32/64 switch");
            }
            if (!is64bitProcess && asmCode.Contains("use64"))
            {
                throw new ArgumentException("Target process is 32bit, but you're trying to compile using use64 parameter !", "use32/64 switch");
            }
            Assemble(asmCode);
            OnyxMemory.WriteBytes(hProcess, injectionAddress, _assembledBytes);
            return true;
        }

        public void InjectAndExecute(IntPtr hProcess, IntPtr injectionAddress)
        {
            Inject(hProcess, injectionAddress);
            var threadId = IntPtr.Zero;
            var hThread = OnyxNative.CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0,
                injectionAddress,
                IntPtr.Zero,
                ThreadFlags.THREAD_EXECUTE_IMMEDIATELY,
                out threadId);
            //wait for thread handle to have signaled state
            //exit code will be equal to the base address of the dll
            if (OnyxNative.WaitForSingleObject(hThread, 5000) == WaitValues.WAIT_OBJECT_0)
            {
            }
            OnyxNative.CloseHandle(hThread);
        }
    }
}