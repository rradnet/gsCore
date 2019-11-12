﻿using g3;
using System;
using System.Collections.Generic;

namespace gs
{
    public interface IVisualizer
    {
        void BeginGCodeLineStream();
        void ProcessGCodeLine(GCodeLine line);
        void EndGCodeLineStream();

        void PrintLayerCompleted(PrintLayerData printLayerData);

        event Action<ToolpathPreviewVertex[], int[], int> OnMeshGenerated;
        event Action<List<Vector3d>, int> OnLineGenerated;
    }



    public struct ToolpathPreviewVertex
    {
        public Vector3d point;
        public FillTypeFlags fillType;
        public Vector2d dimensions;
        public double feedrate;
        public int layerIndex;
        public int pointCount;
        public double brightness;

        public ToolpathPreviewVertex(Vector3d point, FillTypeFlags fillType, Vector2d dimensions, double feedrate, int layerIndex, int pointCount, double brightness)
        {
            this.point = point;
            this.fillType = fillType;
            this.dimensions = dimensions;
            this.feedrate = feedrate;
            this.layerIndex = layerIndex;
            this.pointCount = pointCount;
            this.brightness = brightness;
        }
    }
}