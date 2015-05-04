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
using System.Net;
using System.Text;

namespace Mpdn.PlayerExtensions.GitHub
{
    #region Subtitle
    public class Subtitle
    {
        private readonly SubtitleDownloader m_Downloader;

        protected internal Subtitle(SubtitleDownloader downloader)
        {
            m_Downloader = downloader;
        }

        public string Lang { get; protected internal set; }
        public string Name { get; protected internal set; }
        public string Movie { get; protected internal set; }
        public int ID { get; protected internal set; }
        public string SRT
        {
            get { return m_Downloader.FetchSubtitleText(this); }
        }

        public void Save()
        {
            m_Downloader.SaveSubtitleFile(this);
        }
    }
    #endregion

    public class SubtitleDownloader
    {
        private static readonly string OSUrl =
            "http://www.opensubtitles.org/isdb/index.php?player={0}&name[0]={1}&size[0]={2}&hash[0]={3}";

        private static readonly string OSDlSub = "http://www.opensubtitles.org/isdb/dl.php?id={0}&ticket={1}";
        private readonly string PlayerId;
		private readonly string UserAgent;
        private readonly WebClient WebClient = new WebClient();
        private string LastTicket;
        private string MediaFilename;


        public SubtitleDownloader(string UserAgent, string PlayerID)
        {
            this.UserAgent = UserAgent;
            this.PlayerId = PlayerID;
        }

        private string DoRequest(string url)
        {
            try
            {
                using (var client = this.WebClient)
                {
                    client.Headers.Set("User-Agent", UserAgent);
                    return client.DownloadString(url);
                }
            }
            catch (Exception)
            {                
                throw new InternetConnectivityException();
            }
  
        }
        /// <summary>
        /// Get Subtitles for the file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public List<Subtitle> GetSubtitles(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            var file = new FileInfo(filename);
            if (!file.Exists)
            {
                throw new ArgumentException("File doesn't exist", filename);
            }

            var name = file.Name;
            var size = file.Length.ToString("X");
            var hash = HashCalculator.GetHash(filename);
            MediaFilename = filename;
            string subs = DoRequest(string.Format(OSUrl, PlayerId, name, size, hash));

            if(string.IsNullOrEmpty(subs)) {
                throw new EmptyResponseException();
            }

            return parseSubtitlesResponse(subs);

        }

        private List<Subtitle> parseSubtitlesResponse(string subs)
        {
            var subList = new List<Subtitle>();
            var subtitle = new Subtitle(this);
            foreach (var line in subs.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("ticket="))
                {
                    LastTicket = GetValue(line);
                }
                else if (line.StartsWith("movie="))
                {
                    var value = GetValue(line);
                    subtitle.Movie = value.Remove(value.Length - 1);
                }
                else if (line.StartsWith("subtitle="))
                {
                    subtitle.ID = int.Parse(GetValue(line));
                }
                else if (line.StartsWith("language="))
                {
                    subtitle.Lang = GetValue(line);
                }
                else if (line.StartsWith("name="))
                {
                    subtitle.Name = GetValue(line);
                }
                else if (line.Equals("endsubtitle"))
                {
                    subList.Add(subtitle);
                    subtitle = new Subtitle(this);
                }
            }
            return subList;
        }

        private string GetValue(string line)
        {
            return line.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1];
        }

        public string FetchSubtitleText(Subtitle subtitle)
        {
            var url = string.Format(OSDlSub, subtitle.ID, this.LastTicket);
            string sub = DoRequest(url);
            if (string.IsNullOrEmpty(sub))
            {
                throw new EmptyResponseException();
            }
            return sub;
        }

        public void SaveSubtitleFile(Subtitle subtitle)
        {
            var dir = Path.GetDirectoryName(MediaFilename);
            var subFile = Path.GetFileNameWithoutExtension(MediaFilename) + ".srt";
            var fullPath = Path.Combine(dir, subFile);
            var subs = this.FetchSubtitleText(subtitle);
            if (string.IsNullOrWhiteSpace(subs))
                throw new Exception("Empty Subtitle");
            var subtitleLines = subs.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            File.WriteAllLines(@fullPath, subtitleLines);
        }

    }

    #region HashCalculator
    public class HashCalculator
    {

        public static string GetHash(string filename)
        {
            return ToHexadecimal(ComputeMovieHash(filename));
        }

        private static byte[] ComputeMovieHash(string filename)
        {
            byte[] result;
            using (Stream input = File.OpenRead(filename))
            {
                result = ComputeMovieHash(input);
            }
            return result;
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long lhash, streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            var buffer = new byte[sizeof (long)];
            while (i < 65536/sizeof (long) && (input.Read(buffer, 0, sizeof (long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536/sizeof (long) && (input.Read(buffer, 0, sizeof (long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            var result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            var hexBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }
    }

    #endregion
#region Exceptions
    public class EmptySubtitleException : Exception
    {

    }

    public class EmptyResponseException : Exception
    {

    }

    public class InternetConnectivityException : Exception
    {

    }
#endregion
}