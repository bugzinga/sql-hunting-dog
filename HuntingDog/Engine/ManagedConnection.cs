
using System.Diagnostics.CodeAnalysis;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace DatabaseObjectSearcher
{
    [SuppressMessage("Microsoft.Design", "CA1063")]
    public class ManagedConnection : IManagedConnection
    {
        public SqlOlapConnectionInfoBase Connection
        {
            get;
            set;
        }

        public void Close()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1063")]
        public void Dispose()
        {
        }
    }
}
