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

        public void CompareFiles(string gcodeFilePathExpected, string gcodeFilePathResult)
        {
            var resultLayersDetails = GetLayerFeatureInfo(LoadGCode(gcodeFilePathResult));
            var expectedLayersDetails = GetLayerFeatureInfo(LoadGCode(gcodeFilePathExpected));

            if (resultLayersDetails.Count != expectedLayersDetails.Count)
            {
                throw new LayerCountMismatch($"Expected {expectedLayersDetails.Count} layers but the result has {resultLayersDetails.Count}.");
            }

            for (int layerNumber = 0; layerNumber < resultLayersDetails.Count; layerNumber++)
            {
                Dictionary<FillTypeFlags, FeatureInfo> resultLayerDetails = resultLayersDetails[layerNumber];
                Dictionary<FillTypeFlags, FeatureInfo> expectedLayerDetails = expectedLayersDetails[layerNumber];


                foreach (var key in resultLayerDetails.Keys)
                    if (!expectedLayerDetails.ContainsKey(key))
                        throw new MissingFeature($"Result has unexpected feature {key}");

                foreach (var key in expectedLayerDetails.Keys)
                    if (!resultLayerDetails.ContainsKey(key))
                        throw new MissingFeature($"Result was missing expected feature {key}");

                foreach (FillTypeFlags fillType in resultLayerDetails.Keys)
                {

                    try
                    {
                        resultLayerDetails[fillType].AssertEqualsExpected(expectedLayerDetails[fillType]);
                    }
                    catch (Exception e)
                    {
                        e.Data.Add("Layer", layerNumber);
                        throw;
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
                            if (!currentLayer.TryGetValue(lineFillType, out subLayerDetails))
                            {
                                subLayerDetails = new FeatureInfo();
                                currentLayer.Add(lineFillType, subLayerDetails);
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


                        double f = GCodeUtil.UnspecifiedValue;
                        if (GCodeUtil.TryFindParamNum(line.parameters, "F", ref f))
                            feedrate = f;

                        double extrusionAmount = GCodeUtil.UnspecifiedValue;
                        if (GCodeUtil.TryFindParamNum(line.parameters, "E", ref extrusionAmount) &&
                            extrusionAmount >= lastExtrusionAmount)
                        {
                            subLayerDetails.Extrusion += extrusionAmount - lastExtrusionAmount;
                            subLayerDetails.Distance += distance;
                            subLayerDetails.BoundingBox.Contain(new Vector2d(x, y));
                            subLayerDetails.CenterOfMass += new Vector2d(averageX, averageY) * (extrusionAmount - lastExtrusionAmount);
                            subLayerDetails.Duration += distance / feedrate;

                            lastExtrusionAmount = extrusionAmount;
                        }

                        lastX = x;
                        lastY = y;

                        break;
                }
            }

            if (currentLayer != null)
                layers.Add(currentLayer);
            return layers;
        }

    }
}