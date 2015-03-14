using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mpdn.PlayerExtensions
{
    public class MbtBridge : PlayerExtension
    {
        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("EF9EB466-3DFE-4FBB-8C16-D4A91BF7B9F6"),
                    Name = "Media Browser Theater Bridge",
                    Description = "Performs handshaking and configuration required for integration with Media Browser Theater."
                };
            }
        }

        public override IList<Verb> Verbs
        {
            get { return new List<Verb>(); }
        }

        public override async void Initialize()
        {
            base.Initialize();

            bool success = await SendHandshake();
            if (success)
            {
                PlayerControl.Form.FormBorderStyle = FormBorderStyle.None;
                PlayerControl.Form.TopMost = false;
            }
        }

        private async Task<bool> SendHandshake()
        {
            try
            {
                var address = new IPEndPoint(IPAddress.Loopback, 6546);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(address);

                using (var stream = new NetworkStream(socket))
                using (var writer = new StreamWriter(stream) {AutoFlush = true})
                using (var reader = new StreamReader(stream))
                {
                    await writer.WriteLineAsync("Handshake");
                    string response = await reader.ReadLineAsync();
                    return response == "OK";
                }
            }
            catch
            {
                return false;
            }
        }
    }
}