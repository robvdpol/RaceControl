namespace RaceControl.Services.Interfaces.Lark
{
    public interface ILarkRequest
    {
        ILarkRequest WithField(string field, bool expand = false);

        ILarkRequest WithSubField(string field, string subField, bool expand = false);

        ILarkRequest WithSubSubField(string field, string subField, string subSubField);

        ILarkRequest WithFilter(string field, LarkFilterType filterType, string filterValue);

        ILarkRequest OrderBy(string field, LarkSortDirection direction);

        string GetURL();
    }
}