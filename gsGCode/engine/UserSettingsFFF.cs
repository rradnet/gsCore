using System.Globalization;
using gs.interfaces;

namespace gs.engines
{
    public class UserSettingsFFF<TSettings> : UserSettingCollection<TSettings> where TSettings : SingleMaterialFFFSettings {

        #region Advanced

        public static readonly UserSettingGroup GroupAdvanced =
            new UserSettingGroup(() => UserSettingTranslations.GroupAdvanced);

        public UserSettingBool<TSettings> EnableAutoBedLeveling = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.EnableAutoBedLeveling_Name,
            () => UserSettingTranslations.EnableAutoBedLeveling_Description,
            GroupAdvanced,
            (settings) => settings.Machine.EnableAutoBedLeveling,
            (settings, val) => settings.Machine.EnableAutoBedLeveling = val);

        #endregion
        #region Basic

        public static readonly UserSettingGroup GroupBasic =
            new UserSettingGroup(() => UserSettingTranslations.GroupBasic);

        public UserSettingBool<TSettings> EnableBridging = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.EnableBridging_Name,
            () => UserSettingTranslations.EnableBridging_Description,
            GroupBasic,
            (settings) => settings.EnableBridging,
            (settings, val) => settings.EnableBridging = val);

        public UserSettingInt<TSettings> ExtruderTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.ExtruderTempC_Name,
            () => UserSettingTranslations.ExtruderTempC_Description,
            GroupBasic,
            (settings) => settings.ExtruderTempC,
            (settings, val) => settings.ExtruderTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> FilamentDiamMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.FilamentDiamMM_Name,
            () => UserSettingTranslations.FilamentDiamMM_Description,
            GroupBasic,
            (settings) => settings.Machine.FilamentDiamMM,
            (settings, val) => settings.Machine.FilamentDiamMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> FloorLayers = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.FloorLayers_Name,
            () => UserSettingTranslations.FloorLayers_Description,
            GroupBasic,
            (settings) => settings.FloorLayers,
            (settings, val) => settings.FloorLayers = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingBool<TSettings> GenerateSupport = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.GenerateSupport_Name,
            () => UserSettingTranslations.GenerateSupport_Description,
            GroupBasic,
            (settings) => settings.GenerateSupport,
            (settings, val) => settings.GenerateSupport = val);

        public UserSettingInt<TSettings> HeatedBedTempC = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.HeatedBedTempC_Name,
            () => UserSettingTranslations.HeatedBedTempC_Description,
            GroupBasic,
            (settings) => settings.HeatedBedTempC,
            (settings, val) => settings.HeatedBedTempC = val,
            UserSettingNumericValidations<int>.ValidateMin(-273, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> LayerHeightMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.LayerHeightMM_Name,
            () => UserSettingTranslations.LayerHeightMM_Description,
            GroupBasic,
            (settings) => settings.LayerHeightMM,
            (settings, val) => settings.LayerHeightMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> MinExtrudeSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MinExtrudeSpeed_Name,
            () => UserSettingTranslations.MinExtrudeSpeed_Description,
            GroupBasic,
            (settings) => settings.MinExtrudeSpeed,
            (settings, val) => settings.MinExtrudeSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> RoofLayers = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.RoofLayers_Name,
            () => UserSettingTranslations.RoofLayers_Description,
            GroupBasic,
            (settings) => settings.RoofLayers,
            (settings, val) => settings.RoofLayers = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> Shells = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.Shells_Name,
            () => UserSettingTranslations.Shells_Description,
            GroupBasic,
            (settings) => settings.Shells,
            (settings, val) => settings.Shells = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region Bridging

        public static readonly UserSettingGroup GroupBridging =
            new UserSettingGroup(() => UserSettingTranslations.GroupBridging);

        public UserSettingDouble<TSettings> BridgeExtrudeSpeedX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.BridgeExtrudeSpeedX_Name,
            () => UserSettingTranslations.BridgeExtrudeSpeedX_Description,
            GroupBridging,
            (settings) => settings.BridgeExtrudeSpeedX,
            (settings, val) => settings.BridgeExtrudeSpeedX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> BridgeFillNozzleDiamStepX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.BridgeFillNozzleDiamStepX_Name,
            () => UserSettingTranslations.BridgeFillNozzleDiamStepX_Description,
            GroupBridging,
            (settings) => settings.BridgeFillNozzleDiamStepX,
            (settings, val) => settings.BridgeFillNozzleDiamStepX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> BridgeVolumeScale = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.BridgeVolumeScale_Name,
            () => UserSettingTranslations.BridgeVolumeScale_Description,
            GroupBridging,
            (settings) => settings.BridgeVolumeScale,
            (settings, val) => settings.BridgeVolumeScale = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> MaxBridgeWidthMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MaxBridgeWidthMM_Name,
            () => UserSettingTranslations.MaxBridgeWidthMM_Description,
            GroupBridging,
            (settings) => settings.MaxBridgeWidthMM,
            (settings, val) => settings.MaxBridgeWidthMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

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
        #region FirstLayer

        public static readonly UserSettingGroup GroupFirstLayer =
            new UserSettingGroup(() => UserSettingTranslations.GroupFirstLayer);

        public UserSettingDouble<TSettings> CarefulExtrudeSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.CarefulExtrudeSpeed_Name,
            () => UserSettingTranslations.CarefulExtrudeSpeed_Description,
            GroupFirstLayer,
            (settings) => settings.CarefulExtrudeSpeed,
            (settings, val) => settings.CarefulExtrudeSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> StartLayerHeightMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.StartLayerHeightMM_Name,
            () => UserSettingTranslations.StartLayerHeightMM_Description,
            GroupFirstLayer,
            (settings) => settings.StartLayerHeightMM,
            (settings, val) => settings.StartLayerHeightMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> StartLayers = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.StartLayers_Name,
            () => UserSettingTranslations.StartLayers_Description,
            GroupFirstLayer,
            (settings) => settings.StartLayers,
            (settings, val) => settings.StartLayers = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

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
        #region Hidden

        public static readonly UserSettingGroup GroupHidden =
            new UserSettingGroup(() => UserSettingTranslations.GroupHidden);

        public UserSettingString<TSettings> ManufacturerName = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ManufacturerName_Name,
            () => UserSettingTranslations.ManufacturerName_Description,
            GroupHidden,
            (settings) => settings.Machine.ManufacturerName,
            (settings, val) => settings.Machine.ManufacturerName = val);

        public UserSettingString<TSettings> ManufacturerUUID = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ManufacturerUUID_Name,
            () => UserSettingTranslations.ManufacturerUUID_Description,
            GroupHidden,
            (settings) => settings.Machine.ManufacturerUUID,
            (settings, val) => settings.Machine.ManufacturerUUID = val);

        public UserSettingString<TSettings> ModelIdentifier = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ModelIdentifier_Name,
            () => UserSettingTranslations.ModelIdentifier_Description,
            GroupHidden,
            (settings) => settings.Machine.ModelIdentifier,
            (settings, val) => settings.Machine.ModelIdentifier = val);

        public UserSettingString<TSettings> ModelUUID = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ModelUUID_Name,
            () => UserSettingTranslations.ModelUUID_Description,
            GroupHidden,
            (settings) => settings.Machine.ModelUUID,
            (settings, val) => settings.Machine.ModelUUID = val);

        public UserSettingString<TSettings> Class = new UserSettingString<TSettings>(
            () => UserSettingTranslations.Class_Name,
            () => UserSettingTranslations.Class_Description,
            GroupHidden,
            (settings) => settings.Machine.Class.ToString(),
            (settings, val) => settings.Machine.Class = (MachineClass)System.Enum.Parse(typeof(MachineClass), val));

        public UserSettingString<TSettings> ClassTypeName = new UserSettingString<TSettings>(
            () => UserSettingTranslations.ClassTypeName_Name,
            () => UserSettingTranslations.ClassTypeName_Description,
            GroupHidden,
            (settings) => settings.ClassTypeName,
            (settings, val) => throw new System.Exception("ClassTypeName is read-only!"));

        public UserSettingString<TSettings> Identifier = new UserSettingString<TSettings>(
            () => UserSettingTranslations.Identifier_Name,
            () => UserSettingTranslations.Identifier_Description,
            GroupHidden,
            (settings) => settings.Identifier,
            (settings, val) => settings.Identifier = val);

        public UserSettingDouble<TSettings> MinPointSpacingMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MinPointSpacingMM_Name,
            () => UserSettingTranslations.MinPointSpacingMM_Description,
            GroupHidden,
            (settings) => settings.Machine.MinPointSpacingMM,
            (settings, val) => settings.Machine.MinPointSpacingMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region Miscellaneous

        public static readonly UserSettingGroup GroupMiscellaneous =
            new UserSettingGroup(() => UserSettingTranslations.GroupMiscellaneous);

        public UserSettingDouble<TSettings> MinLayerTime = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MinLayerTime_Name,
            () => UserSettingTranslations.MinLayerTime_Description,
            GroupMiscellaneous,
            (settings) => settings.MinLayerTime,
            (settings, val) => settings.MinLayerTime = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region Perimeters

        public static readonly UserSettingGroup GroupPerimeters =
            new UserSettingGroup(() => UserSettingTranslations.GroupPerimeters);

        public UserSettingBool<TSettings> ClipSelfOverlaps = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.ClipSelfOverlaps_Name,
            () => UserSettingTranslations.ClipSelfOverlaps_Description,
            GroupPerimeters,
            (settings) => settings.ClipSelfOverlaps,
            (settings, val) => settings.ClipSelfOverlaps = val);

        public UserSettingInt<TSettings> InteriorSolidRegionShells = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.InteriorSolidRegionShells_Name,
            () => UserSettingTranslations.InteriorSolidRegionShells_Description,
            GroupPerimeters,
            (settings) => settings.InteriorSolidRegionShells,
            (settings, val) => settings.InteriorSolidRegionShells = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingBool<TSettings> OuterShellLast = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.OuterShellLast_Name,
            () => UserSettingTranslations.OuterShellLast_Description,
            GroupPerimeters,
            (settings) => settings.OuterShellLast,
            (settings, val) => settings.OuterShellLast = val);

        public UserSettingDouble<TSettings> SelfOverlapToleranceX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SelfOverlapToleranceX_Name,
            () => UserSettingTranslations.SelfOverlapToleranceX_Description,
            GroupPerimeters,
            (settings) => settings.SelfOverlapToleranceX,
            (settings, val) => settings.SelfOverlapToleranceX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> ShellsFillNozzleDiamStepX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.ShellsFillNozzleDiamStepX_Name,
            () => UserSettingTranslations.ShellsFillNozzleDiamStepX_Description,
            GroupPerimeters,
            (settings) => settings.ShellsFillNozzleDiamStepX,
            (settings, val) => settings.ShellsFillNozzleDiamStepX = val,
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
        #region Retraction

        public static readonly UserSettingGroup GroupRetraction =
            new UserSettingGroup(() => UserSettingTranslations.GroupRetraction);

        public UserSettingDouble<TSettings> MinRetractTravelLength = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.MinRetractTravelLength_Name,
            () => UserSettingTranslations.MinRetractTravelLength_Description,
            GroupRetraction,
            (settings) => settings.MinRetractTravelLength,
            (settings, val) => settings.MinRetractTravelLength = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> RetractDistanceMM = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.RetractDistanceMM_Name,
            () => UserSettingTranslations.RetractDistanceMM_Description,
            GroupRetraction,
            (settings) => settings.RetractDistanceMM,
            (settings, val) => settings.RetractDistanceMM = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> RetractSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.RetractSpeed_Name,
            () => UserSettingTranslations.RetractSpeed_Description,
            GroupRetraction,
            (settings) => settings.RetractSpeed,
            (settings, val) => settings.RetractSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region SolidFill

        public static readonly UserSettingGroup GroupSolidFill =
            new UserSettingGroup(() => UserSettingTranslations.GroupSolidFill);

        public UserSettingDouble<TSettings> SolidFillBorderOverlapX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SolidFillBorderOverlapX_Name,
            () => UserSettingTranslations.SolidFillBorderOverlapX_Description,
            GroupSolidFill,
            (settings) => settings.SolidFillBorderOverlapX,
            (settings, val) => settings.SolidFillBorderOverlapX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SolidFillNozzleDiamStepX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SolidFillNozzleDiamStepX_Name,
            () => UserSettingTranslations.SolidFillNozzleDiamStepX_Description,
            GroupSolidFill,
            (settings) => settings.SolidFillNozzleDiamStepX,
            (settings, val) => settings.SolidFillNozzleDiamStepX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region SparseFill

        public static readonly UserSettingGroup GroupSparseFill =
            new UserSettingGroup(() => UserSettingTranslations.GroupSparseFill);

        public UserSettingDouble<TSettings> SparseFillBorderOverlapX = new UserSettingDouble<TSettings>(
           () => UserSettingTranslations.SparseFillBorderOverlapX_Name,
           () => UserSettingTranslations.SparseFillBorderOverlapX_Description,
           GroupSparseFill,
           (settings) => settings.SparseFillBorderOverlapX,
           (settings, val) => settings.SparseFillBorderOverlapX = val,
           UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SparseLinearInfillStepX = new UserSettingDouble<TSettings>(
           () => UserSettingTranslations.SparseLinearInfillStepX_Name,
           () => UserSettingTranslations.SparseLinearInfillStepX_Description,
           GroupSparseFill,
           (settings) => settings.SparseLinearInfillStepX,
           (settings, val) => settings.SparseLinearInfillStepX = val,
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

        public UserSettingDouble<TSettings> OuterPerimeterSpeedX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.OuterPerimeterSpeedX_Name,
            () => UserSettingTranslations.OuterPerimeterSpeedX_Description,
            GroupSpeeds,
            (settings) => settings.OuterPerimeterSpeedX,
            (settings, val) => settings.OuterPerimeterSpeedX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> RapidExtrudeSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.RapidExtrudeSpeed_Name,
            () => UserSettingTranslations.RapidExtrudeSpeed_Description,
            GroupSpeeds,
            (settings) => settings.RapidExtrudeSpeed,
            (settings, val) => settings.RapidExtrudeSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> RapidTravelSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.RapidTravelSpeed_Name,
            () => UserSettingTranslations.RapidTravelSpeed_Description,
            GroupSpeeds,
            (settings) => settings.RapidTravelSpeed,
            (settings, val) => settings.RapidTravelSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> ZTravelSpeed = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.ZTravelSpeed_Name,
            () => UserSettingTranslations.ZTravelSpeed_Description,
            GroupSpeeds,
            (settings) => settings.ZTravelSpeed,
            (settings, val) => settings.ZTravelSpeed = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        #endregion
        #region Support

        public static readonly UserSettingGroup GroupSupport =
            new UserSettingGroup(() => UserSettingTranslations.GroupSupport);

        public UserSettingBool<TSettings> EnableSupportReleaseOpt = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.EnableSupportReleaseOpt_Name,
            () => UserSettingTranslations.EnableSupportReleaseOpt_Description,
            GroupSupport,
            (settings) => settings.EnableSupportReleaseOpt,
            (settings, val) => settings.EnableSupportReleaseOpt = val);

        public UserSettingBool<TSettings> EnableSupportShell = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.EnableSupportShell_Name,
            () => UserSettingTranslations.EnableSupportShell_Description,
            GroupSupport,
            (settings) => settings.EnableSupportShell,
            (settings, val) => settings.EnableSupportShell = val);

        public UserSettingDouble<TSettings> SupportAreaOffsetX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportAreaOffsetX_Name,
            () => UserSettingTranslations.SupportAreaOffsetX_Description,
            GroupSupport,
            (settings) => settings.SupportAreaOffsetX,
            (settings, val) => settings.SupportAreaOffsetX = val);

        public UserSettingBool<TSettings> SupportMinZTips = new UserSettingBool<TSettings>(
            () => UserSettingTranslations.SupportMinZTips_Name,
            () => UserSettingTranslations.SupportMinZTips_Description,
            GroupSupport,
            (settings) => settings.SupportMinZTips,
            (settings, val) => settings.SupportMinZTips = val);

        public UserSettingDouble<TSettings> SupportOverhangAngleDeg = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportOverhangAngleDeg_Name,
            () => UserSettingTranslations.SupportOverhangAngleDeg_Description,
            GroupSupport,
            (settings) => settings.SupportOverhangAngleDeg,
            (settings, val) => settings.SupportOverhangAngleDeg = val,
            UserSettingNumericValidations<double>.ValidateMinMax(0, 90, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportPointDiam = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportPointDiam_Name,
            () => UserSettingTranslations.SupportPointDiam_Description,
            GroupSupport,
            (settings) => settings.SupportPointDiam,
            (settings, val) => settings.SupportPointDiam = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingInt<TSettings> SupportPointSides = new UserSettingInt<TSettings>(
            () => UserSettingTranslations.SupportPointSides_Name,
            () => UserSettingTranslations.SupportPointSides_Description,
            GroupSupport,
            (settings) => settings.SupportPointSides,
            (settings, val) => settings.SupportPointSides = val,
            UserSettingNumericValidations<int>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportRegionJoinTolX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportRegionJoinTolX_Name,
            () => UserSettingTranslations.SupportRegionJoinTolX_Description,
            GroupSupport,
            (settings) => settings.SupportRegionJoinTolX,
            (settings, val) => settings.SupportRegionJoinTolX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportReleaseGap = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportReleaseGap_Name,
            () => UserSettingTranslations.SupportReleaseGap_Description,
            GroupSupport,
            (settings) => settings.SupportReleaseGap,
            (settings, val) => settings.SupportReleaseGap = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportSolidSpace = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportSolidSpace_Name,
            () => UserSettingTranslations.SupportSolidSpace_Description,
            GroupSupport,
            (settings) => settings.SupportSolidSpace,
            (settings, val) => settings.SupportSolidSpace = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportSpacingStepX = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportSpacingStepX_Name,
            () => UserSettingTranslations.SupportSpacingStepX_Description,
            GroupSupport,
            (settings) => settings.SupportSpacingStepX,
            (settings, val) => settings.SupportSpacingStepX = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

        public UserSettingDouble<TSettings> SupportVolumeScale = new UserSettingDouble<TSettings>(
            () => UserSettingTranslations.SupportVolumeScale_Name,
            () => UserSettingTranslations.SupportVolumeScale_Description,
            GroupSupport,
            (settings) => settings.SupportVolumeScale,
            (settings, val) => settings.SupportVolumeScale = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));

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
