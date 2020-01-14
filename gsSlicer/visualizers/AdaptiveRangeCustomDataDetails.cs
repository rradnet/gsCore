using g3;
using System;
using gs.interfaces;


namespace gs
{
    public class AdaptiveRangeCustomDataDetails : IVisualizerCustomDataDetails
    {
        private readonly Func<string> labelF;
        public string Label { get => labelF(); }

        private Interval1d interval = Interval1d.Empty;
        public float RangeMin { get => (float)interval.a; }
        public float RangeMax { get => (float)interval.b; }

        public AdaptiveRangeCustomDataDetails(Func<string> labelF)
        {
            this.labelF = labelF;
        }

        public void ObserveValue(float value)
        {
            interval.Contain(value);
        }
    }
}
