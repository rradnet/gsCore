using System;
using System.Collections.Generic;
using System.IO;
using g3;
using gs;
using gsCore.FunctionalTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.FunctionalTests.Utility 
{
    public class ResultComparer : IResultComparer
    {
        protected GCodeFile LoadGCode(string gcodeFilePath)
        {
            var parser = new GenericGCodeParser();
            using var fileReader = File.OpenText(gcodeFilePath);
            return parser.Parse(fileReader);
        }

        public double allowedBoundingBoxError = 1e-4;
        public double allowedCenterOfMassError = 1e-4;
        public double allowedExtrusionAmountError = 1e-4;
        public double allowedExtrusionDistanceError = 1e-4;
        public double allowedExtrusionTimeError = 1e-4;

        private const double maximumDifferenceToSkipErrorFraction = 1;

        public void AssertMatches(SubLayerDetails layerA, SubLayerDetails layerB, FillTypeFlags fillType, int layerNumber)
        {
            AssertErrorFraction(layerA.boundingBox.Max.x, layerB.boundingBox.Max.x, allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum x");
            AssertErrorFraction(layerA.boundingBox.Max.y, layerB.boundingBox.Max.y, allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum y");
            AssertErrorFraction(layerA.boundingBox.Min.x, layerB.boundingBox.Min.x, allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum x");
            AssertErrorFraction(layerA.boundingBox.Min.y, layerB.boundingBox.Min.y, allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum y");
            AssertErrorFraction(layerA.CenterOfMass.x, layerB.CenterOfMass.x, allowedCenterOfMassError, layerNumber, fillType, "center of mass x");
            AssertErrorFraction(layerA.CenterOfMass.y, layerB.CenterOfMass.y, allowedCenterOfMassError, layerNumber, fillType, "center of mass y");
            AssertErrorFraction(layerA.extrusionAmount, layerB.extrusionAmount, allowedExtrusionAmountError, layerNumber, fillType, "extrusion amount");
            AssertErrorFraction(layerA.extrusionDistance, layerB.extrusionDistance, allowedExtrusionDistanceError, layerNumber, fillType, "extrusion distance");
            AssertErrorFraction(layerA.extrusionTime, layerB.extrusionTime, allowedExtrusionTimeError, layerNumber, fillType, "extrusion time");
        }

        private static void AssertErrorFraction(double result, double expected, double maximumError, int layerNumber, FillTypeFlags fillType, string name = "value")
        {
            if (Math.Abs(result - expected) < maximumDifferenceToSkipErrorFraction)
                return;
            var error = Math.Abs((result - expected) / result);
            if (error > maximumError)
                Assert.Fail("Expected " + name + " to be " + expected + ", got " + result + " (layer " + layerNumber + ", fill type " + fillType + "). Error was " + error + " > " + maximumError + ".");
        }

        public void CompareFiles(string gcodeFilePathExpected, string gcodeFilePathResult)
        {
            var resultLayersDetails = GetLayersDetails(LoadGCode(gcodeFilePathExpected));
            var expectedLayersDetails = GetLayersDetails(LoadGCode(gcodeFilePathResult));

            Assert.AreEqual(resultLayersDetails.Count, expectedLayersDetails.Count, "The expected file has " + expectedLayersDetails.Count + " layers, while the result file has " + resultLayersDetails.Count + ".");

            for (int layerNumber = 0; layerNumber < resultLayersDetails.Count; layerNumber++)
            {
                Dictionary<FillTypeFlags, SubLayerDetails> resultLayerDetails = resultLayersDetails[layerNumber];
                Dictionary<FillTypeFlags, SubLayerDetails> expectedLayerDetails = expectedLayersDetails[layerNumber];

                foreach (FillTypeFlags fillType in resultLayerDetails.Keys)
                {

                    if (expectedLayerDetails.TryGetValue(fillType, out SubLayerDetails expectedSubLayer))
                    {
                        AssertMatches(expectedSubLayer, resultLayerDetails[fillType], fillType, layerNumber);
                    }
                    else
                    {
                        Assert.Fail("The expected file does not have fillType " + fillType + " on layer " + layerNumber + " while the result file does.");
                    }
                }
            }
        }

        private List<Dictionary<FillTypeFlags, SubLayerDetails>> GetLayersDetails(GCodeFile file)
        {

            var layers = new List<Dictionary<FillTypeFlags, SubLayerDetails>>();
            Dictionary<FillTypeFlags, SubLayerDetails> currentLayer = null;

            double lastExtrusionAmount = 0;
            double lastX = 0;
            double lastY = 0;
            double feedrate = 0;

            foreach (var line in file.AllLines())
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
                    if (indexOfFillType >= 0 && int.TryParse(line.comment?.Substring(indexOfFillType + 9), out int fillTypeInt))
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