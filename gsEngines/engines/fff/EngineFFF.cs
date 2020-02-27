using Sutro.PathWorks.Plugins.API;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace gs.engines
{
    [Export(typeof(IEngine))]
    [ExportMetadata("Name", "fff")]
    [ExportMetadata("Description", "Provides access to the basic print generator included in gsCore. Can only create gcode for a single mesh with single material.")]
    public class EngineFFF : Engine<SingleMaterialFFFSettings>
    {
        public override IGenerator<SingleMaterialFFFSettings> Generator => 
            new SinglePartGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>();

        public override ISettingsManager<SingleMaterialFFFSettings> SettingsManager => 
            new SettingsManagerFFF();

        public override List<IVisualizer> Visualizers => new List<IVisualizer>() {
            new VolumetricBeadVisualizer(),
        };

    }
}
