// Copyright © 2021 Ravahn - All Rights Reserved
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see<http://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.IO;

namespace Machina.FFXIV.Oodle
{
    public static class OodleFactory
    {
        private static IOodleNative _oodleNative;
        private static readonly object _lock = new object();
        private static bool _internalLibraryAvailable = true;
        private static bool _internalLibraryInitialized = false;
        private static string _internallibraryName = "oo2net_9_win64.dll";

        public static void AutoChooseImplementation(OodleImplementation implementation, string path)
        {
            lock (_lock)
            {
                if (_internalLibraryAvailable && !_internalLibraryInitialized)
                {
                    if (!(_oodleNative is OodleNative_Library))
                        _oodleNative?.UnInitialize();
                    _oodleNative = new OodleNative_Library();
                    string pathLibrary = Path.Combine(System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase).LocalPath), "Plugins", "FFXIV_ACT_Plugin", _internallibraryName);
                    if (File.Exists(pathLibrary))
                    {
                        Trace.WriteLine($"{nameof(OodleNative_Library)}: Found oddle dll at {pathLibrary}, loading...", "DEBUG-MACHINA");
                        _oodleNative.Initialize(pathLibrary);
                        if (!_oodleNative.IsInitialized)
                        {
                            Trace.WriteLine($"{nameof(OodleNative_Library)}: Oddle dll load failed, fallback to game executable...", "DEBUG-MACHINA");
                            _internalLibraryAvailable = false;
                        }
                        else
                        {
                            _internalLibraryInitialized = true;
                            return;
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"{nameof(OodleNative_Library)}: Could not found oddle dll at {pathLibrary}, fallback to game executable...", "DEBUG-MACHINA");
                        _internalLibraryAvailable = false;
                    }

                }
                else if (_internalLibraryInitialized)
                {
                    return;
                }

                // Note: Do not re-initialize if not changing implementation type.
                if (implementation == OodleImplementation.Library)
                {
                    if (!(_oodleNative is OodleNative_Library))
                        _oodleNative?.UnInitialize();
                    else
                        return;
                    _oodleNative = new OodleNative_Library();
                }
                else
                {
                    if (!(_oodleNative is OodleNative_Ffxiv))
                        _oodleNative?.UnInitialize();
                    else
                        return;
                    _oodleNative = new OodleNative_Ffxiv();
                }
                _oodleNative.Initialize(path);
            }
        }

        public static Oodle Create()
        {
            lock (_lock)
            {
                if (_oodleNative is null)
                    return null;
                return new Oodle(_oodleNative);
            }
        }
    }
}
