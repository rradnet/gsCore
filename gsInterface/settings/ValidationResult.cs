namespace gs.interfaces
{
    public struct ValidationResult
    {
        public ValidationResult(Level severity, string message, string settingName = null)
        {
            Severity = severity;
            Message = message;
            SettingName = settingName;
        }

        public enum Level { None, Warning, Error }
        public Level Severity;
        public string SettingName;
        public string Message;
    }
}
