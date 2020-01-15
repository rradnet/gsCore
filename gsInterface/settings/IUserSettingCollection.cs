using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace gs.interfaces
{
    /// <summary>
    /// Provides a common interface to raw settings. 
    /// </summary>
    /// <remarks>
    /// The common version of this interface is provided so that calling code 
    /// doesn't need to know about the type of the underlying raw setting.
    /// However, concrete classes should not use this interface directly;
    /// rather, they should implement the typed version IUserSettingCollection<TSettings>
    /// to enforce matching types for the raw settings object.
    /// </remarks>
    public interface IUserSettingCollection
    {
        IEnumerable<UserSetting> Settings();

        List<ValidationResult> Validate(object rawSettings);

        void LoadFromRaw(object rawSettings,
                         IEnumerable<UserSetting> userSettings);

        void ApplyToRaw(object rawSettings, 
                        IEnumerable<UserSetting> userSettings);

        void SetCulture(CultureInfo cultureInfo);
    }

    public interface IUserSettingCollection<TSettings> : IUserSettingCollection
    {
        new IEnumerable<UserSetting<TSettings>> Settings();

        List<ValidationResult> Validate(TSettings rawSettings);

        void LoadFromRaw(TSettings rawSettings, 
                         IEnumerable<UserSetting<TSettings>> userSettings);
        
        void ApplyToRaw(TSettings rawSettings, 
                        IEnumerable<UserSetting<TSettings>> userSettings);
    }

    public abstract class UserSettingCollection<TSettings> : IUserSettingCollection<TSettings>
    {
        /// <summary>
        /// Provides iteration through user settings typed with underlying raw settings type.
        /// </summary>
        public IEnumerable<UserSetting<TSettings>> Settings()
        {
            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
                if (typeof(UserSetting<TSettings>).IsAssignableFrom(property.PropertyType))
                {
                    var setting = (UserSetting<TSettings>)property.GetValue(this);
                    if (!setting.Hidden)
                        yield return setting;
                }

            
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo field in fields)
                if (typeof(UserSetting<TSettings>).IsAssignableFrom(field.FieldType))
                {
                    var setting = (UserSetting<TSettings>)field.GetValue(this);
                    if (!setting.Hidden)
                        yield return setting;
                }
        }

        /// <summary>
        /// Provides iteration through user settings without needing underlying raw settings type. 
        /// </summary>
        /// <remarks>
        /// Common version of generic method IUserSettingCollection<TSettings>.Settings()
        /// </remarks>
        IEnumerable<UserSetting> IUserSettingCollection.Settings()
        {
            foreach (var setting in Settings())
            {
                yield return setting;
            }
        }

        /// <summary>
        /// Checks the individual validations of each user setting.
        /// </summary>
        /// <remarks>
        /// This method can be overridden in derived classes to add validations
        /// between combinations of user settings in addition to the individual checks.
        /// </remarks>
        public virtual List<ValidationResult> Validate(TSettings settings)
        {
            var validations = new List<ValidationResult>();
            foreach (var userSetting in Settings())
            {
                userSetting.LoadFromRaw(settings);
                var validation = userSetting.Validation;

                if (validation.Severity != ValidationResult.Level.None)
                {
                    validations.Add(new ValidationResult(validation.Severity, validation.Message, userSetting.Name));
                }
            }
            return validations;
        }

        public List<ValidationResult> Validate(object rawSettings)
        {
            return Validate((TSettings)rawSettings);
        }


        /// <summary>
        /// Loads values from raw settings into a collection of user settings.
        /// </summary>
        public void LoadFromRaw(TSettings rawSettings, IEnumerable<UserSetting<TSettings>> userSettings)
        {
            foreach (var setting in userSettings)
            {
                setting.LoadFromRaw(rawSettings);
            }
        }

        /// <summary>
        /// Loads values from raw settings into a collection of user settings.
        /// </summary>
        public void LoadFromRaw(object rawSettings, IEnumerable<UserSetting> userSettings)
        {
            var userSettingsTyped = new List<UserSetting<TSettings>>();
            foreach (var setting in userSettings)
                userSettingsTyped.Add((UserSetting<TSettings>)setting);
            LoadFromRaw((TSettings)rawSettings, userSettingsTyped);
        }

        /// <summary>
        /// Loads values from collection of user settings into raw settings.
        /// </summary>
        public void ApplyToRaw(TSettings rawSettings, IEnumerable<UserSetting<TSettings>> userSettings)
        {
            foreach (var setting in userSettings)
            {
                setting.ApplyToRaw(rawSettings);
            }
        }

        /// <summary>
        /// Loads values from collection of user settings into raw settings.
        /// </summary>
        public void ApplyToRaw(object rawSettings, IEnumerable<UserSetting> userSettings)
        {
            ApplyToRaw((TSettings)rawSettings, (IEnumerable<UserSetting<TSettings>>)userSettings);
        }

        public abstract void SetCulture(CultureInfo cultureInfo);
    }


}