﻿/**
 * SPDX-FileCopyrightText: 2011-2023 EasyCoding Team
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
*/

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;

namespace mhed.lib
{
    /// <summary>
    /// Class for working with Microsoft Windows specific functions.
    /// </summary>
    public class PlatformWindows : CurrentPlatform
    {
        /// <summary>
        /// Open the specified text file in default (or overrided in application's
        /// settings (only on Windows platform)) text editor.
        /// </summary>
        /// <param name="FileName">Full path to text file.</param>
        /// <param name="EditorBin">External text editor (Windows only).</param>
        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void OpenTextEditor(string FileName, string EditorBin)
        {
            Process.Start(EditorBin, AddQuotesToPath(FileName));
        }

        /// <summary>
        /// Show the specified file in default file manager.
        /// </summary>
        /// <param name="FileName">Full path to file.</param>
        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void OpenExplorer(string FileName)
        {
            Process.Start(Properties.Resources.ShBinWin, string.Format("{0} \"{1}\"", Properties.Resources.ShParamWin, FileName));
        }

        /// <summary>
        /// Start the required application from administrator.
        /// </summary>
        /// <param name="FileName">Full path to the executable.</param>
        /// <returns>PID of the newly created process.</returns>
        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override int StartElevatedProcess(string FileName)
        {
            // Setting advanced properties...
            ProcessStartInfo ST = new ProcessStartInfo
            {
                FileName = FileName,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = true
            };

            // Starting process...
            Process NewProcess = Process.Start(ST);

            // Returning PID of created process...
            return NewProcess.Id;
        }

        /// <summary>
        /// Get platform-dependent suffix for HTTP_USER_AGENT header.
        /// </summary>
        public override string UASuffix => Properties.Resources.AppUASuffixWin;

        /// <summary>
        /// Get current operating system ID.
        /// </summary>
        public override OSType OS => OSType.Windows;

        /// <summary>
        /// Return whether automatic updates are supported on this platform.
        /// </summary>
        public override bool AutoUpdateSupported => true;

        /// <summary>
        /// Return whether Hosts file header is required on this platform.
        /// </summary>
        public override bool HostsFileHeader => true;

        /// <summary>
        /// Return whether localhost entry is required on this platform.
        /// </summary>
        public override bool LocalHostEntry => false;

        /// <summary>
        /// Return platform-dependent location of the Hosts file.
        /// </summary>
        public override string HostsFileLocation
        {
            get
            {
                string HostsDirectory;

                try
                {
                    using (RegistryKey ResKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", false))
                    {
                        HostsDirectory = (string)ResKey.GetValue("DataBasePath");
                    }
                }
                catch
                {
                    HostsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "drivers", "etc");
                }

                return HostsDirectory;
            }
        }
    }
}
