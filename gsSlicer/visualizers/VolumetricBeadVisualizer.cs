using g3;
using System;
using System.Collections.Generic;
using gs.interfaces;


namespace gs
{
    public class VolumetricBeadVisualizer : IVisualizer
    {        
        Vector3d position = default;
        double feedrate = default;
        double extrusion = default;
        Vector2d dimensions = default;
        FillTypeFlags fillType = default;
        int layerIndex = default;
        int pointCount = default;
        Vector2d positionShift;

        List<PrintVertex> toolpath = null;
        PrintVertex lastVertex = default;

        public SingleMaterialFFFSettings settings = null;

        public event Action<ToolpathPreviewVertex[], int[], int> OnMeshGenerated;
        public event Action<List<Vector3d>, int> OnLineGenerated;

        public string Name => "Bead Visualizer";

        public Dictionary<int, FillType> FillTypes { get; protected set; } = new Dictionary<int, FillType>()
        {
            {(int)FillTypeFlags.Unknown, new FillType("Unknown", new Vector3f(0.5, 0.5, 0.5))},
            {(int)FillTypeFlags.PerimeterShell, new FillType("Inner Perimeter", new Vector3f(1, 0, 0))},
            {(int)FillTypeFlags.OutermostShell, new FillType("Outer Perimeter", new Vector3f(1, 1, 0))},
            {(int)FillTypeFlags.OpenShellCurve, new FillType("Open Mesh Curve", new Vector3f(0, 1, 1))},
            {(int)FillTypeFlags.SolidInfill, new FillType("Solid Fill", new Vector3f(0, 0.5f, 1))},
            {(int)FillTypeFlags.SparseInfill, new FillType("Sparse Fill", new Vector3f(0.5f, 0, 1))},
            {(int)FillTypeFlags.SupportMaterial, new FillType("Support", new Vector3f(1, 0, 1))},
            {(int)FillTypeFlags.BridgeSupport, new FillType("Bridge", new Vector3f(0, 0, 1))},
        };

        private FixedRangeCustomDataDetails customDataBeadWidth = 
            new FixedRangeCustomDataDetails(0.1f, 0.8f, () => "Bead Width");
        public IVisualizerCustomDataDetails CustomDataDetails0 => customDataBeadWidth;


        private AdaptiveRangeCustomDataDetails customDataFeedRate =
            new AdaptiveRangeCustomDataDetails(() => "Feed Rate");
        public IVisualizerCustomDataDetails CustomDataDetails1 => customDataFeedRate;

        public IVisualizerCustomDataDetails CustomDataDetails2 => null;
        public IVisualizerCustomDataDetails CustomDataDetails3 => null;
        public IVisualizerCustomDataDetails CustomDataDetails4 => null;
        public IVisualizerCustomDataDetails CustomDataDetails5 => null;

        public VolumetricBeadVisualizer()
        {
        }

        public void BeginGCodeLineStream()
        {
            // TODO: Find a more decoupled way of passing in bed info.
            if (settings == null)
                throw new Exception("Should assign settings field before starting stream");

            dimensions.x = settings.Machine.NozzleDiamMM;
            dimensions.y = settings.LayerHeightMM;

            Vector2d originRepositioning = new Vector2d(settings.Machine.BedOriginFactorX, settings.Machine.BedOriginFactorY);
            Vector2d bedSize = new Vector2d(settings.Machine.BedSizeXMM, settings.Machine.BedSizeYMM);
            positionShift = originRepositioning * bedSize;
        }

