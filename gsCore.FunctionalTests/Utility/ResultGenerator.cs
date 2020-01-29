using System;
using System.IO;
using g3;
using gs;
using gs.interfaces;

namespace gsCore.FunctionalTests.Utility
{
    public class ResultGenerator : IResultGenerator
    {
        private readonly IEngine engine;
        private readonly string testCaseName;
        private readonly ILogger logger;
        public IProfile Settings { get; set; }

        public ResultGenerator(IEngine engine, string testCaseName, ILogger logger)
        {
            this.engine = engine;
            this.testCaseName = testCaseName;
            this.logger = logger;

            Settings = engine.SettingsManager.FactorySettings[0];
            LoadSettingsFromFiles();
        }

        protected void LoadSettingsFromFiles()
        {
            var settingsFiles = TestUtilities.GetTestDataDirectory(testCaseName).GetFiles("*.json");

            foreach (var file in settingsFiles)
            {
                var filePath = Path.GetFullPath(file.FullName);
                try
                {

                    logger.WriteLine($"Loading file {filePath}");
                    engine.SettingsManager.ApplyJSON(Settings, File.ReadAllText(filePath));
                }
                catch (Exception e)
                {
                    logger.WriteLine("Error processing settings file: ");
                    logger.WriteLine(Path.GetFullPath(filePath));
                    logger.WriteLine(e.Message);
                    return;
                }
            }
        }

        protected void SaveGCode(string path, GCodeFile file)
        {
            logger.WriteLine($"Saving file to {path}");
            using var streamWriter = new StreamWriter(path);
            var gCodeWriter = new StandardGCodeWriter();
            gCodeWriter.WriteFile(file, streamWriter);
        }

        public void GenerateResultFile()
        {
            var directory = TestUtilities.GetTestDataDirectory(testCaseName);

            var parts = new[]{
                new Tuple<DMesh3, object>(StandardMeshReader.ReadMesh(
                    TestUtilities.GetMeshFilePath(directory)), 
                    null)
            };

            var gCodeFile = engine.Generator.GenerateGCode(parts, Settings, out var generationReport, null, (s) => logger.WriteLine(s));
            SaveGCode(TestUtilities.GetResultFilePath(directory), gCodeFile); 
        }
    }
}