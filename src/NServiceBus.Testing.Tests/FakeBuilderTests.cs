namespace NServiceBus.Testing.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class FakeBuilderTests
    {
        [Test]
        public void ShouldResolveRegisteredFuncs()
        {
            var builder = new FakeBuilder();

            var instance = new SomeClass();

            builder.Register(()=> instance);

            Assert.AreSame(instance, builder.Build<SomeClass>());
        }

        [Test]
        public void ShouldResolveRegisteredFuncArrays()
        {
            var builder = new FakeBuilder();

            var instance = new SomeClass();
            var instance2 = new SomeClass();

            builder.Register(() => new []
            {
                instance,
                instance2
            });

            CollectionAssert.Contains(builder.BuildAll<SomeClass>(), instance);
            CollectionAssert.Contains(builder.BuildAll<SomeClass>(), instance2);
        }

        [Test]
        public void ShouldResolveRegisteredObjectFuncs()
        {
            var builder = new FakeBuilder();

            object instance = new SomeClass();

            builder.Register(typeof(SomeClass),() => instance);

            Assert.AreSame(instance, builder.Build<SomeClass>());
        }

        [Test]
        public void ShouldResolveRegisteredObjectFuncArrays()
        {
            var builder = new FakeBuilder();

            var instance = new SomeClass();
            var instance2 = new SomeClass();

            builder.Register(typeof(SomeClass), () => new object[]
            {
                instance,
                instance2
            });

            CollectionAssert.Contains(builder.BuildAll<SomeClass>(), instance);
            CollectionAssert.Contains(builder.BuildAll<SomeClass>(), instance2);
        }

        class SomeClass
        {
        }
    }
}