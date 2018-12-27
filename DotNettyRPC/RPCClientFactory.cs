using ImpromptuInterface;
using System.Collections.Concurrent;

namespace Coldairarrow.DotNettyRPC
{
    public class RPCClientFactory
    {
        private static ConcurrentDictionary<string, object> _services { get; } = new ConcurrentDictionary<string, object>();
        public static T GetClient<T>(string serverIp, int port) where T : class
        {
            return GetClient<T>(serverIp, port, typeof(T).Name);
        }

        public static T GetClient<T>(string serverIp, int port, string serviceName) where T : class
        {
            T service = null;
            string key = $"{serviceName}-{serverIp}-{port}";
            try
            {
                service = (T)_services[key];
            }
            catch
            {
                var clientProxy = new RPCClientProxy
                {
                    ServerIp = serverIp,
                    ServerPort = port,
                    ServiceType = typeof(T),
                    ServiceName = serviceName
                };
                service = clientProxy.ActLike<T>();
                _services[key] = service;
            }

            return service;
        }
    }
}
