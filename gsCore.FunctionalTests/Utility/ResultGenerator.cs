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
        private readonly ILogger logger;

        public IProfile Settings { get; set; }

        public ResultGenerator(IEngine engine, ILogger logger)
        {
            this.engine = engine;
            this.logger = logger;

            Settings = engine.SettingsManager.FactorySettings[0];
        }

        protected void SaveGCode(string path, GCodeFile file)
        {
            logger.WriteLine($"Saving file to {path}");
            using var streamWriter = new StreamWriter(path);
            var gCodeWriter = new StandardGCodeWriter();
            gCodeWriter.WriteFile(file, streamWriter);
        }

        public void GenerateResultFile(string meshFilePath, string outputFilePath)
        {
            var parts = new[]{
                new Tuple<DMesh3, object>(StandardMeshReader.ReadMesh(meshFilePath), null)
            };

            var gCodeFile = engine.Generator.GenerateGCode(parts, Settings, out _, null, (s) => logger.WriteLine(s));
            SaveGCode(outputFilePath, gCodeFile);
        }
    }
}