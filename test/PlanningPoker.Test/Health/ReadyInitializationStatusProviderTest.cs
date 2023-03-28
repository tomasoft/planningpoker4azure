using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningPoker.Health;

namespace PlanningPoker.Test.Health
{
    [TestClass]
    public class ReadyInitializationStatusProviderTest
    {
        [TestMethod]
        public void IsInitialized_ReturnsTrue()
        {
            var target = new ReadyInitializationStatusProvider();
            var result = target.IsInitialized;
            Assert.IsTrue(result);
        }
    }
}
