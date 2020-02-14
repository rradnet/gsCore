using System;
using System.Collections.Generic;
using g3;
using gs;
using gsCore.FunctionalTests.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace gsCore.FunctionalTests.Utility
{
    public class FeatureInfoFactoryFFF : IFeatureInfoFactory<FeatureInfo>
    {

        private PrintVertex VertexPrevious;
        private PrintVertex VertexCurrent;

        private FeatureInfo currentFeatureInfo;

        private readonly List<string> endFeatureComments = new List<string>()
        {
            "retract", "travel"
        };

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

            double x = VertexPrevious.Position.x;
            double y = VertexPrevious.Position.y;

            GCodeUtil.TryFindParamNum(line.parameters, "X", ref x);
            GCodeUtil.TryFindParamNum(line.parameters, "Y", ref y);

            VertexCurrent.Position = new Vector3d(x, y, 0);

            double f = GCodeUtil.UnspecifiedValue;
            if (GCodeUtil.TryFindParamNum(line.parameters, "F", ref f))
                VertexCurrent.FeedRate = f;

            double extrusionAmount = GCodeUtil.UnspecifiedValue;
            bool featureActive = GCodeUtil.TryFindParamNum(line.parameters, "E", ref extrusionAmount) &&
                                 extrusionAmount > VertexPrevious.Extrusion.x &&
                                 currentFeatureInfo != null;

            foreach (var s in endFeatureComments)
                if (!string.IsNullOrWhiteSpace(line.comment) && line.comment.ToLower().Contains(s))
                    featureActive = false;

            if (featureActive)
            {
                Vector2d average = new Segment2d(VertexCurrent.Position.xy, VertexPrevious.Position.xy).Center;
                double distance = VertexCurrent.Position.Distance(VertexPrevious.Position);
                double extrusion = extrusionAmount - VertexPrevious.Extrusion.x;

                currentFeatureInfo.Extrusion += extrusion;
                currentFeatureInfo.Distance += distance;
                currentFeatureInfo.BoundingBox.Contain(VertexPrevious.Position.xy);
                currentFeatureInfo.BoundingBox.Contain(VertexCurrent.Position.xy);
                currentFeatureInfo.UnweightedCenterOfMass += average * extrusion;
                currentFeatureInfo.Duration += distance / VertexCurrent.FeedRate;

                VertexCurrent.Extrusion = new Vector3d(extrusionAmount, 0, 0);
            }

            VertexPrevious = new PrintVertex(VertexCurrent);
        }

        public void Initialize()
        {
            VertexPrevious = new PrintVertex(Vector3d.Zero, 0, Vector2d.Zero);
            VertexCurrent = new PrintVertex(VertexPrevious);
            currentFeatureInfo = null;
        }
    }
}