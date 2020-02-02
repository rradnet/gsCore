using g3;

namespace gsCore.FunctionalTests.Models
{
    public class FeatureInfo
    {
        public AxisAlignedBox2d BoundingBox { get; set; }
        public Vector2d UnscaledCenterOfMass { get; set; }
        public double ExtrusionAmount { get; set; }
        public double ExtrusionDistance { get; set; }
        public double ExtrusionTime { get; set; }

        public Vector2d CenterOfMass => UnscaledCenterOfMass / ExtrusionAmount;

        public override string ToString()
        {
            return
                "Bounding Box:\t" + BoundingBox +
                "\r\nCenter Of Mass:\t" + CenterOfMass +
                "\r\nExtrusion Amt:\t" + ExtrusionAmount +
                "\r\nExtrusion Dist:\t" + ExtrusionDistance +
                "\r\nExtrusion Time:\t" + ExtrusionTime;
        }
    }
}