using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bravo.Accessors.Containers;

namespace Bravo.Tests.AccessorTests
{
    public class BaseAccessorTest
    {
        TransactionScope transaction;

        [TestInitialize]
        public void Init()
        {
            transaction = new TransactionScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transaction?.Dispose();
        }
    }
}
