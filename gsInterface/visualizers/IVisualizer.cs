﻿using g3;
using System;
using System.Collections.Generic;

namespace gs.interfaces
{
    public interface IVisualizer
    {
        void BeginGCodeLineStream();
        void ProcessGCodeLine(GCodeLine line);
        void EndGCodeLineStream();

        event Action<ToolpathPreviewVertex[], int[], int> OnMeshGenerated;
        event Action<List<Vector3d>, int> OnLineGenerated;
    }



    public struct ToolpathPreviewVertex
    {
        public Vector3d point;
        public int fillType;
        public Vector2d dimensions;
        public double feedrate;
        public int layerIndex;
        public int pointCount;
        public Vector3f color;

        public ToolpathPreviewVertex(Vector3d point, int fillType, Vector2d dimensions, double feedrate, int layerIndex, int pointCount, Vector3f color)
        {
            this.point = point;
            this.fillType = fillType;
            this.dimensions = dimensions;
            this.feedrate = feedrate;
            this.layerIndex = layerIndex;
            this.pointCount = pointCount;
            this.color = color;
        }
    }
}