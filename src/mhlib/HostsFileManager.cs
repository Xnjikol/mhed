﻿/**
 * SPDX-FileCopyrightText: 2011-2023 EasyCoding Team
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mhed.lib
{
    /// <summary>
    /// Class for working with Hosts file.
    /// </summary>
    public sealed class HostsFileManager
    {
        /// <summary>
        /// Store information about current running platform.
        /// </summary>
        private readonly CurrentPlatform Platform;

        /// <summary>
        /// Get or set full Hosts file path.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Get or set Hosts file contents.
        /// </summary>
        public SortableBindingList<HostsFileEntry> Contents { get; private set; }

        /// <summary>
        /// Clear Hosts file data object.
        /// </summary>
        private void ClearHostsContents()
        {
            Contents.Clear();
        }

        /// <summary>
        /// Add localhost entry for other than Windows operating systems.
        /// Windows since NT 6.0 (Vista) don't need it.
        /// </summary>
        private void AddLocalHostEntry()
        {
            if (Platform.LocalHostEntry)
            {
                Contents.Add(new HostsFileEntry(IPAddress.Loopback, Hostname.LocalHost));
                Contents.Add(new HostsFileEntry(IPAddress.IPv6Loopback, Hostname.LocalHost));
            }
        }

        /// <summary>
        /// Asynchronically read contents of Hosts file to the data object.
        /// </summary>
        private async Task ReadHostsFile()
        {
            using (StreamReader OpenedHosts = new StreamReader(FilePath, Encoding.Default))
            {
                while (OpenedHosts.Peek() >= 0)
                {
                    string ImpStr = StringsManager.CleanString(await OpenedHosts.ReadLineAsync());
                    if (!string.IsNullOrEmpty(ImpStr))
                    {
                        int SpPos = ImpStr.IndexOf(" ", StringComparison.InvariantCulture);
                        if (SpPos != -1 && IPAddress.TryParse(ImpStr.Substring(0, SpPos), out IPAddress IP) && Hostname.TryParse(ImpStr.Remove(0, SpPos + 1), out Hostname Host))
                        {
                            Contents.Add(new HostsFileEntry(IP, Host));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronically write contents of the data object to the Hosts file.
        /// </summary>
        private async Task WriteHostsFile()
        {
            using (StreamWriter CFile = new StreamWriter(FilePath, false, Encoding.Default))
            {
                if (Platform.HostsFileHeader)
                {
                    await CFile.WriteLineAsync(Properties.Resources.HtTemplate);
                }

                foreach (HostsFileEntry Entry in Contents.Where(e => e.IsValid))
                {
                    await CFile.WriteLineAsync(string.Format("{0} {1}", Entry.IPAddr, Entry.Hostname));
                }
            }
        }

        /// <summary>
        /// Read Hosts file from disk.
        /// </summary>
        public async Task Load()
        {
            await ReadHostsFile();
        }

        /// <summary>
        /// Re-read (refresh) Hosts file from disk.
        /// </summary>
        public async Task Refresh()
        {
            ClearHostsContents();
            await ReadHostsFile();
        }

        /// <summary>
        /// Restore default contents of the Hosts file.
        /// </summary>
        public void Restore()
        {
            ClearHostsContents();
            AddLocalHostEntry();
        }

        /// <summary>
        /// Write Hosts file changes to disk.
        /// </summary>
        public async Task Save()
        {
            await WriteHostsFile();
        }

        /// <summary>
        /// HostsFileManager class constructor.
        /// </summary>
        /// <param name="RunningPlatform">Current running platform.</param>
        public HostsFileManager(CurrentPlatform RunningPlatform)
        {
            Platform = RunningPlatform;
            FilePath = Platform.HostsFileFullPath;
            Contents = new SortableBindingList<HostsFileEntry>();
        }
    }
}
