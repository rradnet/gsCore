using System.Collections.Generic;

namespace gs.interfaces
{
    public interface IEngineData
    {
        string Name { get; }
        string Description { get; }
    }

    public interface IEngine
    {
        IGenerator Generator { get; }
        ISettingsManager SettingsManager { get; }
        List<IVisualizer> Visualizers { get; }
    }

    public interface IEngine<TSettings> : IEngine where TSettings : IProfile
    {
        new IGenerator<TSettings> Generator { get; }
        new ISettingsManager<TSettings> SettingsManager { get; }
    }

    public abstract class Engine<TSettings> : IEngine<TSettings> where TSettings : IProfile
    {
        public abstract IGenerator<TSettings> Generator { get; }
        public abstract ISettingsManager<TSettings> SettingsManager { get; }
        public abstract List<IVisualizer> Visualizers { get; }

        IGenerator IEngine.Generator => Generator;
        ISettingsManager IEngine.SettingsManager => SettingsManager;
    }
}
