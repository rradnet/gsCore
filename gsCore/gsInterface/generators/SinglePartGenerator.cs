﻿using g3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace gs
{
    public class SinglePartGenerator<TPrintGenerator, TPrintSettings> : IGenerator<TPrintSettings>
            where TPrintGenerator : ThreeAxisPrintGenerator, IPrintGeneratorInitialize, new()
            where TPrintSettings : SingleMaterialFFFSettings
    {
        public bool AcceptsParts { get; } = true;
        public bool AcceptsPartSettings { get; } = false;

        public Version Version { get {
                var assembly = Assembly.GetAssembly(typeof(TPrintGenerator));
                return assembly.GetName().Version;
            } }

        public void SaveGCode(string path, GCodeFile file)
        {
            using (StreamWriter w = new StreamWriter(path))
            {
                StandardGCodeWriter writer = new StandardGCodeWriter();
                writer.WriteFile(file, w);
            }
        }

        public GCodeFile LoadGCode(string path)
        {
            GenericGCodeParser parser = new GenericGCodeParser();
            using (StreamReader fileReader = File.OpenText(path))
                return parser.Parse(fileReader);
        }

        public GCodeFile GenerateGCode(IList<Tuple<DMesh3, TPrintSettings>> parts, TPrintSettings globalSettings, Action<GCodeLine> gcodeLineReadyF = null, Action<PrintLayerData> layerReadyF = null, Action<string> progressMessageF = null)
        {
            if (AcceptsParts == false && parts != null && parts.Count > 0)
                throw new Exception("Must pass null or empty list of parts to generator that does not accept parts.");

            // Create print mesh set
            PrintMeshAssembly meshes = new PrintMeshAssembly();

            foreach (var part in parts)
            {
                if (part.Item2 != null)
                    throw new ArgumentException($"Entries for the `parts` arguments must have a null second item since this generator does not handle per-part settings.");
                meshes.AddMesh(part.Item1, PrintMeshOptions.Default());
            }

            progressMessageF?.Invoke("Slicing...");

            // Do slicing
            MeshPlanarSlicer slicer = new MeshPlanarSlicer()
            {
                LayerHeightMM = globalSettings.LayerHeightMM
            };

            slicer.Add(meshes);
            PlanarSliceStack slices = slicer.Compute();

            // Run the print generator
            progressMessageF?.Invoke("Running print generator...");
            var printGenerator = new TPrintGenerator();
            AssemblerFactoryF overrideAssemblerF = globalSettings.AssemblerType();
            printGenerator.Initialize(meshes, slices, globalSettings, overrideAssemblerF);
            if (printGenerator.Generate())
                return printGenerator.Result;
            else
                throw new Exception("PrintGenerator failed to generate gcode!");
        }

        public GCodeFile GenerateGCode(IList<Tuple<DMesh3, object>> parts, object globalSettings, Action<GCodeLine> gcodeLineReadyF = null, Action<PrintLayerData> layerReadyF = null, Action<string> progressMessageF = null)
        {
            var partsTypedSettings = new List<Tuple<DMesh3, TPrintSettings>>();
            foreach (var part in parts)
            {
                partsTypedSettings.Add(Tuple.Create(part.Item1, (TPrintSettings)(part.Item2)));
            }

            return GenerateGCode(partsTypedSettings, (TPrintSettings)globalSettings, gcodeLineReadyF, layerReadyF, progressMessageF);
        }
    }
}
