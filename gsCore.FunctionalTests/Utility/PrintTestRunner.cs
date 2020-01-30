using System.IO;

namespace gsCore.FunctionalTests.Utility
{
    public class PrintTestRunner
    {
        protected readonly IResultGenerator resultGenerator;
        protected readonly IResultComparer resultComparer;

        protected DirectoryInfo directory;

        public PrintTestRunner(string name, IResultGenerator resultGenerator, IResultComparer resultComparer)
        {
            this.resultGenerator = resultGenerator;
            this.resultComparer = resultComparer;

            directory = Paths.GetTestDataDirectory(name);
        }

        public void CompareResults()
        {
            resultComparer.CompareFiles(
                Paths.GetExpectedFilePath(directory), 
                Paths.GetResultFilePath(directory));
        }

        public void GenerateFile()
        {
            resultGenerator.GenerateResultFile(
                Paths.GetMeshFilePath(directory), 
                Paths.GetResultFilePath(directory));
        }
    }
}
