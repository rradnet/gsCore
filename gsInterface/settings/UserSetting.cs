using System;

namespace gs.interfaces
{
    public abstract class UserSetting
    {
        private readonly Func<string> NameF;
        private readonly Func<string> DescriptionF;

        public string Name => NameF();
        public string Description => DescriptionF();

        public readonly UserSettingGroup Group;

        public abstract void LoadFromRaw(object settings);
        public abstract void ApplyToRaw(object settings);
        public abstract object GetFromRaw(object settings);
        public abstract void SetToRaw(object settings, object value);

        public abstract ValidationResult Validation { get; }
        public UserSetting(Func<string> nameF, Func<string> descriptionF = null, UserSettingGroup group = null)
        {
            NameF = nameF;
            DescriptionF = descriptionF;
            Group = group;
        }
    }

    public abstract class UserSetting<TSettings> : UserSetting
    {
        public UserSetting(Func<string> nameF,
                           Func<string> descriptionF = null,
                           UserSettingGroup group = null) : base(nameF, descriptionF, group)
        {
        }

        public override void LoadFromRaw(object settings)
        {
            LoadFromRaw((TSettings)settings);
        }
        public override void ApplyToRaw(object settings)
        {
            ApplyToRaw((TSettings)settings);
        }
        public override object GetFromRaw(object settings) {
            return GetFromRaw((TSettings)settings);
        }
        public override void SetToRaw(object settings, object value)
        {
            SetToRaw((TSettings) settings, value);
        }

        public abstract void LoadFromRaw(TSettings settings);
        public abstract void ApplyToRaw(TSettings settings);
        public abstract object GetFromRaw(TSettings settings);
        public abstract void SetToRaw(TSettings settings, object value);
    }

    public class UserSetting<TSettings, TValue> : UserSetting<TSettings>
    {
        public TValue Value { get; set; }

        private readonly Func<TValue, ValidationResult> validateF;
        private readonly Func<TSettings, TValue> loadF;
        private readonly Action<TSettings, TValue> applyF;

        public UserSetting(
            Func<string> nameF,
            Func<string> descriptionF,
            UserSettingGroup group,
            Func<TSettings, TValue> loadF,
            Action<TSettings, TValue> applyF,
            Func<TValue, ValidationResult> validateF = null) : base(nameF, descriptionF, group)
        {
            this.validateF = validateF;
            this.applyF = applyF;
            this.loadF = loadF;
        }

        public override void LoadFromRaw(TSettings settings) { Value = loadF(settings); }
        public override void ApplyToRaw(TSettings settings) { applyF(settings, Value); }
        public override object GetFromRaw(TSettings settings) { return loadF(settings); }
        public override void SetToRaw(TSettings settings, object value) { applyF(settings, (TValue)value); }

        public override ValidationResult Validation
        {
            get
            {
                if (validateF != null)
                    return validateF(Value);
                return new ValidationResult();
            }
        }
    }
}
