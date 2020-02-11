using System.Collections.Generic;

namespace gs.interfaces
{
    public interface ISettingsManager
    {
        List<IProfile> FactorySettings { get; }

        IProfile FactorySettingByManufacturerAndModel(string manufacturer, string model);

        void ApplyJSON(IProfile settings, string json);
        void ApplyKeyValuePair(IProfile settings, string keyValue);

        IUserSettingCollection MachineUserSettings { get; }
        IUserSettingCollection MaterialUserSettings { get; }
        IUserSettingCollection PrintUserSettings { get; }
    }


    public interface ISettingsManager<TSettings> : ISettingsManager where TSettings : IProfile
    {
        new List<TSettings> FactorySettings { get; }

        new TSettings FactorySettingByManufacturerAndModel(string manufacturer, string model);


        void ApplyJSON(TSettings settings, string json);
        void ApplyKeyValuePair(TSettings settings, string keyValue);

        new IUserSettingCollection<TSettings> MachineUserSettings { get; }

        new IUserSettingCollection<TSettings> MaterialUserSettings { get; }

        new IUserSettingCollection<TSettings> PrintUserSettings { get; }
    }
}