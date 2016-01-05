using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emby.Theater
{
    public static class ExtensionMethods
    {
        public static void InvokeIfRequired(this Control c, Action action)
        {
            c.Invoke(action);
        }
    }
}
