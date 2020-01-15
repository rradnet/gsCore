using System;

namespace gs
{
    public static class FeatureTypeLabelerFFF
    {
        private static FeatureTypeLabeler singleton = new FeatureTypeLabeler(new Tuple<FillTypeFlags, string>[]
            {
                Tuple.Create(FillTypeFlags.Unknown, "unknown"),
                Tuple.Create(FillTypeFlags.PerimeterShell, "inner perimeter"),
                Tuple.Create(FillTypeFlags.OutermostShell, "outer perimeter"),
                Tuple.Create(FillTypeFlags.SolidInfill, "solid layer"),
                Tuple.Create(FillTypeFlags.SparseInfill, "infill"),
                Tuple.Create(FillTypeFlags.SupportMaterial, "support"),
                Tuple.Create(FillTypeFlags.BridgeSupport, "bridge"),
                Tuple.Create(FillTypeFlags.Skirt, "skirt"),
                Tuple.Create(FillTypeFlags.OpenShellCurve, "external single extrusion"),
                Tuple.Create(FillTypeFlags.InteriorShell, "interior shell")
            });
        public static FeatureTypeLabeler Value {get => singleton;}
    }
}
