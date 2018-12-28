using ImpromptuInterface;
using System.Collections.Concurrent;

namespace Coldairarrow.DotNettyRPC
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class RPCClientFactory
    {
        private static ConcurrentDictionary<string, object> _services { get; } = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 获取客户端
        /// 注：默认服务名为接口名
        /// </summary>
        /// <typeparam name="T">接口定义类型</typeparam>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns></returns>
        public static T GetClient<T>(string serverIp, int port) where T : class
        {
            return GetClient<T>(serverIp, port, typeof(T).Name);
        }

        /// <summary>
        /// 获取客户端
        /// 注：自定义服务名
        /// </summary>
        /// <typeparam name="T">接口定义类型</typeparam>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <param name="serviceName">服务名</param>
        /// <returns></returns>
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
