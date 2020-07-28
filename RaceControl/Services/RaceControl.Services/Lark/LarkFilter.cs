using RaceControl.Services.Interfaces.Lark;

namespace RaceControl.Services.Lark
{
    public class LarkFilter : LarkField
    {
        public LarkFilter(string fieldName, LarkFilterType filterType, string filterValue) : base(fieldName)
        {
            FilterType = filterType;
            FilterValue = filterValue;
        }

        public LarkFilterType FilterType { get; }

        public string FilterValue { get; }

        public string GetFilterKey()
        {
            switch (FilterType)
            {
                case LarkFilterType.GreaterThan:
                    return $"{FieldName}__gt";

                case LarkFilterType.LessThan:
                    return $"{FieldName}__lt";

                default:
                    return FieldName;
            }
        }
    }
}