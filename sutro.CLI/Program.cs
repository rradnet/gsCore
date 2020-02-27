using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using g3;
using gs.engines;
using Sutro.PathWorks.Plugins.API;

namespace sutro.CLI
{
    class Program
    {
        /// <remarks>
        /// Need to explicitly reference any class from each assembly with engines in it;
        /// this is due to the fact that Fody.Costura will discard unreferenced assemblies
        /// which breaks MEF discovery. This is a bit of a hack; hopefully able to come
        /// up with something more elegant in the future.
        /// </remarks>
        protected virtual void ReferenceEngines()
        {
            _ = new EngineFFF();
        }

        public class Options
        {
            [Value(0, MetaName = "engine", Required = true, HelpText = "Select which toolpathing engine to use.")]
            public string Engine { get; set; }

            [Value(1, MetaName = "gcode", Required = true, HelpText = "Path to output gcode file.")]
            public string GCodeFilePath { get; set; }

            [Value(2, MetaName = "mesh", Required = false, HelpText = "Path to input mesh file.")]
            public string MeshFilePath { get; set; }

            [Option('c', "center_xy", Required = false, Default = false, HelpText = "Center the part on the print bed in XY.")]
            public bool CenterXY { get; set; }

            [Option('z', "drop_z", Required = false, Default = false, HelpText = "Drop the part to the print bed in Z.")]
            public bool DropZ { get; set; }

            [Option('s', "settings_files", Required=false, HelpText = "Settings file(s).")]
            public IEnumerable<string> SettingsFiles { get; set; }

            [Option('o', "settings_override", Required = false, HelpText = "Override individual settings")]
            public IEnumerable<string> SettingsOverride { get; set; }

            [Option('m', "machine_manufacturer", Default ="RepRap", Required = false, HelpText = "Machine manufacturer.")]
            public string MachineManufacturer { get; set; }

            [Option('d', "machine_model", Default = "Generic", Required = false, HelpText = "Machine model.")]
            public string MachineModel { get; set; }

            [Option('f', "force_invalid_settings", Default = false, Required = false, 
                HelpText = "Unless true, settings will be validated against UserSettings for the settings type; the generator will not run with invalid settings. If true, invalid settings will still be used.")]
            public bool ForceInvalidSettings { get; set; }
        }


        private static Dictionary<string, Lazy<IEngine, IEngineData>> EngineDictionary;

        [STAThread]
        static void Main(string[] args)
        {
            // Construct a dictionary of all the engines that were imported via MEF
            var pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            var a = new EngineFinder(pluginDirectory, (s) => Console.WriteLine(s));
            EngineDictionary = a.EngineDictionary;

            // Parse the input arguments
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);

            parserResult.WithParsed(ParsingSuccessful);
            parserResult.WithNotParsed((err) => ParsingUnsuccessful(err, parserResult));
            return;
        }

        private static IEnumerable<string> ListEngines()
        {
            var result = new List<string>();

            foreach (var engine in EngineDictionary.Values)
            {
                result.Add(engine.Metadata.Name + " : " + engine.Metadata.Description);
                result.Add("");
            }
            return result;
        }

        protected static void ParsingSuccessful(Options o)
        {
            if (!EngineDictionary.TryGetValue(o.Engine, out var engineEntry))
            {
                Console.WriteLine("Invalid engine specified.");
                Console.WriteLine("");
                Console.WriteLine("Available engines:");
                Console.WriteLine("");
                foreach (string s in ListEngines())
                    Console.WriteLine(s);
                return;
            }
            var engine = engineEntry.Value;

            ConsoleWriteSeparator();
            Version cliVersion = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine("gsCore.CLI " + VersionToString(cliVersion));
            Console.WriteLine();

            Console.WriteLine($"Using engine {engine.GetType()} {VersionToString(engine.Generator.Version)}");

            if (engine.Generator.AcceptsParts && (o.MeshFilePath is null || !File.Exists(o.MeshFilePath)))
            {
                Console.WriteLine("Must provide valid mesh file path as second argument.");
                Console.WriteLine(Path.GetFullPath(o.MeshFilePath));
                return;
            }

            else if (o.GCodeFilePath is null || !Directory.Exists(Directory.GetParent(o.GCodeFilePath).ToString()))
            {
                Console.WriteLine("Must provide valid gcode file path as second argument.");
                return;
            }

            foreach (string s in o.SettingsFiles)
            {
                if (!File.Exists(s))
                {
                    Console.WriteLine("Must provide valid settings file path.");
                    return;
                }
            }

            ConsoleWriteSeparator();
            Console.WriteLine($"SETTINGS");
            Console.WriteLine();
            IProfile settings;
            try
            {
                settings = engine.SettingsManager.FactorySettingByManufacturerAndModel(o.MachineManufacturer, o.MachineModel);
                Console.WriteLine($"Starting with factory profile {settings.ManufacturerName} {settings.ModelIdentifier}");
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.Message);

                settings = engine.SettingsManager.FactorySettings[0];
                Console.WriteLine($"Falling back to first factory profile: {settings.ManufacturerName} {settings.ModelIdentifier}");
            }
            
