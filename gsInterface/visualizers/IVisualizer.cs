using g3;
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

        string Name { get; }
        Dictionary<int, FillType> FillTypes { get; }

        IVisualizerCustomDataDetails CustomDataDetails0 { get; }
        IVisualizerCustomDataDetails CustomDataDetails1 { get; }
        IVisualizerCustomDataDetails CustomDataDetails2 { get; }
        IVisualizerCustomDataDetails CustomDataDetails3 { get; }
        IVisualizerCustomDataDetails CustomDataDetails4 { get; }
        IVisualizerCustomDataDetails CustomDataDetails5 { get; }

    }

    public interface IVisualizerCustomDataDetails
    {
        string Label { get; }
        float RangeMin { get; }
        float RangeMax { get; }
    }

    public struct FillType
    {
        public string Label { get; }

        public Vector3f Color { get; }

        public FillType(string label, Vector3f color)
        {
            Label = label;
            Color = color;
        }
    }


    public struct ToolpathPreviewVertex
    {
        public Vector3d point;

        public int fillType;
        public int layerIndex;

        public Vector3f color;
        public float brightness;

        public float[] customData;

        public ToolpathPreviewVertex(Vector3d point, int fillType, int layerIndex, Vector3f color, float brightness,
            float? customField0 = null, float? customField1 = null, float? customField2 = null, 
            float? customField3 = null, float? customField4 = null, float? customField5 = null)
        {
            this.point = point;

            this.fillType = fillType;
            this.layerIndex = layerIndex;
            this.color = color;
            this.brightness = brightness;

            customData = new float[6];
            customData[0] = customField0 ?? 0;
            customData[1] = customField1 ?? 0;
            customData[2] = customField2 ?? 0;
            customData[3] = customField3 ?? 0;
            customData[4] = customField4 ?? 0;
            customData[5] = customField5 ?? 0;
        }
    }
}