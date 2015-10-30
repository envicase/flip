using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    using static Scheduler;
    using static Times;

    public class ObservableExtensionsTest
    {
        public class Component : ObservableObject
        {
            private string _foo;

            public string Foo
            {
                get { return _foo; }
                set { SetValue(ref _foo, value); }
            }
        }

        [Theory, AutoData]
        public void ObserveReturnsObservableForSpecifiedProperty(
            string first, string second)
        {
            // TODO: 간헐적으로 다음과 같은 메시지와 함께 테스트가 실패합니다.
            //
            // Test Name:	ObserveReturnsObservableForSpecifiedProperty
            // Test FullName:	Flip.Tests.ObservableExtensionsTest.ObserveReturnsObservableForSpecifiedProperty
            // Test Source:	C:\Users\Gyuwon\Documents\Projects\flip\tests\Flip.Mvvm.Tests\ObservableExtensionsTest.cs : line 27
            // Test Outcome:	Failed
            // Test Duration:	0:00:00.027
            //
            // Result StackTrace:
            // at Moq.Mock.ThrowVerifyException(MethodCall expected, IEnumerable`1 setups, IEnumerable`1 actualCalls, Expression expression, Times times, Int32 callCount)
            //    at Moq.Mock.VerifyCalls(Interceptor targetInterceptor, MethodCall expected, Expression expression, Times times)
            //    at Moq.Mock.Verify[T](Mock`1 mock, Expression`1 expression, Times times, String failMessage)
            //    at Moq.Mock`1.Verify(Expression`1 expression, Times times)
            //    at Flip.Tests.ObservableExtensionsTest.ObserveReturnsObservableForSpecifiedProperty(String first, String second) in C:\Users\Gyuwon\Documents\Projects\flip\tests\Flip.Mvvm.Tests\ObservableExtensionsTest.cs:line 34
            // Result Message:
            // Moq.MockException :
            // Expected invocation on the mock once, but was 0 times: f => f.Action<String>(.first)
            // No setups configured.
            //
            // Performed invocations:
            // IFunctor.Action<String>("second60a9d28d-7c3d-4942-ac99-4a90f1d98814")

            var comp = new Component { Foo = first };
            var functor = Mock.Of<IFunctor>();
            comp.Observe(x => x.Foo)?
                .ObserveOn(Immediate)
                .SubscribeOn(Immediate)
                .Subscribe(functor.Action);

            comp.Foo = second;

            Mock.Get(functor).Verify(f => f.Action(first), Once());
            Mock.Get(functor).Verify(f => f.Action(second), Once());
        }

        [Fact]
        public void ObserveFailsWithInvalidExpression()
        {
            var component = new Component();
            Action action = () => component.Observe(x => x.ToString());
            action.ShouldThrow<ArgumentException>();
        }

        [Theory, AutoData]
        public void ObserveReturnsObservableOfProjectionForSpecifiedProperty()
        {
            var comp = new Component { Foo = string.Empty };
            var functor = Mock.Of<IFunctor>();
            comp.Observe(x => x.Foo, s => string.IsNullOrWhiteSpace(s))?
                .ObserveOn(Immediate)
                .SubscribeOn(Immediate)
                .Subscribe(functor.Action);

            comp.Foo = "Hello World";

            Mock.Get(functor).Verify(f => f.Action(true), Once());
            Mock.Get(functor).Verify(f => f.Action(false), Once());
        }
    }
}
