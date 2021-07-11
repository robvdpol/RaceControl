namespace RaceControl.Core.Settings
{
    public class HotkeyBinding
    {
        public string Key { get; }
        public string Action { get; }
        public string[] Parameters { get; }

        public HotkeyBinding(string key, string action, string[] parameters)
        {
            Key = key;
            Action = action;
            Parameters = parameters;
        }
    }
}
