using g3;
using gs;
using gs.engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using gsCore.FunctionalTests.Models;
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

            var directory = TestDataPaths.GetTestDataDirectory(CaseName);
            var meshFilePath = TestDataPaths.GetMeshFilePath(directory);
            var expectedFilePath = TestDataPaths.GetExpectedFilePath(directory);

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
            ExpectFailure<LayerCountMismatch>(new GenericRepRapSettings() { LayerHeightMM = 0.3 });
        }

        [TestMethod]
        public void WrongShells()
        {
            ExpectFailure<FeatureCumulativeExtrusionMismatch>(new GenericRepRapSettings() { Shells = 3 });
        }

        [TestMethod]
        public void WrongFloorLayers()
        {
            ExpectFailure<MissingFeature>(new GenericRepRapSettings() { FloorLayers = 0 });
        }

        [TestMethod]
        public void WrongRoofLayers()
        {
            ExpectFailure<MissingFeature>(new GenericRepRapSettings() { FloorLayers = 3 });
        }

        public void ExpectFailure<ExceptionType>(GenericRepRapSettings settings) where ExceptionType : Exception
        {
            // Arrange
            var engine = new EngineFFF();
            var resultGenerator = new ResultGenerator(engine, new ConsoleLogger()) { Settings = settings };
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF());
            var print = new PrintTestRunner(CaseName, resultGenerator, resultAnalyzer);

            // Act
            print.GenerateFile();

            // Assert
            Assert.ThrowsException<ExceptionType>(() =>
            {
                print.CompareResults();
            });
        }
    }
}