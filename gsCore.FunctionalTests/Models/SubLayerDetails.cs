using g3;

namespace gsCore.FunctionalTests.Models
{

    public class SubLayerDetails
    { 
        // a layer is divided into sublayers by fill type
        
        public AxisAlignedBox2d boundingBox;
        public Vector2d unscaledCenterOfMass;
        public double extrusionAmount;
        public double extrusionDistance;
        public double extrusionTime;

        public Vector2d CenterOfMass => unscaledCenterOfMass / extrusionAmount;

        public override string ToString()
        {
            return
                "Bounding Box:\t" + boundingBox +
                "\r\nCenter Of Mass:\t" + CenterOfMass +
                "\r\nExtrusion Amt:\t" + extrusionAmount +
                "\r\nExtrusion Dist:\t" + extrusionDistance +
                "\r\nExtrusion Time:\t" + extrusionTime;
        }
    }
}
