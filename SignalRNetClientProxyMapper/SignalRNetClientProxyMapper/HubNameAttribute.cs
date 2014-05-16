using System;

namespace SignalRNetClientProxyMapper
{
    /// <summary>
    ///     Enables you to define an alternative server-side name mapping for a hub
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class HubNameAttribute : Attribute
    {
        readonly string _hubName;

        /// <summary>
        ///     Enables you to define an alternative server-side name mapping for a hub
        /// </summary>
        /// <param name="hubName">Name of the hub</param>
        public HubNameAttribute(string hubName) {
            _hubName = hubName;
        }

        /// <summary>
        ///     The name of the hub on the Server
        /// </summary>
        public string HubName {
            get { return _hubName; }
        }
    }
}