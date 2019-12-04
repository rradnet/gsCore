using System;

namespace gs.interfaces
{
    public class UserSettingString<TSettings> : UserSetting<TSettings, string>
    {
        public UserSettingString(
            Func<string> nameF,
            Func<string> descriptionF,
            UserSettingGroup group,
            Func<TSettings, string> loadF,
            Action<TSettings, string> applyF,
            Func<string, ValidationResult> validateF = null) : base(nameF, descriptionF, group, loadF, applyF, validateF)
        {
        }
    }
}
