using System;

namespace SignalRNetClientProxyMapper
{
    /// <summary>
    ///     Enables you to define an alternative server-side name mapping for a hub method
    /// </summary>
    public class HubMethodNameAttribute : Attribute
    {
        readonly string _methodName;

        /// <summary>
        ///     Enables you to define an alternative server-side name mapping for a hub method
        /// </summary>
        /// <param name="methodName">Name of the method</param>
        /// <exception cref="NotImplementedException">Attribute not yet implimented</exception>
        public HubMethodNameAttribute(string methodName) {
            _methodName = methodName;
        }

        /// <summary>
        ///     The name of the method on the Server
        /// </summary>
        public string MethodName {
            get { return _methodName; }
        }
    }
}