        public void ProcessGCodeLine(GCodeLine line)
        {
            if (line == null || line.type == GCodeLine.LType.Blank)
                return;

            pointCount++;

            if (line.comment != null && line.comment.Contains("layer") && !line.comment.Contains("feature"))
            {
                if (toolpath != null)
                {
                    Emit(toolpath, layerIndex, pointCount - toolpath.Count);
                    toolpath = null;
                }
                layerIndex++;

                return;
            }

            ExtractPositionFeedrateAndExtrusion(line, ref position, ref feedrate, ref extrusion);
            ExtractDimensions(line, ref dimensions);
            ExtractFillType(line, ref fillType);

            PrintVertex vertex = new PrintVertex
            {
                Position = position,
                FeedRate = feedrate,
                Extrusion = new Vector3d(extrusion, 0, 0),
                Dimensions = dimensions,
                Source = fillType,
            };

            if (line.type == GCodeLine.LType.GCode)
            {
                if (toolpath == null)
                {
                    if (extrusion > lastVertex.Extrusion.x)
                    {
                        lastVertex.Source = fillType;
                        lastVertex.Dimensions = vertex.Dimensions;
                        lastVertex.FeedRate = vertex.FeedRate;
                        lastVertex.ExtendedData = vertex.ExtendedData;

                        toolpath = new List<PrintVertex> { lastVertex, vertex };
                    }
                    else
                    {
                        OnLineGenerated(new List<Vector3d>() { lastVertex.Position, vertex.Position }, layerIndex);
                    }
                }
                else
                {
                    toolpath.Add(vertex);
                    if (extrusion <= lastVertex.Extrusion.x)
                    {
                        Emit(toolpath, layerIndex, pointCount - toolpath.Count);
                        toolpath = null;
                    }
                }
            }

            lastVertex = vertex;
        }

        public void EndGCodeLineStream()
        {
            if (toolpath != null)
            {
                Emit(toolpath, layerIndex, pointCount - toolpath.Count);
            }
        }

        private void Emit(List<PrintVertex> toolpath, int layerIndex, int startPointCount)
        {

            Func<Tuple<ToolpathPreviewVertex[], int[]>> func = () => {

                List<ToolpathPreviewVertex> vertices = new List<ToolpathPreviewVertex>();
                List<int> triangles = new List<int>();

                ToolpathPreviewJoint[] joints = new ToolpathPreviewJoint[toolpath.Count];

                joints[joints.Length - 1] = GenerateMiterJoint(toolpath, joints.Length - 1, layerIndex, positionShift, startPointCount, vertices);

                for (int i = joints.Length - 2; i > 0; i--)
                {
                    Vector3d a = toolpath[i - 1].Position;
                    Vector3d b = toolpath[i].Position;
                    Vector3d c = toolpath[i + 1].Position;
                    Vector3d ab = b - a;
                    Vector3d bc = c - b;
                    var angleRad = SignedAngleRad(ab.xy, bc.xy);
                    if (Math.Abs(angleRad) > 0.698132)
                    {
                        if (angleRad < 0)
                        {
                            joints[i] = GenerateRightBevelJoint(toolpath, i, layerIndex, positionShift, startPointCount, vertices, triangles);
                        }
                        else
                        {
                            joints[i] = GenerateLeftBevelJoint(toolpath, i, layerIndex, positionShift, startPointCount, vertices, triangles);
                        }
                    }
                    else
                    {
                        joints[i] = GenerateMiterJoint(toolpath, i, layerIndex, positionShift, startPointCount, vertices);
                    }
                }

                joints[0] = GenerateMiterJoint(toolpath, 0, layerIndex, positionShift, startPointCount, vertices);

                AddEdges(joints, triangles);

                return new Tuple<ToolpathPreviewVertex[], int[]>(vertices.ToArray(), triangles.ToArray());
            };
            func.BeginInvoke((ar) => EndEmit(func, layerIndex, ar), null);
        }

        private void AddEdges(ToolpathPreviewJoint[] joints, List<int> triangles)
        {
            for (int i = joints.Length - 2; i >= 0; i--)
            {

                var start = joints[i];
                var end = joints[i + 1];

                triangles.Add(start.out0);
                triangles.Add(start.out1);
                triangles.Add(end.in0);

                triangles.Add(end.in0);
                triangles.Add(start.out1);
                triangles.Add(end.in1);

                triangles.Add(start.out1);
                triangles.Add(start.out2);
                triangles.Add(end.in1);

                triangles.Add(end.in1);
                triangles.Add(start.out2);
                triangles.Add(end.in2);

                triangles.Add(start.out2);
                triangles.Add(start.out3);
                triangles.Add(end.in2);

                triangles.Add(end.in2);
                triangles.Add(start.out3);
                triangles.Add(end.in3);

                triangles.Add(start.out3);
                triangles.Add(start.out0);
                triangles.Add(end.in3);

                triangles.Add(end.in3);
                triangles.Add(start.out0);
                triangles.Add(end.in0);
            }
        }

