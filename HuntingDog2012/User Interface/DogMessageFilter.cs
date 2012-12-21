using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace HuntingDog.UI
{
    public interface IKeyNotification
    {

    }

    public class DogMessageFilter : IMessageFilter
    {
        public event Action<Message> KeyPressed;
        public DogMessageFilter Create()
        {
            var d = new DogMessageFilter();
            Application.AddMessageFilter(d);
            return d;
        }

        public bool PreFilterMessage(ref Message m)
        {
            return false;
        }
    }
}
