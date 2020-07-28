using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Lark;

namespace RaceControl.Services.Lark
{
    public class F1TVClient : LarkClient, IF1TVClient
    {
        public F1TVClient(IRestClient restClient) : base(restClient, "https://f1tv.formula1.com/api")
        {
        }
    }
}