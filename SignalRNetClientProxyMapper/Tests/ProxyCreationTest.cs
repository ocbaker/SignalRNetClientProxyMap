using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using SignalRNetClientProxyMapper;

namespace Tests
{
    [TestFixture]
    public class ProxyCreationTest
    {
        [SetUp]
        public void SetUp() {
            _hubProxy = A.Fake<IHubProxy>();
            _hubConnection = A.Fake<HubConnection>();
        }

        IHubProxy _hubProxy;
        const ITestProxy TestProxy = null;
        HubConnection _hubConnection = null;

        [Test]
        public void CanCallActionsWithNoParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.ActionWithNoParameters();

            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanCallActionsWithParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.ActionWithParameter("test");

            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, new object[] {"test"}))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanCallActionsWithNoParametersAndAlternativeName() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.ActionWithNoParametersAndAlternativeName();

            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanCallFunctionsWithNoParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.FunctionWithNoParameters();

            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanCallFunctionsWithParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.FunctionWithParameter("test");

            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, new object[] {"test"}))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanCallFunctionsWithNoParametersAndAlternativeName() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.FunctionWithNoParametersAndAlternativeName();

            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void CanSubscribeToEventWithNoParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.subscribableEventWithNoParameters(() => { });
        }

        [Test]
        public void CanSubscribeToEventWithParameters() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.subscribableEventWithParameter(paramter => { });
        }

        [Test]
        public void CanSubscribeToEventWithNoParametersAndAlternativeName() {
            var testProxy = TestProxy.GetStrongTypedClientProxy(_hubProxy);

            testProxy.subscribableEventWithNoParametersAndAlternativeName(() => { });
        }

        [Test]
        public void ShouldFailBecuaseOnlyInterfacesAccepted() {
            var test = new FailingClass(_hubProxy);
            test.Invoking(x => x.GetStrongTypedClientProxy(_hubProxy)).ShouldThrow<InvalidCastException>();
        }

        [Test]
        public void ShouldFailIfNonTasksInProxy() {
            IFailingProxywithInvalidMethodSignature testProxy = null;

            testProxy.Invoking(x => x = x.GetStrongTypedClientProxy(_hubProxy))
                .ShouldThrow<ArgumentException>("Proxy should never accept anything other than Task & Task<T>");
        }

        [Test]
        public void CreateProxyFromConnection() {
            var testProxy = _hubConnection.CreateStrongHubProxy<ITestProxy>();

            testProxy.ActionWithNoParameters();
        }
    }

    public interface IEmptyProxy : IClientHubProxyBase {}


    public class FailingClass : IEmptyProxy
    {
        public FailingClass(IHubProxy hubProxy) {}

        public Task Invoke(string method, params object[] args) {
            throw new NotImplementedException();
        }

        public Task<T> Invoke<T>(string method, params object[] args) {
            throw new NotImplementedException();
        }

        public Subscription Subscribe(string eventName) {
            throw new NotImplementedException();
        }
    }

    public interface IFailingProxywithInvalidMethodSignature : IClientHubProxyBase
    {
        void ActionWithVoidReturn();
    }

    public interface ITestProxy : IClientHubProxyBase
    {
        //IObservable<string> ObservedEvent { get; set; }
        IDisposable subscribableEventWithNoParameters(Action action);
        IDisposable subscribableEventWithParameter(Action<string> action);
        [HubMethodName("AlternativeName")]
        IDisposable subscribableEventWithNoParametersAndAlternativeName(Action action);

        Task ActionWithNoParameters();
        Task ActionWithParameter(string message);
        [HubMethodName("AlternativeName")]
        Task ActionWithNoParametersAndAlternativeName();

        Task<string> FunctionWithNoParameters();
        Task<string> FunctionWithParameter(string message);
        [HubMethodName("AlternativeName")]
        Task<string> FunctionWithNoParametersAndAlternativeName();
    }
}