using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseObjectSearcherUI
{
    class SearchObjectFactory:IListViewItemFactory
    {
        #region IListViewItemFactory Members

        public IListViewItem CreateNew(object boundObject)
        {
            IListViewItem it;
           
            if (boundObject is Detail || boundObject is IndexDetail)
            {
                it = new ucDetailItem();
            }
            else if (boundObject is DatabaseObjectSearcher.DatabaseDependencyResult)
            {
                it = new ucDependencyItem();
            }
            else
            {
                it = new ucSearchItem2();
            }


            it.BoundObject = boundObject;
            return it;
        }

        #endregion
    }
}
