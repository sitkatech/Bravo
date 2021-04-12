using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bravo.Accessors;
using Bravo.Accessors.Modflow;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Engines;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Bravo.Tests.EngineTests
{
    [TestClass]
    public class AnalysisEngineTests
    {
        private readonly IModflowAccessor _modflowAccessorMock = Mock.Create<IModflowAccessor>(Behavior.Strict);
        private AnalysisEngine CreateAnalysisEngine()
        {
            var result = new AnalysisEngine();
            result.AccessorFactory = new AccessorFactory();
            result.AccessorFactory.AddOverride(_modflowAccessorMock);
            return result;
        }
        [TestMethod]
        public void ModflowFailed()
        {
            _modflowAccessorMock.Arrange(a => a.RunModflow("fake.exe", "fake.nam")).Returns(false);
            var sut = CreateAnalysisEngine();
            var result = sut.RunAnalysis(new Run
            {
                Model = new Model
                {
                    ModflowExeName = "fake.exe",
                    NamFileName = "fake.nam"
                }
            });
            Assert.IsFalse(result.Success);
            _modflowAccessorMock.Assert(a => a.RunModflow(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunModflow("fake.exe", "fake.nam"), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget(Arg.AnyString), Occurs.Never());
        }
        [TestMethod]
        public void ModflowSuccess_NoZoneBudget()
        {
            _modflowAccessorMock.Arrange(a => a.RunModflow("fake.exe", "fake.nam")).Returns(true);
            var sut = CreateAnalysisEngine();
            var result = sut.RunAnalysis(new Run
            {
                Model = new Model
                {
                    ModflowExeName = "fake.exe",
                    NamFileName = "fake.nam"
                }
            });
            Assert.IsTrue(result.Success);
            _modflowAccessorMock.Assert(a => a.RunModflow(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunModflow("fake.exe", "fake.nam"), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget(Arg.AnyString), Occurs.Never());
        }

        [TestMethod]
        public void ModflowSuccess_ZoneBudgetSuccess()
        {
            _modflowAccessorMock.Arrange(a => a.RunModflow("fake.exe", "fake.nam")).Returns(true);
            _modflowAccessorMock.Arrange(a => a.RunZoneBudget("fakeZB.exe")).Returns(true);
            var sut = CreateAnalysisEngine();
            var result = sut.RunAnalysis(new Run
            {
                Model = new Model
                {
                    ModflowExeName = "fake.exe",
                    NamFileName = "fake.nam",
                    ZoneBudgetExeName = "fakeZB.exe"
                }
            });
            Assert.IsTrue(result.Success);
            _modflowAccessorMock.Assert(a => a.RunModflow(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunModflow("fake.exe", "fake.nam"), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget(Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget("fakeZB.exe"), Occurs.Once());
        }

        [TestMethod]
        public void ModflowSuccess_ZoneBudgetFailed()
        {
            _modflowAccessorMock.Arrange(a => a.RunModflow("fake.exe", "fake.nam")).Returns(true);
            _modflowAccessorMock.Arrange(a => a.RunZoneBudget("fakeZB.exe")).Returns(false);
            var sut = CreateAnalysisEngine();
            var result = sut.RunAnalysis(new Run
            {
                Model = new Model
                {
                    ModflowExeName = "fake.exe",
                    NamFileName = "fake.nam",
                    ZoneBudgetExeName = "fakeZB.exe"
                }
            });
            Assert.IsFalse(result.Success);
            _modflowAccessorMock.Assert(a => a.RunModflow(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunModflow("fake.exe", "fake.nam"), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget(Arg.AnyString), Occurs.Once());
            _modflowAccessorMock.Assert(a => a.RunZoneBudget("fakeZB.exe"), Occurs.Once());
        }
    }
}
