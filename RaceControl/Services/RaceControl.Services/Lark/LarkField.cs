namespace RaceControl.Services.Lark
{
    public class LarkField
    {
        public LarkField(string fieldName, bool expand = false)
        {
            FieldName = fieldName;
            Expand = expand;
        }

        public string FieldName { get; }

        public bool Expand { get; }
    }
}