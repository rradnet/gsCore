using g3;
using gs;
using gs.interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace gsCore.FunctionalTests
{

    public class PrintGenComparator
    {

        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            MissingMemberHandling = MissingMemberHandling.Error,
        };

        public readonly string meshFilePath;
        public readonly string resultFilePath;
        public readonly string expectedFilePath;

        public double allowedBoundingBoxError = 1e-4;
        public double allowedCenterOfMassError = 1e-4;
        public double allowedExtrusionAmountError = 1e-4;
        public double allowedExtrusionDistanceError = 1e-4;
        public double allowedExtrusionTimeError = 1e-4;

        const double maximumDifferenceToSkipErrorFraction = 1;
        protected DirectoryInfo directory;
        public SingleMaterialFFFSettings settings;
        public IEngine engine;

        public PrintGenComparator(string name, IEngine engine)
        {
            var searchDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            do {
                foreach (var subdirectory in searchDirectory.GetDirectories()) {
                    if (subdirectory.Name == "TestData") {
                        directory = new DirectoryInfo(Path.Combine(subdirectory.FullName, name));
                        break;
                    }
                }
            } while ((searchDirectory = searchDirectory.Parent) != null && directory == null);
            if (directory == null)
                throw new FileNotFoundException("No TestData directory found.");

            var meshFiles = directory.GetFiles("*.stl");
            if (meshFiles.Length != 1) throw new ArgumentException("Expected single STL file in directory");
            meshFilePath = meshFiles[0].FullName;

            resultFilePath = Path.Combine(directory.FullName, directory.Name + ".Result.gcode");
            expectedFilePath = Path.Combine(directory.FullName, directory.Name + ".Expected.gcode");

            this.engine = engine;
        }

        public void LoadSettingsFromFiles()
        {
            Assert.IsNotNull(settings);
            string[] settingsFilePaths;
            var settingsFiles = directory.GetFiles("*.json");
            settingsFilePaths = new string[settingsFiles.Length];
            for (int i = settingsFiles.Length - 1; i >= 0; i--)
                settingsFilePaths[i] = settingsFiles[i].FullName;

            foreach (string s in settingsFilePaths) {
                try {
                    Console.WriteLine($"Loading file {Path.GetFullPath(s)}");
                    string settingsText = File.ReadAllText(s);
                    JsonConvert.PopulateObject(settingsText, settings, jsonSerializerSettings);
                } catch (Exception e) {
                    Console.WriteLine("Error processing settings file: ");
                    Console.WriteLine(Path.GetFullPath(s));
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public void SaveGCode(string path, GCodeFile file)
        {
            using (StreamWriter w = new StreamWriter(path)) {
                StandardGCodeWriter writer = new StandardGCodeWriter();
                writer.WriteFile(file, w);
            }
        }

        public GCodeFile LoadGCode(string path) 
        {
            GenericGCodeParser parser = new GenericGCodeParser();
            using (StreamReader fileReader = File.OpenText(path))
                return parser.Parse(fileReader);
        }

        static void AssertErrorFraction(double result, double expected, double maximumError, int layerNumber, FillTypeFlags fillType, string name = "value")
        {
            if (Math.Abs(result - expected) < maximumDifferenceToSkipErrorFraction)
                return;
            double error = Math.Abs((result - expected) / result);
            if (error > maximumError)
                Assert.Fail("Expected " + name + " to be " + expected + ", got " + result + " (layer " + layerNumber + ", fill type " + fillType + "). Error was " + error + " > " + maximumError + ".");
        }

        class SubLayerDetails
        { //a layer is divided into sublayers by fill type
            public AxisAlignedBox2d boundingBox;
            public Vector2d unscaledCenterOfMass;
            public double extrusionAmount;
            public double extrusionDistance;
            public double extrusionTime;

            public Vector2d CenterOfMass
            {
                get
                {
                    return unscaledCenterOfMass / extrusionAmount;
                }
            }

            public override string ToString()
            {
                return
                    "Bounding Box:\t" + boundingBox +
                    "\r\nCenter Of Mass:\t" + CenterOfMass +
                    "\r\nExtrusion Amt:\t" + extrusionAmount +
                    "\r\nExtrusion Dist:\t" + extrusionDistance +
                    "\r\nExtrusion Time:\t" + extrusionTime;
            }

            public void AssertMatches(PrintGenComparator test, SubLayerDetails other, FillTypeFlags fillType, int layerNumber)
            {
                AssertErrorFraction(other.boundingBox.Max.x, boundingBox.Max.x, test.allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum x");
                AssertErrorFraction(other.boundingBox.Max.y, boundingBox.Max.y, test.allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum y");
                AssertErrorFraction(other.boundingBox.Min.x, boundingBox.Min.x, test.allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum x");
                AssertErrorFraction(other.boundingBox.Min.y, boundingBox.Min.y, test.allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum y");
                AssertErrorFraction(other.CenterOfMass.x, CenterOfMass.x, test.allowedCenterOfMassError, layerNumber, fillType, "center of mass x");
                AssertErrorFraction(other.CenterOfMass.y, CenterOfMass.y, test.allowedCenterOfMassError, layerNumber, fillType, "center of mass y");
                AssertErrorFraction(other.extrusionAmount, extrusionAmount, test.allowedExtrusionAmountError, layerNumber, fillType, "extrusion amount");
                AssertErrorFraction(other.extrusionDistance, extrusionDistance, test.allowedExtrusionDistanceError, layerNumber, fillType, "extrusion distance");
                AssertErrorFraction(other.extrusionTime, extrusionTime, test.allowedExtrusionTimeError, layerNumber, fillType, "extrusion time");
            }
        }

        public void GenerateFile()
        {
            Assert.IsNotNull(settings);
            Assert.IsNotNull(engine);

            var generator = engine.Generator;

            var parts = new[]{
                new Tuple<DMesh3, object>(StandardMeshReader.ReadMesh(meshFilePath), generator.AcceptsPartSettings ? settings : null)
            };

            GCodeFile resultFile = engine.Generator.GenerateGCode(parts, settings, out var generationReport, null, (s) => Console.WriteLine(s));
            SaveGCode(resultFilePath, resultFile); //writing and reading the gcode file from the hard drive can cause precision loss. To avoid false assertion fails, it may be desireable to give both the same treatment.
        }

        public void CompareResults()
        {
            var resultLayersDetails = GetLayersDetails(LoadGCode(resultFilePath));
            var expectedLayersDetails = GetLayersDetails(LoadGCode(expectedFilePath));

            Assert.AreEqual(resultLayersDetails.Count, expectedLayersDetails.Count, "The expected file has " + expectedLayersDetails.Count + " layers, while the result file has " + resultLayersDetails.Count + ".");

            for (int layerNumber = 0; layerNumber < resultLayersDetails.Count; layerNumber++)
            {
                Dictionary<FillTypeFlags, SubLayerDetails> resultLayerDetails = resultLayersDetails[layerNumber];
                Dictionary<FillTypeFlags, SubLayerDetails> expectedLayerDetails = expectedLayersDetails[layerNumber];

                foreach (FillTypeFlags fillType in resultLayerDetails.Keys)
                {

                    if (expectedLayerDetails.TryGetValue(fillType, out SubLayerDetails expectedSubLayer))
                    {
                        expectedSubLayer.AssertMatches(this, resultLayerDetails[fillType], fillType, layerNumber);
                    }
                    else
                    {
                        Assert.Fail("The expected file does not have fillType " + fillType + " on layer " + layerNumber + " while the result file does.");
                    }
                }
            }
        }

        List<Dictionary<FillTypeFlags, SubLayerDetails>> GetLayersDetails(GCodeFile file)
        {

            List<Dictionary<FillTypeFlags, SubLayerDetails>> layers = new List<Dictionary<FillTypeFlags, SubLayerDetails>>();
            Dictionary<FillTypeFlags, SubLayerDetails> currentLayer = null; //not set to an instance of an object until the layer 0 comment is reached

            double lastExtrusionAmount = 0;
            double lastX = 0;
            double lastY = 0;
            double feedrate = 0;

            foreach (GCodeLine line in file.AllLines())
            {

                if (line.comment != null && line.comment.Contains("layer") && !line.comment.Contains("feature"))
                {
                    if (currentLayer != null)
                        layers.Add(currentLayer);
                    currentLayer = new Dictionary<FillTypeFlags, SubLayerDetails>();
                    continue;
                }

                if (line.type != GCodeLine.LType.GCode || currentLayer == null)
                    continue;

                double f = GCodeUtil.UnspecifiedValue;
                if (GCodeUtil.TryFindParamNum(line.parameters, "F", ref f))
                    feedrate = f;

                double x = GCodeUtil.UnspecifiedValue, y = GCodeUtil.UnspecifiedValue;
                bool found_x = GCodeUtil.TryFindParamNum(line.parameters, "X", ref x);
                bool found_y = GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y);

                if (!found_x || !found_y)
                    continue;

                double extrusionAmount = GCodeUtil.UnspecifiedValue;
                if (GCodeUtil.TryFindParamNum(line.parameters, "E", ref extrusionAmount) && extrusionAmount >= lastExtrusionAmount)
                {

                    int indexOfFillType = line.comment?.IndexOf("Fill Type") ?? -1;
                    if (indexOfFillType >= 0 && int.TryParse(line.comment.Substring(indexOfFillType + 9), out int fillTypeInt))
                    {

                        FillTypeFlags fillType = (FillTypeFlags)fillTypeInt;

                        if (!currentLayer.TryGetValue(fillType, out SubLayerDetails subLayerDetails))
                        {
                            subLayerDetails = new SubLayerDetails();
                            currentLayer.Add(fillType, subLayerDetails);
                        }

                        double averageX = (lastX + x) * 0.5;
                        double averageY = (lastY + y) * 0.5;
                        double distance = Math.Sqrt((lastX - x) * (lastX - x) + (lastY - y) * (lastY - y));

                        subLayerDetails.extrusionAmount += extrusionAmount - lastExtrusionAmount;
                        subLayerDetails.extrusionDistance += distance;
                        subLayerDetails.boundingBox.Contain(new Vector2d(x, y));
                        subLayerDetails.unscaledCenterOfMass += new Vector2d(averageX, averageY) * (extrusionAmount - lastExtrusionAmount);
                        subLayerDetails.extrusionTime += distance / feedrate;

                        lastExtrusionAmount = extrusionAmount;
                    }
                }

                lastX = x;
                lastY = y;
            }

            if (currentLayer != null)
                layers.Add(currentLayer);
            return layers;
        }
    }
}
