using System;
using gs.interfaces;


namespace gs
{
    public class FixedRangeCustomDataDetails : IVisualizerCustomDataDetails
    {
        private readonly Func<string> labelF;
        public string Label { get => labelF(); }

        public float RangeMin { get; private set; }
        public float RangeMax { get; private set; }

        public FixedRangeCustomDataDetails(float rangeMin, float rangeMax, Func<string> labelF)
        {
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            this.labelF = labelF;
        }
    }
}
