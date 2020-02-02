using g3;
using gs;
using gs.engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using gsCore.FunctionalTests.Utility;

namespace gsCore.FunctionalTests
{

    [TestClass]
    public class FFF_PrintTests_ExpectedFailures
    {
        private const string CaseName = "Cube.Failures";

        [ClassInitialize]
        public static void CreateExpectedResult(TestContext context)
        {
            var generator = new EngineFFF().Generator;

            var directory = Paths.GetTestDataDirectory(CaseName);
            var meshFilePath = Paths.GetMeshFilePath(directory);
            var expectedFilePath = Paths.GetExpectedFilePath(directory);

            var parts = new[]{
                new Tuple<DMesh3, object>(StandardMeshReader.ReadMesh(meshFilePath), null)
            };

            var expectedResult = generator.GenerateGCode(parts, new GenericRepRapSettings(), out _, null, Console.WriteLine);

            using var w = new StreamWriter(expectedFilePath);
            var writer = new StandardGCodeWriter();
            writer.WriteFile(expectedResult, w);
        }

        [TestMethod]
        public void WrongLayerHeight()
        {
            ExpectFailure(new GenericRepRapSettings() { LayerHeightMM = 0.3 });
        }

        [TestMethod]
        public void WrongShells()
        {
            ExpectFailure(new GenericRepRapSettings() { Shells = 1 });
        }

        [TestMethod]
        public void WrongFloorLayers()
        {
            ExpectFailure(new GenericRepRapSettings() { FloorLayers = 0 });
        }

        [TestMethod]
        public void WrongRoofLayers()
        {
            ExpectFailure(new GenericRepRapSettings() { FloorLayers = 3 });
        }

        public void ExpectFailure(GenericRepRapSettings settings)
        {
            // Arrange
            var engine = new EngineFFF();
            var resultGenerator = new ResultGenerator(engine, new ConsoleLogger());
            var resultComparer = new ResultComparer();
            var print = new PrintTestRunner(CaseName, resultGenerator, resultComparer);

            // Use reflection to set a property on the settings object
            resultGenerator.Settings = settings;

            // Act
            print.GenerateFile();

            // Assert
            Assert.ThrowsException<AssertFailedException>(() =>
            {
                print.CompareResults();
            });
        }
    }
}