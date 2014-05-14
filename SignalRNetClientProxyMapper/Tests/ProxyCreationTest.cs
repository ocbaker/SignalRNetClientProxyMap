using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        ITestProxy _testProxy;
        IHubProxy _hubProxy;

        [SetUp]
        public void SetUp() {
            _hubProxy = A.Fake<IHubProxy>();
        }

        [Test]
        public void CreateProxyTest() {
            _testProxy = _testProxy.GetStrongTypedClientProxy(_hubProxy);

            _testProxy.ActionWithNoParameters();
            _testProxy.ActionWithParameter("test");
            _testProxy.FunctionWithNoParameters();
            _testProxy.FunctionWithParameter("test");

            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, A<object[]>.That.IsEmpty())).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke(A<string>.Ignored, new object[] { "test" })).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, A<object[]>.That.IsEmpty())).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _hubProxy.Invoke<string>(A<string>.Ignored, new object[] { "test" })).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldFailIfNonTasksInProxy() {
            IFailingProxywithInvalidMethodSignature testProxy = null;

            testProxy.Invoking(x => x = x.GetStrongTypedClientProxy(_hubProxy)).ShouldThrow<ArgumentException>("Proxy should never accept anything other than Task & Task<T>");
        }

        [Test]
        public void ShouldFailBecuaseOnlyInterfacesAccepted() {
            FailingClass test = new FailingClass(_hubProxy);
            test.Invoking(x => x.GetStrongTypedClientProxy(_hubProxy)).ShouldThrow<InvalidCastException>();
        }

    }

    public interface IEmptyProxy : IClientHubProxyBase { }


    public class FailingClass : ClientHubProxyBase, IEmptyProxy
    {
        public FailingClass(IHubProxy hubProxy) : base(hubProxy) {}
    }

    public interface IFailingProxywithInvalidMethodSignature : IClientHubProxyBase
    {

        void ActionWithVoidReturn();

    }

    public interface ITestProxy : IClientHubProxyBase
    {
        Task ActionWithNoParameters();
        Task ActionWithParameter(string message);
        Task<string> FunctionWithNoParameters();
        Task<string> FunctionWithParameter(string message);
    }
}
