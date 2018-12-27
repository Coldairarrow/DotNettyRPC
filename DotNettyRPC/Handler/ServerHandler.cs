using Coldairarrow.Util;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Text;

namespace Coldairarrow.DotNettyRPC
{
    class ServerHandler : ChannelHandlerAdapter
    {
        public ServerHandler(RPCServer rPCServer)
        {
            _rpcServer = rPCServer;
        }
        RPCServer _rpcServer { get; }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message as IByteBuffer;
            ResponseModel response = _rpcServer.GetResponse(msg.ToString(Encoding.UTF8).ToObject<RequestModel>());
            var sendMsg = response.ToJson().ToBytes(Encoding.UTF8);
            context.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendMsg));
            context.CloseAsync();
        }
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}