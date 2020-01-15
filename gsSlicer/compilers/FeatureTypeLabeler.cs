using System;
using System.Collections.Generic;
using System.Text;

namespace gs
{
    public class FeatureTypeLabeler
    {
        public FeatureTypeLabeler(IEnumerable<Tuple<FillTypeFlags, string>> featureLabels)
        {
            FlagToFeatureLabelDictionary = new Dictionary<int, string>();
            foreach (var pair in featureLabels)
                FlagToFeatureLabelDictionary[(int)pair.Item1] = pair.Item2;
        }

        public string FeatureLabelFromFillTypeFlag(FillTypeFlags flag)
        {
            var flagInt = (int)flag;
            if (FlagToFeatureLabelDictionary.TryGetValue(flagInt, out string name))
            {
                return name;
            }
            else
            {
                return FlagToFeatureLabelDictionary[(int)FillTypeFlags.Unknown];
            }
        }

        public FillTypeFlags FillTypeFlagFromFeatureLabel(string name)
        {
            foreach (var pair in FlagToFeatureLabelDictionary)
                if (pair.Value.Equals(name))
                    return (FillTypeFlags)pair.Key;
            return FillTypeFlags.Unknown;
        }

        protected Dictionary<int, string> FlagToFeatureLabelDictionary;
    }
}
