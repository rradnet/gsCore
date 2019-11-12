using System.Collections.Generic;
using gs;
using gs.interfaces;

namespace clientTest
{
    public class UserSettingsExtended<TSettings> : UserSettingsFFF<TSettings> where TSettings : SettingsExtended
    {
        public static UserSettingGroup GroupExtended = new UserSettingGroup(() => UserSettingsExtendedTranslations.GroupExtended);

        public UserSetting<SettingsExtended, double> NewSetting = new UserSetting<SettingsExtended, double>(
            () => UserSettingsExtendedTranslations.NewSetting_Name,
            () => UserSettingsExtendedTranslations.NewSetting_Description,
            GroupExtended,
            (settings) => settings.NewSetting,
            (settings, val) => settings.NewSetting = val,
            UserSettingNumericValidations<double>.ValidateMin(0, ValidationResult.Level.Error));
    }
}
