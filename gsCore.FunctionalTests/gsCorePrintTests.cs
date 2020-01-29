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

        [TestMethod]
        public void Sphere_Flashforge()
        {
            // Arrange
            var print = new PrintGenComparator("Sphere.Flashforge", new EngineFFF());
            print.settings = new FlashforgeSettings
            {
                GenerateSupport = true,
            };

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Bunny_Printrbot()
        {
            // Arrange
            var print = new PrintGenComparator("Bunny.Printrbot", new EngineFFF());
            print.settings = new PrintrbotSettings
            {
                GenerateSupport = false,
            };

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Benchy_Monoprice()
        {
            // Arrange
            var print = new PrintGenComparator("Benchy.Monoprice", new EngineFFF());
            print.settings = new MonopriceSettings
            {
                GenerateSupport = false,
            };

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Robot_Makerbot()
        {
            // Arrange
            var print = new PrintGenComparator("Robot.Makerbot", new EngineFFF());
            print.settings = new MakerbotSettings
            {
                GenerateSupport = false,
                Shells = 1,
                FloorLayers = 3,
                RoofLayers = 3,
            };

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }
    }
}