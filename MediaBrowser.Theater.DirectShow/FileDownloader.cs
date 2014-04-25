using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.DirectShow
{
    public class FileDownloader : IDisposable
    {
        ManualResetEvent _mreBlock = new ManualResetEvent(false);
        bool _disposed = false;
        byte[] _result = new byte[]{};
        WebClient _wc = new WebClient();

        public FileDownloader()
        {
            _wc.DownloadDataCompleted += FileDownloader_DownloadDataCompleted;
            _wc.DownloadProgressChanged += _wc_DownloadProgressChanged;
        }

        void _wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void FileDownloader_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            _result = e.Result;
            _mreBlock.Set();
        }

        public byte[] DownloadBlock(Uri address)
        {
            _mreBlock.Reset();
            _wc.DownloadDataAsync(address);
            _mreBlock.WaitOne();
            return _result;
        }

        //protected override void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
        //{
        //    _result = e.Result;
        //    _mreBlock.Set();
        //    base.OnDownloadDataCompleted(e);
        //}

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_mreBlock != null)
                        _mreBlock.Dispose();

                    if (_wc != null)
                        _wc.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
