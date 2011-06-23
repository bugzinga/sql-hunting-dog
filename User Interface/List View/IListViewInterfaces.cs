using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseObjectSearcherUI
{
    public interface IListViewItem
    {
        Control Control { get; }
        bool Selection { get; set; }
        object BoundObject { get;set; }
        event ListHandler OnClicked;
        event ListHandler OnKeyPressed;
        event ListHandler OnDoubleClicked;
        event ListHandler<ActionArgs> OnAction;
    }

    public delegate void ListHandler(IListViewItem sender,EventArgs args);
    public delegate void ListHandler<T>(IListViewItem sender, T args);

    public interface IListViewItemFactory
    {
        IListViewItem CreateNew(object boundObject);
    }

    public enum EAction
    {
        Locate,
        Execute,
        MoveTo
    }

    public class ActionArgs : EventArgs
    {
        public EAction Action { get; set; }
    }


}
