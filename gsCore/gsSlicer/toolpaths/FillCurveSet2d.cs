using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using g3;

namespace gs
{
    /// <summary>
    /// Collection of loop and curve centerlines of fill curves
    /// 
    /// [TODO] support thickness variation along curves?
    /// </summary>
	public class FillCurveSet2d
	{
		public List<IFillElementPolygon> Loops = new List<IFillElementPolygon>();
		public List<IFillElementPolyline> Curves = new List<IFillElementPolyline>();

		public FillCurveSet2d()
		{
		}

		public void Append(GeneralPolygon2d poly, FillTypeFlags typeFlags) {
			Loops.Add(new FillPolygon2d(poly.Outer.VerticesItr(false)) { TypeFlags = typeFlags });
			foreach (var h in poly.Holes)
				Loops.Add(new FillPolygon2d(h.VerticesItr(false)) { TypeFlags = typeFlags });
		}

		public void Append(List<GeneralPolygon2d> polys, FillTypeFlags typeFlags) {
			foreach (var p in polys)
				Append(p, typeFlags);
		}

		public void Append(Polygon2d poly, FillTypeFlags typeFlags) {
			Loops.Add(new FillPolygon2d(poly.VerticesItr(false)) { TypeFlags = typeFlags } );
        }

        public void Append(List<Polygon2d> polys, FillTypeFlags typeFlags) {
            foreach (var p in polys)
				Append(p, typeFlags);
        }

        public void Append(IFillElementPolygon loop) {
			Loops.Add(loop);
		}

        public void Append(List<IFillElementPolygon> loops) {
            foreach ( var l in loops )
			    Loops.Add(l);
		}


        public void Append(IFillElementPolyline curve) {
			Curves.Add(curve);
		}

		public void Append(List<IFillElementPolyline> curves) {
			foreach (var p in curves)
				Append(p);
		}


        public void SetFlags(FillTypeFlags flags)
        {
            foreach (var loop in Loops)
                loop.TypeFlags = flags;
            foreach (var curve in Curves)
                curve.TypeFlags = flags;
        }

        public void AddFlags(FillTypeFlags flags)
        {
            foreach (var loop in Loops)
                loop.TypeFlags |= flags;
            foreach (var curve in Curves)
                curve.TypeFlags |= flags;
        }


		public double TotalLength()
		{
			double len = 0;
			foreach (var loop in Loops)
				len += loop.Perimeter;
			foreach (var curve in Curves)
				len += curve.ArcLength;
			return len;
		}
	}
}
