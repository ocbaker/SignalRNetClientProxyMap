using System;

namespace SignalRNetClientProxyMapper
{
    /// <summary>
    ///     Disables Mapping of a function for Strongly Typed Hubs
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NotMappedAttribute : Attribute { }
}