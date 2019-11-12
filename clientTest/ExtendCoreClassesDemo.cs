using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using g3;
using gs;
using gs.interfaces;

namespace clientTest
{
    public class SettingsExtended : GenericRepRapSettings
    {
        public double NewSetting { get; set; } = 10;
    }

    public class PrintGeneratorExtended : SingleMaterialFFFPrintGenerator
    {

    }

    public class SettingsManagerExtended : SettingsManager<SettingsExtended>
    {
        public override List<SettingsExtended> FactorySettings { 
            get {
                var result = new List<SettingsExtended>();
                var a = new SettingsExtended();
                var b = new SettingsExtended();

                a.BaseMachine.ManufacturerName = "ExtenderCo";
                b.BaseMachine.ManufacturerName = "ExtenderCo";

                a.BaseMachine.ModelIdentifier = "Generic";
                b.BaseMachine.ManufacturerName = "Plus";

                result.Add(a);
                result.Add(b);
                return result;
            }
        }

        public override IUserSettingCollection<SettingsExtended> UserSettings =>
            new UserSettingsExtended<SettingsExtended>();
    }

    [Export(typeof(IEngine))]
    [ExportMetadata("Name", "ext")]
    [ExportMetadata("Description", "Awesome extended engine.")]
    public class EngineExtended : Engine<SettingsExtended>
    {
        public override IGenerator<SettingsExtended> Generator => 
            new SinglePartGenerator<PrintGeneratorExtended, SettingsExtended>();

        public override ISettingsManager<SettingsExtended> SettingsManager =>             
            new SettingsManagerExtended();

        public override List<IVisualizer> Visualizers => null;
    }
}
