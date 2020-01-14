using System;
using gs.interfaces;


namespace gs
{
    public abstract class CustomDataDetails : IVisualizerCustomDataDetails
    {
        public virtual float RangeMin { get; protected set; }
        public virtual float RangeMax { get; protected set; }

        private readonly Func<string> labelF;
        public string Label { get => labelF(); }

        private Func<float, string> colorScaleLabelerF;
        public string FormatColorScaleLabel(float value)
        {
            return colorScaleLabelerF(value);
        }

        public CustomDataDetails(Func<string> labelF, Func<float, string> colorScaleLabelerF)
        {
            this.labelF = labelF;
            this.colorScaleLabelerF = colorScaleLabelerF;
        }
    }
}
