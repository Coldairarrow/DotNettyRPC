using Coldairarrow.Util;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;

namespace Coldairarrow.DotNettyRPC
{
    /// <summary>
    /// RPC服务端
    /// </summary>
    public class RPCServer
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听端口</param>
        public RPCServer(int port)
        {
            _port = port;

            _serverBootstrap = new ServerBootstrap()
                .Group(new MultithreadEventLoopGroup(), new MultithreadEventLoopGroup())
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new ServerHandler(this));
                }));
        }

        #endregion

        #region 私有成员

        private int _port { get; set; }
        private Dictionary<string, Type> _serviceHandle { get; set; } = new Dictionary<string, Type>();
        ServerBootstrap _serverBootstrap { get; }
        IChannel _serverChannel { get; set; }
        internal ResponseModel GetResponse(RequestModel request)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                var requestModel = request;
                if (!_serviceHandle.ContainsKey(requestModel.ServiceName))
                    throw new Exception("未找到该服务");
                var serviceType = _serviceHandle[requestModel.ServiceName];
                var service = Activator.CreateInstance(serviceType);
                var method = serviceType.GetMethod(requestModel.MethodName);
                if (method == null)
                    throw new Exception("未找到该方法");
                var paramters = requestModel.Paramters.ToArray();
                var methodParamters = method.GetParameters();
                for (int i = 0; i < methodParamters.Length; i++)
                {
                    if (paramters[i].GetType() != methodParamters[i].ParameterType)
                    {
                        paramters[i] = paramters[i].ToJson().ToObject(methodParamters[i].ParameterType);
                    }
                }
                var res = method.Invoke(service, paramters);

                response.Success = true;
                response.Data = res.ToJson();
                response.Msg = "请求成功";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Msg = ExceptionHelper.GetExceptionAllMsg(ex);
            }

            return response;
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 注册服务
        /// 注：默认注册的服务为接口名
        /// </summary>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <typeparam name="Service">服务实现</typeparam>
        public void RegisterService<IService, Service>() where Service : class, IService where IService : class
        {
            RegisterService<IService, Service>(typeof(IService).Name);
        }

        /// <summary>
        /// 注册服务,指定服务名
        /// </summary>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <typeparam name="Service">服务实现</typeparam>
        /// <param name="serviceName">服务名</param>
        public void RegisterService<IService, Service>(string serviceName) where Service : class, IService where IService : class
        {
            _serviceHandle.Add(serviceName, typeof(Service));
        }

        /// <summary>
        /// 开始运行服务
        /// </summary>
        public void Start()
        {
            _serverChannel = _serverBootstrap.BindAsync(_port).Result;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            _serverChannel.CloseAsync();
        }

        #endregion
    }
}
