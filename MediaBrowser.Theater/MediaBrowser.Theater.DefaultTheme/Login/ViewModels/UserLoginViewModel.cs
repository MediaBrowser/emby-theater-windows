using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Login.ViewModels
{
    public class UserLoginViewModel
        : BaseViewModel, IHasImage
    {
        public bool HasImage { get; private set; }
        public BitmapImage Image { get; private set; }
    }
}
