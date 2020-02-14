using gs.engines;
using gs.info;
using gs.interfaces;
using gsCore.FunctionalTests.Models;
using gsCore.FunctionalTests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.FunctionalTests
{

    [TestClass]
    public class FFF_PrintTests_Matches
    {
        protected PrintTestRunner TestRunnerFactory(string caseName, IProfile settings)
        {
            var engine = new EngineFFF();
            var logger = new ConsoleLogger();
            var resultGenerator = new ResultGenerator(engine, logger) {Settings = settings};
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), logger);
            return new PrintTestRunner(caseName, resultGenerator, resultAnalyzer);
        }

        [TestMethod]
        public void Frustum_RepRap()
        {
            // Arrange
            var print = TestRunnerFactory("Frustum.RepRap", new RepRapSettings
            {
                GenerateSupport = false,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Cube_Prusa()
        {
            // Arrange
            var print = TestRunnerFactory("Cube.Prusa", new PrusaSettings {
                GenerateSupport = false,
                LayerHeightMM = 0.3,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Sphere_Flashforge()
        {
            // Arrange
            var print = TestRunnerFactory("Sphere.Flashforge", new FlashforgeSettings
            {
                GenerateSupport = true,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Bunny_Printrbot()
        {
            // Arrange
            var print = TestRunnerFactory("Bunny.Printrbot", new PrintrbotSettings
            {
                GenerateSupport = false,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Benchy_Monoprice()
        {
            // Arrange
            var print = TestRunnerFactory("Benchy.Monoprice", new MonopriceSettings
            {
                GenerateSupport = false,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }

        [TestMethod]
        public void Robot_Makerbot()
        {
            // Arrange
            var print = TestRunnerFactory("Robot.Makerbot", new MakerbotSettings
            {
                GenerateSupport = false,
                Shells = 1,
                FloorLayers = 3,
                RoofLayers = 3,
                WallThicknessMM = 0.4,
            });

            // Act
            print.GenerateFile();

            // Assert
            print.CompareResults();
        }
    }
}