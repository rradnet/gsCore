using gs;

namespace gsCore.FunctionalTests.Models
{ 
    public interface IFeatureInfo
    {
        FillTypeFlags FillType { get; set; }

        void Add(IFeatureInfo other);

        void AssertEqualsExpected(IFeatureInfo other);
    }
}