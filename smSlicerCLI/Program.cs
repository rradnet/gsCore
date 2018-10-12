using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using CommandLine;

using g3;
using gs;
using gs.info;

namespace smSlicerCLI
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName="mesh", HelpText="Path to input mesh file.")]
            public string MeshFilePath { get; set; }

            [Value(1, MetaName = "gcode", HelpText = "Path to output gcode file.")]
            public string GCodeFilePath { get; set; }

            [Option('s', "settings_files", Required=false, HelpText = "Settings file(s).")]
            public IEnumerable<string> SettingsFiles { get; set; }

            [Option('o', "settings_override", Required = false, HelpText = "Override individual settings")]
            public IEnumerable<string> SettingsOverride{ get; set; }
        }

        [STAThread]
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                if (o.MeshFilePath is null || !File.Exists(o.MeshFilePath))
                {
                    Console.WriteLine("Must provide valid mesh file path as first argument.");
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

                // create settings
                RepRapSettings settings = new RepRapSettings(RepRap.Models.Unknown);

                JsonSerializerSettings jsonSerializeSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,

                };

                // load settings from files
                foreach (string s in o.SettingsFiles)
                {
                    try
                    {
                        string settingsText = File.ReadAllText(s);
                        // TODO: Make this more strict to avoid converting values unintentionally
                        JsonConvert.PopulateObject(settingsText, settings, jsonSerializeSettings);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error processing settings file: ");
                        Console.WriteLine(Path.GetFullPath(s));
                        Console.WriteLine(e.Message);
                        return;
                    }
                }

                // override settings from command-line arguments
                foreach (string s in o.SettingsOverride)
                {
                    try
                    {
                        // TODO: Make this more strict to avoid converting values unintentionally

                        string[] keyValue = s.Split(':');
                        if (keyValue.Length != 2)
                            throw new Exception("Need setting in \"KeyName:Value\" format; got " + s);
                        string sFormatted = "{\"" + keyValue[0] + "\":" + keyValue[1] + "}";

                        JsonConvert.PopulateObject(sFormatted, settings, jsonSerializeSettings);
                    }
                    catch (Exception)
                    {
                        // TODO: Make error message more useful regarding format
                        Console.WriteLine("Error processing settings override from command line argument: ");
                        Console.WriteLine(s);
                        return;
                    }
                }

                string fMeshFilePath = Path.GetFullPath(o.MeshFilePath);
                string fGCodeFilePath = Path.GetFullPath(o.GCodeFilePath);

                Console.Write("Loading mesh " + fMeshFilePath + "...");
                DMesh3 mesh = StandardMeshReader.ReadMesh(fMeshFilePath);
                Console.WriteLine(" loaded.");


                // center mesh above origin
                AxisAlignedBox3d bounds = mesh.CachedBounds;
                Vector3d baseCenterPt = bounds.Center - bounds.Extents.z * Vector3d.AxisZ;
                MeshTransforms.Translate(mesh, -baseCenterPt);

                // create print mesh set
                PrintMeshAssembly meshes = new PrintMeshAssembly();
                meshes.AddMesh(mesh, PrintMeshOptions.Default());

                Console.Write("Slicing mesh...");

                // do slicing
                MeshPlanarSlicer slicer = new MeshPlanarSlicer()
                {
                    LayerHeightMM = settings.LayerHeightMM
                };
                slicer.Add(meshes);
                PlanarSliceStack slices = slicer.Compute();
                Console.WriteLine(" sliced.");


                Console.Write("Generating print...");
                // run print generator
                SingleMaterialFFFPrintGenerator printGen =
                    new SingleMaterialFFFPrintGenerator(meshes, slices, settings);
                if (printGen.Generate())
                {
                    Console.WriteLine(" generated.");
                    // export gcode

                    Console.Write("Writing gcode...");

                    GCodeFile gcode = printGen.Result;
                    using (StreamWriter w = new StreamWriter(fGCodeFilePath))
                    {
                        StandardGCodeWriter writer = new StandardGCodeWriter();
                        writer.WriteFile(gcode, w);
                    }
                    Console.WriteLine(" done.");

                    Console.WriteLine("".PadRight(79, '-'));
                    foreach (string line in printGen.TotalExtrusionReport)
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine("".PadRight(79, '-'));
                    foreach (string line in printGen.TotalPrintTimeStatistics)
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine("".PadRight(79, '-'));
                }
            });
            return;
        }
    }
}
