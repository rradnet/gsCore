using System;

namespace gs.interfaces
{
    public class UserSettingInt<TSettings> : UserSetting<TSettings, int>
    {
        public UserSettingInt(
            Func<string> nameF,
            Func<string> descriptionF,
            UserSettingGroup group,
            Func<TSettings, int> loadF,
            Action<TSettings, int> applyF,
            Func<int, ValidationResult> validateF = null) : base(nameF, descriptionF, group, loadF, applyF, validateF)
        {

        }
    }

    public static class UserSettingNumericValidations<T> where T : IComparable<T>
    {
        public static Func<T, ValidationResult> ValidateMin(T min, ValidationResult.Level level)
        {
            return (val) =>
            {
                if (val.CompareTo(min) < 0)
                    return new ValidationResult(level, string.Format("Must be at least {0}", min));
                return new ValidationResult();
            };
        }

        public static Func<T, ValidationResult> ValidateMax(T max, ValidationResult.Level level)
        {
            return (val) =>
            {
                if (val.CompareTo(max) > 0)
                    return new ValidationResult(level, string.Format("Must be no more than {0}", max));
                return new ValidationResult();
            };
        }

        public static Func<T, ValidationResult> ValidateMinMax(T min, T max, ValidationResult.Level level)
        {
            return (val) =>
            {
                var result = ValidateMin(min, level).Invoke(val);
                if (result.Severity == ValidationResult.Level.None)
                    result = ValidateMax(max, level).Invoke(val);
                return result;
            };
        }
    }
}
