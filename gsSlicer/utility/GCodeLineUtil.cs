namespace gs.utility
{
    public static class GCodeLineUtil
    {
        public static bool ExtractFillType(GCodeLine line, ref FillTypeFlags fillType)
        {
            if (line.comment != null)
            {
                int indexOfFillType = line.comment.IndexOf("feature");
                if (indexOfFillType >= 0)
                {
                    var featureName = line.comment.Substring(indexOfFillType + 8).Trim();
                    fillType = FeatureTypeLabelerFFF.Value.FillTypeFlagFromFeatureLabel(featureName);
                    return true;
                }
            }
            return false;
        }
    }
}
