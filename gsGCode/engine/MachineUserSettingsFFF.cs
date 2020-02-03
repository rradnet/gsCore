using System.Globalization;
using gs.interfaces;

namespace gs.engines
{

    public class MachineUserSettingsFFF<TSettings> : UserSettingCollection<TSettings> where TSettings : SingleMaterialFFFSettings
    {
        #region Identifiers

        public static readonly UserSettingGroup GroupIdentifiers =
            new UserSettingGroup(() => UserSettingTranslations.GroupIdentifiers);

        public UserSettingString<TSettings> ManufacturerName = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ManufacturerName_Name,
            () => UserSettingTranslations.ManufacturerName_Description,
            GroupIdentifiers,
            (settings) => settings.Machine.ManufacturerName,
            (settings, val) => settings.Machine.ManufacturerName = val);

        public UserSettingString<TSettings> ModelIdentifier = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ModelIdentifier_Name,
            () => UserSettingTranslations.ModelIdentifier_Description,
            GroupIdentifiers,
            (settings) => settings.Machine.ModelIdentifier,
            (settings, val) => settings.Machine.ModelIdentifier = val);
        
        #endregion

        #region Extruder

        public static readonly UserSettingGroup GroupExtruder =
            new UserSettingGroup(() => UserSettingTranslations.GroupExtruder);

        public UserSettingInt<TSettings> MaxExtruderTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxExtruderTempC_Name,
            () => UserSettingTranslations.MaxExtruderTempC_Description,
            GroupExtruder,
            (settings) => settings.Machine.MaxExtruderTempC,
            (settings, val) => settings.Machine.MaxExtruderTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> MinExtruderTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MinExtruderTempC_Name,
            () => UserSettingTranslations.MinExtruderTempC_Description,
            GroupExtruder,
            (settings) => settings.Machine.MinExtruderTempC,
            (settings, val) => settings.Machine.MinExtruderTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> NozzleDiamMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.NozzleDiamMM_Name,
            () => UserSettingTranslations.NozzleDiamMM_Description,
            GroupExtruder,
            (settings) => settings.Machine.NozzleDiamMM,
            (settings, val) => settings.Machine.NozzleDiamMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion

        #region PrintVolume

        public static readonly UserSettingGroup GroupPrintVolume =
            new UserSettingGroup(() => UserSettingTranslations.GroupPrintVolume);

        public UserSettingDouble<TSettings> BedSizeXMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.BedSizeXMM_Name,
            () => UserSettingTranslations.BedSizeXMM_Description,
            GroupPrintVolume,
            (settings) => settings.Machine.BedSizeXMM,
            (settings, val) => settings.Machine.BedSizeXMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> BedSizeYMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.BedSizeYMM_Name,
            () => UserSettingTranslations.BedSizeYMM_Description,
            GroupPrintVolume,
            (settings) => settings.Machine.BedSizeYMM,
            (settings, val) => settings.Machine.BedSizeYMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> MaxHeightMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MaxHeightMM_Name,
            () => UserSettingTranslations.MaxHeightMM_Description,
            GroupPrintVolume,
            (settings) => settings.Machine.MaxHeightMM,
            (settings, val) => settings.Machine.MaxHeightMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        
        #region Speeds

        public static readonly UserSettingGroup GroupSpeeds =
            new UserSettingGroup(() => UserSettingTranslations.GroupSpeeds);

        public UserSettingInt<TSettings> MaxExtrudeSpeedMMM = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxExtrudeSpeedMMM_Name,
            () => UserSettingTranslations.MaxExtrudeSpeedMMM_Description,
            GroupSpeeds,
            (settings) => settings.Machine.MaxExtrudeSpeedMMM,
            (settings, val) => settings.Machine.MaxExtrudeSpeedMMM = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> MaxRetractSpeedMMM = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxRetractSpeedMMM_Name,
            () => UserSettingTranslations.MaxRetractSpeedMMM_Description,
            GroupSpeeds,
            (settings) => settings.Machine.MaxRetractSpeedMMM,
            (settings, val) => settings.Machine.MaxRetractSpeedMMM = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> MaxTravelSpeedMMM = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxTravelSpeedMMM_Name,
            () => UserSettingTranslations.MaxTravelSpeedMMM_Description,
            GroupSpeeds,
            (settings) => settings.Machine.MaxTravelSpeedMMM,
            (settings, val) => settings.Machine.MaxTravelSpeedMMM = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> MaxZTravelSpeedMMM = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxZTravelSpeedMMM_Name,
            () => UserSettingTranslations.MaxZTravelSpeedMMM_Description,
            GroupSpeeds,
            (settings) => settings.Machine.MaxZTravelSpeedMMM,
            (settings, val) => settings.Machine.MaxZTravelSpeedMMM = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        
        #region Capabilities

        public static readonly UserSettingGroup GroupCapabilities =
            new UserSettingGroup(() => UserSettingTranslations.GroupCapabilities);

        public UserSettingBool<TSettings> HasAutoBedLeveling = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.HasAutoBedLeveling_Name,
            () => UserSettingTranslations.HasAutoBedLeveling_Description,
            GroupCapabilities,
            (settings) => settings.Machine.HasAutoBedLeveling,
            (settings, val) => settings.Machine.HasAutoBedLeveling = val);

        public UserSettingBool<TSettings> HasHeatedBed = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.HasHeatedBed_Name,
            () => UserSettingTranslations.HasHeatedBed_Description,
            GroupCapabilities,
            (settings) => settings.Machine.HasHeatedBed,
            (settings, val) => settings.Machine.HasHeatedBed = val);

        public UserSettingDouble<TSettings> MaxLayerHeightMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MaxLayerHeightMM_Name,
            () => UserSettingTranslations.MaxLayerHeightMM_Description,
            GroupCapabilities,
            (settings) => settings.Machine.MaxLayerHeightMM,
            (settings, val) => settings.Machine.MaxLayerHeightMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> MinLayerHeightMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MinLayerHeightMM_Name,
            () => UserSettingTranslations.MinLayerHeightMM_Description,
            GroupCapabilities,
            (settings) => settings.Machine.MinLayerHeightMM,
            (settings, val) => settings.Machine.MinLayerHeightMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion

        #region HeatedBed

        public static readonly UserSettingGroup GroupHeatedBed =
            new UserSettingGroup(() => UserSettingTranslations.GroupHeatedBed);

        public UserSettingInt<TSettings> MaxBedTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MaxBedTempC_Name,
            () => UserSettingTranslations.MaxBedTempC_Description,
            GroupHeatedBed,
            (settings) => settings.Machine.MaxBedTempC,
            (settings, val) => settings.Machine.MaxBedTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> MinBedTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.MinBedTempC_Name,
            () => UserSettingTranslations.MinBedTempC_Description,
            GroupHeatedBed,
            (settings) => settings.Machine.MinBedTempC,
            (settings, val) => settings.Machine.MinBedTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        #endregion

        /// <summary>
        /// Sets the culture for name & description strings.
        /// </summary>
        /// <remarks>
        /// Any translation resources used in derived classes should override to set the culture 
        /// on the resource manager, while still calling the base method.
        /// </remarks>
        /// <param name="cultureInfo"></param>
        public override void SetCulture(CultureInfo cultureInfo) {
            UserSettingTranslations.Culture = cultureInfo;
        }
    }
}
