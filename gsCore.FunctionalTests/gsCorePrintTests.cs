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
            // Arrange
            var print = new PrintGenComparator("Frustum.RepRap", new EngineFFF());
            print.settings = new RepRapSettings { 
                GenerateSupport = false,
            };

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Cube_Prusa()
        {
            // Arrange
            var print = new PrintGenComparator("Cube.Prusa", new EngineFFF());
            print.settings = new PrusaSettings { 
                GenerateSupport = false,
                LayerHeightMM = 0.3,
            };
            
            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }
    }
}