using System;
using System.Collections.Generic;
using g3;

namespace gs
{
	[Flags]
	public enum FillTypeFlags
	{
		Unknown = 0,

		PerimeterShell = 1,
		OutermostShell = 1<<1,
		OuterPerimeter = PerimeterShell | OutermostShell,

		InteriorShell = 1<<2,

		OpenShellCurve = 1<<3,    // ie for single-line-wide features

		SolidInfill = 1<<8,
		SparseInfill = 1<<9,

        SupportMaterial = 1<<10,
		BridgeSupport = 1<<11
	}

    public class FillVertexInfo
    {
    }

    public class FillSegmentInfo : ICloneable
    {
        public bool IsConnector;
        public bool IsSupport;

        public object Clone()
        {
            return (FillSegmentInfo)this.MemberwiseClone();
        }

        public void Reverse()
        {
        }
    }

    public interface IFillElementPolygon
    {
        FillTypeFlags TypeFlags { get; set; }
        double Perimeter { get; }
    }

    public interface IFillElementPolyline
    {
        FillTypeFlags TypeFlags { get; set; }
        double ArcLength { get; }
    }

    /// <summary>
    /// Things that are common to FillPolylineGeneric and FillPolylineGeneric
    /// </summary>
    public abstract class FillElement<TVertexInfo, TSegmentInfo>
        where TVertexInfo : FillVertexInfo, new()
        where TSegmentInfo : FillSegmentInfo, new()
    {
        public class PointData
        {
            public Vector2d Vertex;
            public TVertexInfo VertexInfo;
            public TSegmentInfo SegmentInfo;
        }
    }

    /// <summary>
    /// Additive polygon fill curve
    /// </summary>
    public abstract class FillPolygonGeneric<TVertexInfo, TSegmentInfo> : 
        FillElement<TVertexInfo, TSegmentInfo>, IFillElementPolygon
        where TVertexInfo : FillVertexInfo, new()
        where TSegmentInfo : FillSegmentInfo, new()
    {
        protected Polygon2d Polygon = new Polygon2d();
        protected List<TVertexInfo> VertexInfo = new List<TVertexInfo>();
        protected List<TSegmentInfo> SegmentInfo = new List<TSegmentInfo>();

        public bool HasTypeFlag(FillTypeFlags f) { return (TypeFlags & f) == f; }
        public double CustomThickness { get; set; }

        // Expose some properties & methods from underlying Polygon
        public int VertexCount { get => Polygon.VertexCount; }
        public double Perimeter { get => Polygon.Perimeter; }
        public Vector2d this[int i] { get => Polygon[i]; }
        public IEnumerable<Vector2d> Vertices { get => Polygon.VerticesItr(false); }
        public FillTypeFlags TypeFlags { get; set; } = FillTypeFlags.Unknown;

        public Segment2d Segment(int i) { return Polygon.Segment(i); }
        public double DistanceSquared(Vector2d pt, out int iNearSeg, out double fNearSegT)
        {
            return Polygon.DistanceSquared(pt, out iNearSeg, out fNearSegT);
        }

        public void AppendVertex(Vector2d pt, TVertexInfo vInfo = null, TSegmentInfo sInfo = null)
        {
            Polygon.AppendVertex(pt);
            VertexInfo.Add(vInfo);

            if (Polygon.VertexCount > 0)
                SegmentInfo.Add(sInfo);
            else if (sInfo != null)
                throw new Exception("Cannot add SegmentInfo to the first vertex.");
        }

        public void AppendVertex(Vector2d pt, TSegmentInfo sInfo)
        {
            AppendVertex(pt, null, sInfo);
        }

        public PointData GetPoint(int i, bool reverse)
        {
            if (reverse)
            {
                var segReversed = (TSegmentInfo)SegmentInfo[i].Clone();
                segReversed?.Reverse();
                return new PointData()
                {
                    Vertex = Polygon[i],
                    VertexInfo = VertexInfo[i],
                    SegmentInfo = segReversed,
                };
            }
            else
            {
                return new PointData()
                {
                    Vertex = Polygon[i],
                    VertexInfo = VertexInfo[i],
                    SegmentInfo = SegmentInfo[(i + Polygon.VertexCount - 1) % Polygon.VertexCount]
                };
            }
        }
    }

