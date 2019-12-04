using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using g3;
using gs.interfaces;
using gs.engines;

namespace sutro.CLI
{
    class Program
    {
        #pragma warning disable CS0649
        [ImportMany(typeof(IEngine))]
        protected IEnumerable<Lazy<IEngine, IEngineData>> Engines;
        #pragma warning restore CS0649

        protected static Dictionary<string, Lazy<IEngine, IEngineData>> EngineDictionary;

        private CompositionContainer _container;

        public Program()
        {
            // An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            // Add catalogs in overrideable method
            AddCatalogs(catalog);

            // Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            // Fill the imports of this object
            try
            {
                _container.ComposeParts(this);
            }
            catch (ReflectionTypeLoadException e)
            {
                Console.WriteLine("Composition loader exceptions:");
                foreach (var a in e.LoaderExceptions)
                {
                    Console.WriteLine(a.ToString());
                }
            }
            catch (CompositionException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

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

        protected virtual void AddCatalogs(AggregateCatalog catalog)
        {
            ReferenceEngines();

            // This iteration is required because of the bundling done by Fody.Costura
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                catalog.Catalogs.Add(new AssemblyCatalog(asm));
            }
            var pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            if (Directory.Exists(pluginDirectory))
                catalog.Catalogs.Add(new DirectoryCatalog(pluginDirectory));
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
        
        [STAThread]
        static void Main(string[] args)
        {
            // Construct a dictionary of all the engines that were imported via MEF
            var p = new Program();

            EngineDictionary = new Dictionary<string, Lazy<IEngine, IEngineData>>();

            if (p.Engines == null)
                return;

            foreach (var e in p.Engines)
            {
                if (!EngineDictionary.ContainsKey(e.Metadata.Name))
                    EngineDictionary.Add(e.Metadata.Name.ToLower(), e);
            }

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
            var validations = engine.SettingsManager.UserSettings.Validate(settings);
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
            engine.Generator.SaveGCode(fGCodeFilePath, gcode);

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
