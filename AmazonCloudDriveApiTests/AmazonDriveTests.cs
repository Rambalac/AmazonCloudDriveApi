using Xunit;
using Azi.Amazon.CloudDrive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azi.Amazon.CloudDrive.Tests
{
    public class AmazonDriveTests
    {
        public class A
        {
            public void OnUpdate()
            {

            }
        }

        [Fact]
        public void TestWeakAction()
        {
            var a = new A();
            var ev = new WeakReference<Action>(a.OnUpdate);
            Action callev = () =>
            {
                Action act;
                if (ev.TryGetTarget(out act)) act.Invoke();
                else
                    throw new InvalidOperationException("Should fail");
            };

            callev();

            a = null;
            GC.Collect(2, GCCollectionMode.Forced, true);
            Assert.Throws<InvalidOperationException>(callev);
        }

        [Fact]
        public void AuthenticationTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void AmazonDriveTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact]
        public void SafeAuthenticationAsyncTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}