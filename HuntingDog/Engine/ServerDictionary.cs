using System;
using System.Collections.Generic;
using System.Text;
using HuntingDog.DogEngine;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;


using EnvDTE80;
using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Linq;
using StringUtils;

namespace DatabaseObjectSearcher
{
    public enum EResultBehaviour : int
    {
        ByUsage = 1,
        Alphabetically = 2
    }

    public class ObjectFilter
    {
        public bool ShowTables { get; set; }
        public bool ShowSP { get; set; }
        public bool ShowViews { get; set; }
        public bool ShowFunctions { get; set; }

    }

    public class SearchCriteria
    {
        public string Schema { get; set; }
        public int FilterType { get; set; }
        public string[] CritariaAnd { get; set; }
        public EResultBehaviour ResultBehaviour { get; set; }
    }

  
}
