// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mpdn.PlayerExtensions
{
    public class RemoteControl_AuthHandler
    {
        #region Variables

        private string folder;
        private string _subPath = @"MediaPlayerDotNet\RemoteControl";
        private string filePath = @"accessGUID.conf";
        private Guid nullGUID = Guid.Parse("{00000000-0000-0000-0000-000000000000}");
        private string fullPath;
        private List<Guid> authedClients = new List<Guid>();
        #endregion

        public RemoteControl_AuthHandler()
        {
            var envPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(envPath, _subPath);
            fullPath = Path.Combine(folder, filePath);
            ReadAuthedClients();
        }

        private void ReadAuthedClients()
        {
            if(Directory.Exists(folder))
            {
                var file = File.Open(fullPath, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                String line;
                bool readAgain = false;
                do
                {
                    line = reader.ReadLine();
                    Guid tmpGuid;
                    Guid.TryParse(line, out tmpGuid);
                    if (tmpGuid != nullGUID)
                    {
                        authedClients.Add(tmpGuid);
                    }
                    line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        readAgain = true;
                    else
                        readAgain = false;
                }
                while(readAgain);
                reader.Close();
            }
        }

        public bool IsGUIDAuthed(string ClientGUID)
        {
            bool isAuthed = false;
            Guid tmpGuid;
            Guid.TryParse(ClientGUID, out tmpGuid);
            if(authedClients.Contains(tmpGuid))
            {
                isAuthed = true;
            }
            return isAuthed;
        }

        

        public void AddAuthedClient(string clientGUID)
        {
            FileStream myFile = null;
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            if(!File.Exists(fullPath))
            {
                myFile = File.Create(fullPath);
            }

            try
            {
                if(myFile == null)
                    myFile = File.Open(fullPath, FileMode.Append, FileAccess.Write);
                StreamWriter writer = new StreamWriter(myFile);
                writer.WriteLine(clientGUID);
                writer.Flush();
                writer.Close();
                Guid tmpGuid;
                Guid.TryParse(clientGUID, out tmpGuid);
                authedClients.Add(tmpGuid);
            }
            catch
            {

            }
        }
    }
}
