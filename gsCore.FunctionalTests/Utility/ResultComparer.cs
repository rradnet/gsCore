using System;
using System.Collections.Generic;
using System.IO;
using g3;
using gs;
using gs.utility;
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

        public void AssertMatches(FeatureInfo layerA, FeatureInfo layerB, FillTypeFlags fillType, int layerNumber)
        {
            AssertErrorFraction(layerA.BoundingBox.Max.x, layerB.BoundingBox.Max.x, allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum x");
            AssertErrorFraction(layerA.BoundingBox.Max.y, layerB.BoundingBox.Max.y, allowedBoundingBoxError, layerNumber, fillType, "bounding box maximum y");
            AssertErrorFraction(layerA.BoundingBox.Min.x, layerB.BoundingBox.Min.x, allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum x");
            AssertErrorFraction(layerA.BoundingBox.Min.y, layerB.BoundingBox.Min.y, allowedBoundingBoxError, layerNumber, fillType, "bounding box minimum y");
            AssertErrorFraction(layerA.CenterOfMass.x, layerB.CenterOfMass.x, allowedCenterOfMassError, layerNumber, fillType, "center of mass x");
            AssertErrorFraction(layerA.CenterOfMass.y, layerB.CenterOfMass.y, allowedCenterOfMassError, layerNumber, fillType, "center of mass y");
            AssertErrorFraction(layerA.ExtrusionAmount, layerB.ExtrusionAmount, allowedExtrusionAmountError, layerNumber, fillType, "extrusion amount");
            AssertErrorFraction(layerA.ExtrusionDistance, layerB.ExtrusionDistance, allowedExtrusionDistanceError, layerNumber, fillType, "extrusion distance");
            AssertErrorFraction(layerA.ExtrusionTime, layerB.ExtrusionTime, allowedExtrusionTimeError, layerNumber, fillType, "extrusion time");
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
            var resultLayersDetails = GetLayerFeatureInfo(LoadGCode(gcodeFilePathExpected));
            var expectedLayersDetails = GetLayerFeatureInfo(LoadGCode(gcodeFilePathResult));

            Assert.AreEqual(resultLayersDetails.Count, expectedLayersDetails.Count, "The expected file has " + expectedLayersDetails.Count + " layers, while the result file has " + resultLayersDetails.Count + ".");

            for (int layerNumber = 0; layerNumber < resultLayersDetails.Count; layerNumber++)
            {
                Dictionary<FillTypeFlags, FeatureInfo> resultLayerDetails = resultLayersDetails[layerNumber];
                Dictionary<FillTypeFlags, FeatureInfo> expectedLayerDetails = expectedLayersDetails[layerNumber];

                foreach (FillTypeFlags fillType in resultLayerDetails.Keys)
                {

                    if (expectedLayerDetails.TryGetValue(fillType, out FeatureInfo expectedSubLayer))
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

        private static List<Dictionary<FillTypeFlags, FeatureInfo>> GetLayerFeatureInfo(GCodeFile file)
        {

            var layers = new List<Dictionary<FillTypeFlags, FeatureInfo>>();
            Dictionary<FillTypeFlags, FeatureInfo> currentLayer = null;

            double lastExtrusionAmount = 0;
            double lastX = 0;
            double lastY = 0;
            double feedrate = 0;

            FillTypeFlags fillType = FillTypeFlags.Unknown;

            FeatureInfo subLayerDetails = new FeatureInfo();

            foreach (var line in file.AllLines())
            {

                if (line.comment != null && line.comment.Contains("layer") && !line.comment.Contains("feature"))
                {
                    if (currentLayer != null)
                        layers.Add(currentLayer);
                    currentLayer = new Dictionary<FillTypeFlags, FeatureInfo>();
                    continue;
                }

                switch (line.type)
                {
                    case GCodeLine.LType.Comment:
                        FillTypeFlags lineFillType = FillTypeFlags.Unknown;
                        GCodeLineUtil.ExtractFillType(line, ref lineFillType);

                        if (fillType != lineFillType)
                        {
                            if (!currentLayer.TryGetValue(fillType, out subLayerDetails))
                            {
                                subLayerDetails = new FeatureInfo();
                                currentLayer.Add(fillType, subLayerDetails);
                            }
                        }
                        break;
                    case GCodeLine.LType.GCode:
                        double x = GCodeUtil.UnspecifiedValue;
                        double y = GCodeUtil.UnspecifiedValue;

                        bool found_x = GCodeUtil.TryFindParamNum(line.parameters, "X", ref x);
                        bool found_y = GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y);

                        if (!found_x || !found_y)
                            break;

                        double averageX = (lastX + x) * 0.5;
                        double averageY = (lastY + y) * 0.5;
                        double distance = Math.Sqrt((lastX - x) * (lastX - x) + (lastY - y) * (lastY - y));


                        double extrusionAmount = GCodeUtil.UnspecifiedValue;
                        if (GCodeUtil.TryFindParamNum(line.parameters, "E", ref extrusionAmount) &&
                            extrusionAmount >= lastExtrusionAmount)
                            ;

                        double f = GCodeUtil.UnspecifiedValue;
                        if (GCodeUtil.TryFindParamNum(line.parameters, "F", ref f))
                            feedrate = f;

                        subLayerDetails.ExtrusionAmount += extrusionAmount - lastExtrusionAmount;
                        subLayerDetails.ExtrusionDistance += distance;
                        subLayerDetails.BoundingBox.Contain(new Vector2d(x, y));
                        subLayerDetails.UnscaledCenterOfMass += new Vector2d(averageX, averageY) * (extrusionAmount - lastExtrusionAmount);
                        subLayerDetails.ExtrusionTime += distance / feedrate;


                        lastX = x;
                        lastY = y;
                        lastExtrusionAmount = extrusionAmount;

                        break;
                }
            }

            if (currentLayer != null)
                layers.Add(currentLayer);
            return layers;
        }

    }
}