    /// <summary>
    /// Additive polyline fill curve
    /// </summary>
    public abstract class FillPolylineGeneric<TVertexInfo, TSegmentInfo> :
        FillElement<TVertexInfo, TSegmentInfo>, IFillElementPolyline
        where TVertexInfo : FillVertexInfo, new()
        where TSegmentInfo : FillSegmentInfo, new()
    {
        public FillTypeFlags TypeFlags { get; set; } = FillTypeFlags.Unknown;
        public bool HasTypeFlag(FillTypeFlags f) { return (TypeFlags & f) == f; }
        public double CustomThickness { get; set; }

        // Expose some properties & methods from underlying Polyline
        public int VertexCount { get => Polyline.VertexCount; }
        public double ArcLength { get => Polyline.ArcLength; }
        public Vector2d Start { get => Polyline.Vertices[0]; }
        public Vector2d End { get => Polyline.Vertices[Polyline.VertexCount - 1]; }
        public IEnumerable<Vector2d> Vertices { get => Polyline.Vertices; }
        public Segment2d Segment(int i) { return Polyline.Segment(i); }
        public Vector2d this[int i] { get => Polyline[i]; }

        protected PolyLine2d Polyline = new PolyLine2d();
        protected List<TVertexInfo> VertexInfo = new List<TVertexInfo>();
        protected List<TSegmentInfo> SegmentInfo = new List<TSegmentInfo>();

        public void AppendVertex(Vector2d pt, TVertexInfo vInfo = null, TSegmentInfo sInfo = null)
        {
            Polyline.AppendVertex(pt);
            VertexInfo.Add(vInfo);

            if (Polyline.VertexCount > 0)
                SegmentInfo.Add(sInfo);
            else if (sInfo != null)
                throw new Exception("Cannot add SegmentInfo to the first vertex.");
        }

        public void AppendVertex(Vector2d pt, TSegmentInfo sInfo)
        {
            AppendVertex(pt, null, sInfo);
        }

        public PointData GetPoint(int i, bool reverse)
        {
            if (reverse)
            {

                var segReversed = i >= SegmentInfo.Count - 1 ? null : (TSegmentInfo)SegmentInfo[i]?.Clone();
                segReversed?.Reverse();
                return new PointData()
                {
                    Vertex = Polyline[i],
                    VertexInfo = VertexInfo[i],
                    SegmentInfo = segReversed,
                };
            }
            else
            {
                return new PointData()
                {
                    Vertex = Polyline[i],
                    VertexInfo = VertexInfo[i],
                    SegmentInfo = i == 0 ? null : SegmentInfo[i - 1]
                };
            }
        }

        public void Reverse()
        {
            // Reverse Lists
            Polyline.Reverse();
            VertexInfo.Reverse();
            SegmentInfo.Reverse();

            // Reverse each segment in case segment data is directional
            foreach (var seg in SegmentInfo)
                seg?.Reverse();
        }
    }


    public class FillPolygon2d : FillPolygonGeneric<FillVertexInfo, FillSegmentInfo>
    {
        public FillPolygon2d()
        { }

        public FillPolygon2d(IEnumerable<Vector2d> vertices)
        {
            foreach (var v in vertices)
                AppendVertex(v);
        }
        
    }

    public class FillPolyline2d : FillPolylineGeneric<FillVertexInfo, FillSegmentInfo>
    {
        public FillPolyline2d()
        { }
           
        public FillPolyline2d(IEnumerable<Vector2d> vertices)
        {
            foreach (var v in vertices)
                AppendVertex(v);
        }

        public void Trim(double v)
        {
            Polyline.Trim(v);

            // Remove any vertex info that was trimmed away.
            VertexInfo.RemoveRange(Polyline.VertexCount, VertexInfo.Count - Polyline.VertexCount);

            // Remove any segment info that was trimmed away.
            VertexInfo.RemoveRange(Polyline.VertexCount - 1, SegmentInfo.Count - 1 - Polyline.VertexCount);
        }
    }
}
