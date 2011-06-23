using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseObjectSearcher
{
    public interface IObjectSearcherUI
    {
        void SetSearchController(MSSQLController controller);
    }
}
