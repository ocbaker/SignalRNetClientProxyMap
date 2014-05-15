﻿using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using ImpromptuInterface;
using Microsoft.AspNet.SignalR.Client;

namespace SignalRNetClientProxyMapper
{
    public static class ClientHubProxyExtensions
    {
        static readonly MethodInfo InvokeReturnMethod = typeof (ClientHubProxyBase).GetMethod("InvokeReturn",
            BindingFlags.NonPublic | BindingFlags.Instance);

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

        static void MapEventFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 7,
                "The Proxy mapper only supports events with up to 7 parameters");

            var arguments = method.GetParameters()[0].ParameterType.GenericTypeArguments.Length;

            //Consider having Method Attributes to specify custom name type.
            var name = method.Name;

            switch (arguments) {
            case 0:
                proxy.Add(name, (Func<Action, IDisposable>) (action => proxy.HubProxy.On(name, action)));
                break;
            case 1:
                proxy.Add(name,
                    (Func<Action<dynamic>, IDisposable>) (action => { return proxy.HubProxy.On(name, action); }));
                break;
            case 2:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic>, IDisposable>) (action => proxy.HubProxy.On(name, action)));
                break;
            case 3:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic, dynamic>, IDisposable>) (action => proxy.HubProxy.On(name, action)));
                break;
            case 4:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic, dynamic, dynamic>, IDisposable>)
                        (action => proxy.HubProxy.On(name, action)));
                break;
            case 5:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic, dynamic, dynamic, dynamic>, IDisposable>)
                        (action => proxy.HubProxy.On(name, action)));
                break;
            case 6:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>, IDisposable>)
                        (action => proxy.HubProxy.On(name, action)));
                break;
            case 7:
                proxy.Add(name,
                    (Func<Action<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>, IDisposable>)
                        (action => proxy.HubProxy.On(name, action)));
                break;
            }
        }

        static void MapGenericReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            var arguments = method.ReturnType.GetGenericArguments();
            var invokeReturnInstance = InvokeReturnMethod.MakeGenericMethod(arguments);

            //Consider having Method Attributes to specify custom name type.
            var name = method.Name;

            switch (method.GetParameters().Length) {
            case 0:
                proxy.Add(name,
                    (Func<dynamic>) (() => invokeReturnInstance.Invoke(proxy, new object[] {name, new object[] {}})));
                break;
            case 1:
                proxy.Add(name,
                    (Func<dynamic, dynamic>)
                        (arg1 => invokeReturnInstance.Invoke(proxy, new object[] {name, new object[] {arg1}})));
                break;
            case 2:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic>)
                        ((arg1, arg2) =>
                            invokeReturnInstance.Invoke(proxy, new object[] {name, new object[] {arg1, arg2}})));
                break;
            case 3:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3) =>
                            invokeReturnInstance.Invoke(proxy, new object[] {name, new object[] {arg1, arg2, arg3}})));
                break;
            case 4:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4}})));
                break;
            case 5:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4, arg5}})));
                break;
            case 6:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6}})));
                break;
            case 7:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7}})));
                break;
            case 8:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8}})));
                break;
            case 9:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[] {name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9}})));
                break;
            case 10:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic,
                        dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                            invokeReturnInstance.Invoke(proxy,
                                new object[]
                                {name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10}})));
                break;
            }
        }

        static void MapReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            //Consider having Method Attributes to specify custom  name type.
            var name = method.Name;

            switch (method.GetParameters().Length) {
            case 0:
                proxy.Add(name, (Func<dynamic>) (() => proxy.Invoke(name, new object[] {})));
                break;
            case 1:
                proxy.Add(name, (Func<dynamic, dynamic>) (arg1 => proxy.Invoke(name, new object[] {arg1})));
                break;
            case 2:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic>) ((arg1, arg2) => proxy.Invoke(name, new object[] {arg1, arg2})));
                break;
            case 3:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3) => proxy.Invoke(name, new object[] {arg1, arg2, arg3})));
                break;
            case 4:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4) => proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4})));
                break;
            case 5:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5) =>
                            proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4, arg5})));
                break;
            case 6:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6) =>
                            proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6})));
                break;
            case 7:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                            proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7})));
                break;
            case 8:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
                            proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8})));
                break;
            case 9:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
                            proxy.Invoke(name, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9})));
                break;
            case 10:
                proxy.Add(name,
                    (Func<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic,
                        dynamic>)
                        ((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                            proxy.Invoke(name,
                                new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10})));
                break;
            }
        }
    }
}