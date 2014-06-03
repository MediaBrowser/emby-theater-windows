using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.DirectShow
{
    internal enum DvdMenuMode
    {
        No, 
        Buttons, 
        Still
    }

    public enum VideoScalingScheme
    {
        HALF,
        NORMAL,
        DOUBLE,
        STRETCH,
        FROMINSIDE,
        FROMOUTSIDE,
        ZOOM1,
        ZOOM2
    };
}
