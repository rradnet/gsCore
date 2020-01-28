using gs;
using gs.engines;
using gs.info;
using gs.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.FunctionalTests
{

    [TestClass]
    public class gsCorePrintTests
    {
        [TestMethod]
        public void Frustum_RepRap()
        {
            // arrange
            var print = new PrintGenComparator("Frustum.RepRap", new EngineFFF());
            print.settings = new RepRapSettings { GenerateSupport = false };

            // act
            print.GenerateFile();

            // assert
            print.CompareResults();
        }

        [TestMethod]
        public void Cube_Prusa()
        {
            // arrange
            var print = new PrintGenComparator("Cube.Prusa", new EngineFFF());
            print.settings = new PrusaSettings { GenerateSupport = false };

            // act
            print.GenerateFile();

            // assert
            print.CompareResults();
        }
    }
}