            // Load settings from files
            foreach (string s in o.SettingsFiles)
            {
                try
                {
                    Console.WriteLine($"Loading file {Path.GetFullPath(s)}");
                    string settingsText = File.ReadAllText(s);
                    engine.SettingsManager.ApplyJSON(settings, settingsText);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error processing settings file: ");
                    Console.WriteLine(Path.GetFullPath(s));
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            // Override settings from command-line arguments
            foreach (string s in o.SettingsOverride)
            {
                try
                {
                    engine.SettingsManager.ApplyKeyValuePair(settings, s);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error processing settings override from command line argument: ");
                    Console.WriteLine(s);
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            // Perform setting validations
            Console.WriteLine("Validating settings...");
            var validations = engine.SettingsManager.PrintUserSettings.Validate(settings);
            int errorCount = 0;
            foreach (var v in validations)
            {
                if (v.Severity == ValidationResult.Level.Warning)
                {
                    Console.WriteLine($"\tWarning - {v.SettingName}: {v.Message}");
                }
                else if (v.Severity == ValidationResult.Level.Error)
                {
                    Console.WriteLine($"\tError - {v.SettingName}: {v.Message}");
                    errorCount++;
                }
            }

            if (errorCount > 0)
            {
                if (o.ForceInvalidSettings)
                {
                    Console.WriteLine("Invalid settings found; proceeding anyway since -f flag is enabled.");
                }
                else
                {
                    Console.WriteLine("Invalid settings found; canceling generation. To override validation, use the -f flag.");
                    return;
                }
            }

            var parts = new List<Tuple<DMesh3, object>>();

            if (engine.Generator.AcceptsParts)
            {
                string fMeshFilePath = Path.GetFullPath(o.MeshFilePath);
                ConsoleWriteSeparator();
                Console.WriteLine($"PARTS");
                Console.WriteLine();

                Console.Write("Loading mesh " + fMeshFilePath + "...");
                DMesh3 mesh = StandardMeshReader.ReadMesh(fMeshFilePath);
                Console.WriteLine(" done.");

                // Center mesh above origin.
                AxisAlignedBox3d bounds = mesh.CachedBounds;
                if (o.CenterXY)
                    MeshTransforms.Translate(mesh, new Vector3d(-bounds.Center.x, -bounds.Center.y, 0));

                // Drop mesh to bed.
                if (o.DropZ)
                    MeshTransforms.Translate(mesh, new Vector3d(0, 0, bounds.Extents.z - bounds.Center.z));

                var part = new Tuple<DMesh3, object>(mesh, null);
                parts.Add(part);
            };
            string fGCodeFilePath = Path.GetFullPath(o.GCodeFilePath);

            ConsoleWriteSeparator();
            Console.WriteLine($"GENERATION");
            Console.WriteLine();

            var gcode = engine.Generator.GenerateGCode(parts, settings, out var generationReport, 
                null, (s) => Console.WriteLine(s));

            Console.WriteLine($"Writing gcode to {fGCodeFilePath}");
            using (StreamWriter w = new StreamWriter(fGCodeFilePath))
            {
                engine.Generator.SaveGCode(w, gcode);
            }

            ConsoleWriteSeparator();
            foreach (var s in generationReport)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine();
            Console.WriteLine("Print generation complete.");
        }

        protected static void ParsingUnsuccessful(IEnumerable<Error> errs, ParserResult<Options> parserResult)
        {
            Console.WriteLine("ERRORS:");
            foreach (var err in errs)
                Console.WriteLine(err);
            Console.WriteLine("");

            Console.WriteLine("HELP:");
            var helpText = HelpText.AutoBuild(parserResult, h => { return h; }, e => e);
            Console.WriteLine(helpText.ToString());
            Console.WriteLine("");

            Console.WriteLine("ENGINES:");
            Console.WriteLine("");
            foreach (string s in ListEngines())
                Console.WriteLine(s);
        }

        protected static void ConsoleWriteSeparator()
        {
            Console.WriteLine("".PadRight(79, '-'));
        }

        protected static string VersionToString(Version v)
        {
            return $"v{v.Major}.{v.Minor}.{v.Revision}";
        }
    }
}
