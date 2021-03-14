using RaceControl.Services.Interfaces.F1TV.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaceControl.Services.F1TV
{
    internal class SessionComparer : IEqualityComparer<Session>
    {
        public bool Equals(Session x, Session y)
        {
            return x?.ContentID == y?.ContentID;
        }

        public int GetHashCode([DisallowNull] Session session)
        {
            return session.ContentID.GetHashCode();
        }
    }
}
