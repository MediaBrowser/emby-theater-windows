using System.Collections.Generic;
using System.Windows.Forms;
using MediaInfoDotNet;

namespace Mpdn.PlayerExtensions.GitHub
{
    public partial class ViewMediaInfoForm : Form
    {
        public ViewMediaInfoForm(string path)
        {
            InitializeComponent();

            var mediaFile = new MediaFile(path);

            var lines = new List<string>();
            {
                lines.Add(string.Format("Unique ID            : {0}", mediaFile.uniqueId));
                lines.Add(string.Format("Complete name        : {0}", path));
                lines.Add(string.Format("Format               : {0}", mediaFile.format));
                lines.Add(string.Format("File size            : {0}", mediaFile.size));
                lines.Add(string.Format("Duration             : {0}", mediaFile.duration));
                lines.Add(string.Format("Overall bit rate     : {0}", mediaFile.bitRate));
                lines.Add(string.Format("Encoded date         : {0}", mediaFile.encodedDate));
                lines.Add(string.Format("Writing application  : {0}", mediaFile.miGetString("Encoded_Application")));
                lines.Add(string.Format("Writing library      : {0}", mediaFile.encodedLibrary));
            }                                                 
                                                              
            lines.Add(string.Empty);                          
            lines.Add("Video");                               
            foreach (var info in mediaFile.Video)
            {
                double bitPerFrame = info.Value.size*8/
                                     ((double) info.Value.width*info.Value.height*info.Value.frameCount);
                lines.Add(string.Format("ID                   : {0}", info.Value.uniqueId));
                lines.Add(string.Format("Format               : {0}", info.Value.format));
                lines.Add(string.Format("Format profile       : {0}", info.Value.miGetString("Format_Profile")));
                lines.Add(string.Format("Format settings      : {0}", info.Value.miGetString("Format_Settings")));
                lines.Add(string.Format("Codec ID             : {0}", info.Value.codecId));
                lines.Add(string.Format("Duration             : {0}", info.Value.duration));
                lines.Add(string.Format("Bit rate             : {0}", info.Value.bitRate));
                lines.Add(string.Format("Width                : {0}", info.Value.width));
                lines.Add(string.Format("Height               : {0}", info.Value.height));
                lines.Add(string.Format("Display aspect ratio : {0}", info.Value.pixelAspectRatio));
                lines.Add(string.Format("Frame rate mode      : {0}", info.Value.frameRateMode));
                lines.Add(string.Format("Frame rate           : {0}", info.Value.frameRate));
                lines.Add(string.Format("Chroma subsampling   : {0}", info.Value.miGetString("ChromaSubsampling")));
                lines.Add(string.Format("Bit depth            : {0}", info.Value.bitDepth));
                lines.Add(string.Format("Scan type            : {0}", info.Value.miGetString("ScanType")));
                lines.Add(string.Format("Bits/(Pixel*Frame)   : {0}", bitPerFrame.ToString("0.#####")));
                lines.Add(string.Format("Stream size          : {0}", info.Value.miGetString("StreamSize")));
                lines.Add(string.Format("Writing library      : {0}", info.Value.encodedLibrary));
                lines.Add(string.Format("Encoding settings    : {0}", info.Value.encoderSettingsRaw));
                lines.Add(string.Format("Language             : {0}", info.Value.language));
            }

            lines.Add(string.Empty);

            lines.Add("Audio");
            foreach (var info in mediaFile.Audio)
            {
                lines.Add(string.Format("ID                   : {0}", info.Value.uniqueId));
                lines.Add(string.Format("Format               : {0}", info.Value.format));
                lines.Add(string.Format("Codec ID             : {0}", info.Value.codecId));
                lines.Add(string.Format("Duration             : {0}", info.Value.duration));
                lines.Add(string.Format("Bit rate mode        : {0}", info.Value.bitRateMode));
                lines.Add(string.Format("Bit rate             : {0}", info.Value.bitRate));
                lines.Add(string.Format("Channel(s)           : {0}", info.Value.channels));
                lines.Add(string.Format("Sampling rate        : {0}", info.Value.sampleRate));
                lines.Add(string.Format("Bit depth            : {0}", info.Value.bitDepth));
                lines.Add(string.Format("Compression mode     : {0}", info.Value.compressionMode));
                lines.Add(string.Format("Stream size          : {0}", info.Value.size));
                lines.Add(string.Format("Language             : {0}", info.Value.language));
            }

            lines.Add(string.Empty);

            lines.Add("Subtitle");
            foreach (var info in mediaFile.Text)
            {
                lines.Add(string.Format("ID                   : {0}", info.Value.uniqueId));
                lines.Add(string.Format("Format               : {0}", info.Value.format));
                lines.Add(string.Format("Muxing Mode          : {0}", info.Value.muxingMode));
                lines.Add(string.Format("Codec ID             : {0}", info.Value.codecId));
                lines.Add(string.Format("Language             : {0}", info.Value.language));
            }

            textBoxInfo.Lines = lines.ToArray();
        }
    }
}
