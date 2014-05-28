﻿using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImpromptuInterface;
using Microsoft.AspNet.SignalR.Client;

namespace SignalRNetClientProxyMapper
{
    /// <summary>
    /// Contains extension methods for creating a strong client proxy
    /// </summary>
    public static class ClientHubProxyExtensions
    {
        static readonly MethodInfo InvokeReturnMethod = typeof (ClientHubProxyBase).GetMethod("InvokeReturn",
            BindingFlags.NonPublic | BindingFlags.Instance);
        const string HasInterfaceITest = "^[I]{1}[A-Z]{1}";

        /// <summary>
        /// Creates a strong proxy from the defenition of an interface
        /// </summary>
        /// <typeparam name="T">The interface to create the proxy from</typeparam>
        /// <param name="this">The HubConnection to attach the proxy to</param>
        /// <param name="dropInterfaceI">If the prxy is named with IInterface, drop the I from the mapped name if true, otherwise leave it in.</param>
        /// <returns>An Interface of type T which represents the HubProxy</returns>
        public static T CreateStrongHubProxy<T>(this HubConnection @this, bool dropInterfaceI = true)
            where T : class, IClientHubProxyBase
        {
            Contract.Requires<InvalidCastException>(typeof(T).IsInterface, "The Proxy Type must be an Interface");

            return default(T).GetStrongTypedClientProxy(@this.CreateHubProxy(GetHubName<T>(dropInterfaceI)));
        }

        /// <summary>
        /// Creates a strong proxy from the defenition of an interface
        /// </summary>
        /// <typeparam name="T">The interface to create the proxy from</typeparam>
        /// <param name="this">The interface to create the proxy from</param>
        /// <param name="hubProxy">The hubproxy to create the strong interface from</param>
        /// <returns>An Interface of type T which represents the HubProxy</returns>
        public static T GetStrongTypedClientProxy<T>(this T @this, IHubProxy hubProxy)
            where T : class, IClientHubProxyBase {
            Contract.Requires<InvalidCastException>(typeof (T).IsInterface, "The Proxy Type must be an Interface");

            var type = typeof (T);
            dynamic proxy = new ClientHubProxyBase(hubProxy);

            foreach (
                var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                if (method.IsSpecialName && (method.Name.StartsWith("set_") || method.Name.StartsWith("get_")))
                    continue;

                var returnType = method.ReturnType.IsGenericType
                    ? method.ReturnType.GetGenericTypeDefinition()
                    : method.ReturnType;
                if (returnType == typeof (Task))
                    MapReturnFunctions(proxy, method);
                else if (returnType == typeof (Task<>))
                    MapGenericReturnFunctions(proxy, method);
                else if (returnType == typeof (IDisposable))
                    MapEventFunctions(proxy, method);
                else {
                    throw new ArgumentException(
                        "Strong-Typed Methods must return a Task or Task<>, Events must return an IDisposable",
                        method.Name);
                }
            }

            return Impromptu.ActLike<T>(proxy);
        }

        internal static void MapEventFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 7,
                "The Proxy mapper only supports events with up to 7 parameters");

            var arguments = method.GetParameters()[0].ParameterType.GenericTypeArguments.Length;

            var name = method.Name;
            var hubName = GetHubMethodName(method);

            if (proxy.ContainsKey(name))
                throw new NotSupportedException("Overloading is not supported");

            proxy.Add(name,
                    (Func<dynamic, IDisposable>)(action => HubProxyExtensions.On(proxy.HubProxy, hubName, action)));
        }

        internal static void MapGenericReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            var arguments = method.ReturnType.GetGenericArguments();
            var invokeReturnInstance = InvokeReturnMethod.MakeGenericMethod(arguments);

            var name = method.Name;
            var hubName = GetHubMethodName(method);

            if(proxy.ContainsKey(name))
                throw new NotSupportedException("Overloading is not supported");

            switch (method.GetParameters().Length) {
            case 0:
                proxy.Add(name,
                    (Func<dynamic>)(() => invokeReturnInstance.Invoke(proxy, new object[] { hubName, new object[] { } })));
                break;
            case 1:
                proxy.Add(name,
                    (Func<dynamic, dynamic>)
                        (arg1 => invokeReturnInstance.Invoke(proxy, new object[] { hubName, new object[] { arg1 } })));
                break;
            case 2:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic>)
                        ((arg1, arg2) =>
                            invokeReturnInstance.Invoke(proxy, new object[] { hubName, new object[] { arg1, arg2 } })));
                break;
            case 3:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3) =>
                            invokeReturnInstance.Invoke(proxy, new object[] { hubName, new object[] { arg1, arg2, arg3 } })));
                break;
            case 4:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] { hubName, new object[] { arg1, arg2, arg3, arg4 } })));
                break;
            case 5:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] { hubName, new object[] { arg1, arg2, arg3, arg4, arg5 } })));
                break;
            case 6:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] { hubName, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 } })));
                break;
            case 7:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] { hubName, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 } })));
                break;
            }
        }

        internal static string GetHubMethodName(MethodInfo method) {
            var hubMethodNameAttribute = method.GetCustomAttribute<HubMethodNameAttribute>(false);
            return hubMethodNameAttribute != null ? hubMethodNameAttribute.MethodName : method.Name;
        }

        internal static string GetHubName<T>(bool dropInterfaceI = true) {
            var hubMethodNameAttribute = typeof(T).GetCustomAttribute<HubNameAttribute>(false);

            return hubMethodNameAttribute != null ? hubMethodNameAttribute.HubName : ((dropInterfaceI && Regex.IsMatch(typeof(T).Name, HasInterfaceITest)) ? typeof(T).Name.Remove(0, 1) : typeof(T).Name);
        }

        internal static void MapReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            var name = method.Name;
            var hubName = GetHubMethodName(method);

            if (proxy.ContainsKey(name))
                throw new NotSupportedException("Overloading is not supported");

            switch (method.GetParameters().Length) {
            case 0:
                    proxy.Add(name, (Func<dynamic>)(() => proxy.Invoke(hubName, new object[] { })));
                break;
            case 1:
                proxy.Add(name, (Func<dynamic, dynamic>)(arg1 => proxy.Invoke(hubName, new object[] { arg1 })));
                break;
            case 2:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic>)((arg1, arg2) => proxy.Invoke(hubName, new object[] { arg1, arg2 })));
                break;
            case 3:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3) => proxy.Invoke(hubName, new object[] { arg1, arg2, arg3 })));
                break;
            case 4:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4) => proxy.Invoke(hubName, new object[] { arg1, arg2, arg3, arg4 })));
                break;
            case 5:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5) =>
                            proxy.Invoke(hubName, new object[] { arg1, arg2, arg3, arg4, arg5 })));
                break;
            case 6:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6) =>
                            proxy.Invoke(hubName, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 })));
                break;
            case 7:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                            proxy.Invoke(hubName, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 })));
                break;
            }
        }
    }
}