using System;
using System.Reactive.Subjects;
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
        }

        ITestProxy _testProxy;
        IHubProxy _hubProxy;

        [Test]
        public void CreateProxyTest() {
            _testProxy = _testProxy.GetStrongTypedClientProxy(_hubProxy);

            _testProxy.ActionWithNoParameters();
            _testProxy.ActionWithParameter("test");
            _testProxy.FunctionWithNoParameters();
            _testProxy.FunctionWithParameter("test");

            _testProxy.ObservedEvent.Subscribe(s => { }, exception => { });

            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, new object[] {"test"}))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, A<object[]>.That.IsEmpty()))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, new object[] {"test"}))
                .MustHaveHappened(Repeated.Exactly.Once);
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
        IObservable<string> ObservedEvent { get; set; }
        Task ActionWithNoParameters();
        Task ActionWithParameter(string message);
        Task<string> FunctionWithNoParameters();
        Task<string> FunctionWithParameter(string message);
    }
}