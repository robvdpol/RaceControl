using RaceControl.Services.Interfaces.Lark;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaceControl.Services.Lark
{
    public class LarkRequest : ILarkRequest
    {
        private readonly string _endpoint;
        private readonly string _collection;
        private readonly string _id;

        private readonly IList<LarkField> _fields = new List<LarkField>();
        private readonly IList<LarkFilter> _filters = new List<LarkFilter>();

        private string _sortField;
        private LarkSortDirection _sortDirection;

        public LarkRequest(string endpoint, string collection, string id)
        {
            _endpoint = endpoint;
            _collection = collection;
            _id = id;
        }

        public ILarkRequest WithField(string field, bool expand = false)
        {
            _fields.Add(new LarkField(field, expand));

            return this;
        }

        public ILarkRequest WithSubField(string field, string subField)
        {
            _fields.Add(new LarkField($"{field}__{subField}"));

            return this;
        }

        public ILarkRequest WithFilter(string field, LarkFilterType filterType, string filterValue)
        {
            _filters.Add(new LarkFilter(field, filterType, filterValue));

            return this;
        }

        public ILarkRequest OrderBy(string field, LarkSortDirection direction)
        {
            _sortField = field;
            _sortDirection = direction;

            return this;
        }

        public string GetURL()
        {
            var sb = new StringBuilder();
            sb.Append(_endpoint);
            sb.Append("/");
            sb.Append(_collection);

            if (!string.IsNullOrWhiteSpace(_id))
            {
                sb.Append("/");
                sb.Append(_id);
            }

            var queryParams = new Dictionary<string, string>();

            if (_fields.Any())
            {
                queryParams.Add("fields", string.Join(",", _fields.Select(field => field.FieldName)));

                if (_fields.Any(field => field.Expand))
                {
                    queryParams.Add("fields_to_expand", string.Join(",", _fields.Where(field => field.Expand).Select(field => field.FieldName)));
                }
            }

            foreach (var filter in _filters)
            {
                queryParams.Add(filter.GetFilterKey(), filter.FilterValue);
            }

            if (!string.IsNullOrWhiteSpace(_sortField))
            {
                var sortValue = _sortDirection == LarkSortDirection.Ascending ? _sortField : $"-{_sortField}";
                queryParams.Add("order", sortValue);
            }

            if (queryParams.Any())
            {
                sb.Append("?");
                sb.Append(string.Join("&", queryParams.Select(pair => $"{pair.Key}={pair.Value}")));
            }

            return sb.ToString();
        }
    }
}