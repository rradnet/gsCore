namespace gsCore.FunctionalTests.Utility
{
    public interface IResultComparer
    {
        void CompareFiles(string gcodeFilePathA, string gcodeFilePathB);
    }
}