using System;
using System.Collections.Generic;

namespace CTApiService
{
    public class CtAdapter
    {
        private static readonly CtAdapter Instance = new CtAdapter();
        private Random randGen = new Random();

        private readonly Dictionary<int, CtServer> _servers = new Dictionary<int, CtServer>();

        #region Init
        private CtAdapter()
        {

        }

        static CtAdapter()
        {

        }

        public static CtAdapter GetInstance()
        {
            return Instance;
        }
        #endregion

        public int AddBot(int port, ICtApi bot)
        {
            CtServer server;
            lock (_servers)
            {
                while (_servers.ContainsKey(port)) port = randGen.Next(1000, 9999);

                server = new CtServer(bot);
                _servers.Add(port, server);
                server.Start(port);
                return port;
            }
        }

        public void RemoveBot(int port)
        {
            CtServer server;
            lock (_servers)
            {
                if (_servers.ContainsKey(port))
                {
                    server = _servers[port];
                    server.Stop();
                    _servers.Remove(port);
                }
            }
        }
    }
}
