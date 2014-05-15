using System.Threading.Tasks;
using ImpromptuInterface.Dynamic;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace SignalRNetClientProxyMapper
{
    internal class ClientHubProxyBase : ImpromptuDictionary, IClientHubProxyBase
    {
        readonly IHubProxy _hubProxy;

        internal ClientHubProxyBase(IHubProxy hubProxy) {
            _hubProxy = hubProxy;
        }

        internal IHubProxy HubProxy {
            get { return _hubProxy; }
        }

        public Task Invoke(string method, params object[] args) {
            return _hubProxy.Invoke(method, args);
        }

        public Task<T> Invoke<T>(string method, params object[] args) {
            return _hubProxy.Invoke<T>(method, args);
        }

        public Subscription Subscribe(string eventName) {
            return _hubProxy.Subscribe(eventName);
        }

        internal Task<T> InvokeReturn<T>(string method, params object[] args) {
            return Invoke<T>(method, args);
        }
    }
}