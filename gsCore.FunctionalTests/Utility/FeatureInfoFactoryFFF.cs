using System;
using g3;
using gs;
using gsCore.FunctionalTests.Models;

namespace gsCore.FunctionalTests.Utility
{
    public class FeatureInfoFactoryFFF : IFeatureInfoFactory<FeatureInfo>
    {
        private double lastExtrusionAmount;
        private double lastX;
        private double lastY;
        private double feedrate;
        private FeatureInfo currentFeatureInfo;

        public FeatureInfo SwitchFeature(FillTypeFlags featureType)
        {
            var result = currentFeatureInfo;
            currentFeatureInfo = new FeatureInfo(featureType);
            if (result?.Extrusion > 0)
                return result;
            else
                return null;
        }

        public void ObserveGcodeLine(GCodeLine line)
        {
            if (line.type != GCodeLine.LType.GCode)
                return;

            double x = GCodeUtil.UnspecifiedValue;
            double y = GCodeUtil.UnspecifiedValue;

            bool found_x = GCodeUtil.TryFindParamNum(line.parameters, "X", ref x);
            bool found_y = GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y);

            if (!found_x || !found_y)
                return;

            double averageX = (lastX + x) * 0.5;
            double averageY = (lastY + y) * 0.5;
            double distance = Math.Sqrt((lastX - x) * (lastX - x) + (lastY - y) * (lastY - y));

            double f = GCodeUtil.UnspecifiedValue;
            if (GCodeUtil.TryFindParamNum(line.parameters, "F", ref f))
                feedrate = f;

            double extrusionAmount = GCodeUtil.UnspecifiedValue;
            if (GCodeUtil.TryFindParamNum(line.parameters, "E", ref extrusionAmount) &&
                extrusionAmount >= lastExtrusionAmount && currentFeatureInfo != null)
            {
                currentFeatureInfo.Extrusion += extrusionAmount - lastExtrusionAmount;
                currentFeatureInfo.Distance += distance;
                currentFeatureInfo.BoundingBox.Contain(new Vector2d(x, y));
                currentFeatureInfo.CenterOfMass += new Vector2d(averageX, averageY) * (extrusionAmount - lastExtrusionAmount);
                currentFeatureInfo.Duration += distance / feedrate;

                lastExtrusionAmount = extrusionAmount;
            }

            lastX = x;
            lastY = y;
        }

        public void Initialize()
        {
            lastExtrusionAmount = 0;
            lastX = 0;
            lastY = 0;
            feedrate = 0;
            currentFeatureInfo = null;
        }
    }
}