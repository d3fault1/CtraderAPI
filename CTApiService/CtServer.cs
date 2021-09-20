using System;
using System.ServiceModel;

namespace CTApiService
{
    internal class CtServer
    {
        private readonly ICtApi _service;
        private ServiceHost serviceHost;

        public bool IsOpen
        {
            get
            {
                return serviceHost != null ? serviceHost.State == CommunicationState.Opened : false;
            }
        }

        public CtServer(ICtApi service)
        {
            _service = service;
        }

        public void Start(int port)
        {
            //init localhost by Socket
            var localUrl = "net.pipe://localhost/MtApiService_" + port;

            try
            {
                serviceHost = new ServiceHost(_service);

                //Log Debug by Pipe
                var bind = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647,
                    SendTimeout = new TimeSpan(12, 0, 0),
                    ReceiveTimeout = new TimeSpan(12, 0, 0),
                    ReaderQuotas =
                    {
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 2147483647,
                        MaxDepth = 2147483647,
                        MaxStringContentLength = 2147483647,
                        MaxNameTableCharCount = 2147483647
                    }
                };
                //Log Debug
                serviceHost.AddServiceEndpoint(typeof(ICtApi), bind, localUrl);
                serviceHost.Open();
            }
            catch
            {
                //Log error
                serviceHost = null;
            }
            //Log debug
        }

        public void Stop()
        {
            if (serviceHost != null)
            {
                if (serviceHost.State == CommunicationState.Opened)
                {
                    //Log.Debug("Stop: begin.");
                    try
                    {
                        serviceHost.Close();
                    }
                    catch (TimeoutException ex)
                    {
                        //Log.ErrorFormat("Stop: TimeoutException - {0}", ex.Message);
                        serviceHost.Abort();
                    }
                }
            }
            //Log.Debug("Stop: end.");
        }

    }
}