        private ToolpathPreviewJoint GenerateMiterJoint(List<PrintVertex> toolpath, int toolpathIndex, int layerIndex, Vector2d positionShift, int startPointCount, List<ToolpathPreviewVertex> vertices)
        {

            double miterSecant = 1;
            Vector3d miterNormal;

            if (toolpathIndex == 0)
            {
                Vector3d miterTangent = toolpath[1].Position - toolpath[0].Position;
                miterNormal = new Vector3d(-miterTangent.y, miterTangent.x, 0).Normalized;
            }
            else if (toolpathIndex == toolpath.Count - 1)
            {
                Vector3d miterTangent = toolpath[toolpathIndex].Position - toolpath[toolpathIndex - 1].Position;
                miterNormal = new Vector3d(-miterTangent.y, miterTangent.x, 0).Normalized;
            }
            else
            {
                Vector3d a = toolpath[toolpathIndex - 1].Position;
                Vector3d b = toolpath[toolpathIndex].Position;
                Vector3d c = toolpath[toolpathIndex + 1].Position;
                Vector3d ab = (b - a).Normalized;
                Vector3d bc = (c - b).Normalized;
                miterNormal = GetNormalAndSecant(ab, bc, out miterSecant);
            }

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;
            var fillType = (FillTypeFlags)toolpath[toolpathIndex].Source;
            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            joint.in0 = joint.out0 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in1 = joint.out1 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);
            joint.in2 = joint.out2 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(-0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in3 = joint.out3 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            return joint;
        }

        private ToolpathPreviewJoint GenerateRightBevelJoint(List<PrintVertex> toolpath, int toolpathIndex, int layerIndex, Vector2d positionShift, int startPointCount, List<ToolpathPreviewVertex> vertices, List<int> triangles)
        {

            Vector3d a = toolpath[toolpathIndex - 1].Position;
            Vector3d b = toolpath[toolpathIndex].Position;
            Vector3d c = toolpath[toolpathIndex + 1].Position;
            Vector3d ab = (b - a).Normalized;
            Vector3d bc = (c - b).Normalized;
            Vector3d miterNormal = GetNormalAndSecant(ab, bc, out double miterSecant);
            Vector3d miterTangent = ab + bc;

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;
            var fillType = (FillTypeFlags)toolpath[toolpathIndex].Source;
            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            var bevelNormalIn = GetNormalAndSecant(ab, miterTangent, out double bevelSecantIn);
            joint.in0 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, bevelNormalIn, new Vector2d(0.5, -0.5), bevelSecantIn, 0, pointCount);

            var bevelNormalOut = GetNormalAndSecant(miterTangent, bc, out double bevelSecantOut);
            joint.out0 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, bevelNormalOut, new Vector2d(0.5, -0.5), bevelSecantOut, 0, pointCount);

