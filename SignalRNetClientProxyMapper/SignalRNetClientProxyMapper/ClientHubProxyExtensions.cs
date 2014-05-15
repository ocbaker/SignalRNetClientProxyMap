using System;
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

            var name = method.Name;
            var hubName = GetHubName(method);

            proxy.Add(name,
                    (Func<dynamic, IDisposable>)(action => HubProxyExtensions.On(proxy.HubProxy, hubName, action)));
        }

        static void MapGenericReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            var arguments = method.ReturnType.GetGenericArguments();
            var invokeReturnInstance = InvokeReturnMethod.MakeGenericMethod(arguments);

            var name = method.Name;
            var hubName = GetHubName(method);

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

        static string GetHubName(MethodInfo method) {
            var hubMethodNameAttribute = method.GetCustomAttribute<HubMethodNameAttribute>(false);
            var hubName = hubMethodNameAttribute != null ? hubMethodNameAttribute.MethodName : method.Name;
            return hubName;
        }

        static void MapReturnFunctions(ClientHubProxyBase proxy, MethodInfo method) {
            Contract.Requires<ArgumentOutOfRangeException>(method.GetParameters().Length <= 10,
                "The Proxy mapper only supports methods with up to 10 parameters");

            var name = method.Name;
            var hubName = GetHubName(method);

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