            joint.in1 = joint.out1 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);
            joint.in2 = joint.out2 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(-0.5, -0.5), miterSecant, 0, pointCount);
            joint.in3 = joint.out3 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            triangles.Add(joint.in0);
            triangles.Add(joint.in1);
            triangles.Add(joint.out0);

            triangles.Add(joint.in0);
            triangles.Add(joint.out0);
            triangles.Add(joint.in3);

            return joint;
        }

        private ToolpathPreviewJoint GenerateLeftBevelJoint(List<PrintVertex> toolpath, int toolpathIndex, int layerIndex, Vector2d positionShift, int startPointCount, List<ToolpathPreviewVertex> vertices, List<int> triangles)
        {

            Vector3d a = toolpath[toolpathIndex - 1].Position;
            Vector3d b = toolpath[toolpathIndex].Position;
            Vector3d c = toolpath[toolpathIndex + 1].Position;
            Vector3d ab = (b - a).Normalized;
            Vector3d bc = (c - b).Normalized;
            Vector3d miterNormal = GetNormalAndSecant(ab, bc, out double miterSecant);
            Vector3d miterTangent = ab + bc;

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;
            var fillType = (FillTypeFlags)toolpath[toolpathIndex].Source;
            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            joint.in0 = joint.out0 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in1 = joint.out1 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);

            var bevelNormalIn = GetNormalAndSecant(ab, miterTangent, out double bevelSecantIn);
            joint.in2 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, bevelNormalIn, new Vector2d(-0.5f, -0.5f), bevelSecantIn, 0, pointCount);

            var bevelNormalOut = GetNormalAndSecant(miterTangent, bc, out double bevelSecantOut);
            joint.out2 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, bevelNormalOut, new Vector2d(-0.5f, -0.5f), bevelSecantOut, 0, pointCount);

            joint.in3 = joint.out3 = AddVertex(vertices, positionShift, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            triangles.Add(joint.in2);
            triangles.Add(joint.in3);
            triangles.Add(joint.out2);

            triangles.Add(joint.in2);
            triangles.Add(joint.out2);
            triangles.Add(joint.in1);

            return joint;
        }

        private int AddVertex(List<ToolpathPreviewVertex> vertices, Vector2d positionShift, int layerIndex, Vector3d point, FillTypeFlags fillType, Vector2d dimensions, double feedrate, Vector3d miterNormal, Vector2d crossSectionVertex, double secant, float brightness, int pointCount)
        {
            Vector3d offset = miterNormal * (dimensions.x * crossSectionVertex.x * secant) + new Vector3d(0, 0, dimensions.y * crossSectionVertex.y);
            Vector3d vertex = point - new Vector3d(positionShift.x, positionShift.y, 0) + offset;

            Vector3f color = FillTypes[(int)FillTypeFlags.Unknown].Color;
            if (FillTypes.TryGetValue((int)fillType, out var fillInfo))
            {
                color = fillInfo.Color;
            }

            vertices.Add(new ToolpathPreviewVertex(vertex,
                (int)fillType, layerIndex, color, brightness,
                (float)dimensions.x, (float)feedrate));

            // Update adaptive ranges for custom data
            customDataFeedRate.ObserveValue((float)feedrate);

            return vertices.Count - 1;
        }

        private static Vector3d GetNormalAndSecant(Vector3d ab, Vector3d bc, out double secant)
        {
            secant = 1 / Math.Cos(Vector3d.AngleR(ab, bc) * 0.5);
            Vector3d tangent = ab + bc;
            return new Vector3d(-tangent.y, tangent.x, 0).Normalized;
        }

        private static double SignedAngleRad(Vector2d a, Vector2d b)
        {
            var angleB = Math.Atan2(b.y, b.x);
            var angleA = Math.Atan2(a.y, a.x);
            var ret = angleB - angleA;
            if (ret > Math.PI)
                ret -= Math.PI * 2;
            if (ret < -Math.PI)
                ret += Math.PI * 2;
            return ret;
        }

        private struct ToolpathPreviewJoint
        {
            public int in0;
            public int in1;
            public int in2;
            public int in3;

            public int out0;
            public int out1;
            public int out2;
            public int out3;
        }

        private void EndEmit(Func<Tuple<ToolpathPreviewVertex[], int[]>> func, int layerIndex, IAsyncResult ar)
        {
            var mesh = func.EndInvoke(ar);
            OnMeshGenerated.Invoke(mesh.Item1, mesh.Item2, layerIndex);
        }

        private static void ExtractPositionFeedrateAndExtrusion(GCodeLine line, ref Vector3d position, ref double feedrate, ref double extrusion)
        {
            if (line.parameters != null)
            {
                foreach (var param in line.parameters)
                {
                    switch (param.identifier)
                    {
                        case "X": position.x = param.doubleValue; break;
                        case "Y": position.y = param.doubleValue; break;
                        case "Z": position.z = param.doubleValue; break;
                        case "F": feedrate = param.doubleValue; break;
                        case "E": extrusion = param.doubleValue; break;
                    }
                }
            }
        }

        private static void ExtractDimensions(GCodeLine line, ref Vector2d dimensions)
        {
            if (line.comment != null && line.comment.Contains("tool"))
            {
                foreach (var word in line.comment.Split(' '))
                {
                    int i = word.IndexOf('W');
                    if (i >= 0)
                        dimensions.x = double.Parse(word.Substring(i + 1));
                    i = word.IndexOf('H');
                    if (i >= 0)
                        dimensions.y = double.Parse(word.Substring(i + 1));
                }
            }
        }

        private static void ExtractFillType(GCodeLine line, ref FillTypeFlags fillType)
        {
            if (line.comment != null)
            {
                int indexOfFillType = line.comment.IndexOf("feature");
                if (indexOfFillType >= 0)
                {
                    var featureName = line.comment.Substring(indexOfFillType + 8).Trim();
                    fillType = SingleMaterialFFFCompiler.FlagFromFeatureName(featureName);
                }
            }
        }

        public void PrintLayerCompleted(PrintLayerData printLayerData)
        {
            throw new NotImplementedException();
        }
    }
}
