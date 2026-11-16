using System;
using System.Diagnostics;
using System.Threading;

namespace WorstPC
{
    internal static class Launcher
    {
        private static Mutex _singleton;   // held so Roblox allows more than one client

        // Holding the Roblox single-instance lock lets several clients run at once.
        public static void SetMultiRoblox(bool on)
        {
            if (on)
            {
                if (_singleton == null)
                {
                    try { _singleton = new Mutex(true, "ROBLOX_singletonEvent"); }
                    catch { _singleton = null; }
                }
            }
            else
            {
                if (_singleton != null)
                {
                    try { _singleton.ReleaseMutex(); } catch { }
                    try { _singleton.Dispose(); } catch { }
                    _singleton = null;
                }
            }
        }

        // Build the roblox-player protocol string (RAM's exact format) and launch it.
        public static bool Launch(string ticket, string placeId, out string error)
        {
            error = "";
            try
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string btid = "0";
                string plu = "https://assetgame.roblox.com/game/PlaceLauncher.ashx?request=RequestGame"
                           + "&browserTrackerId=" + btid
                           + "&placeId=" + placeId
                           + "&isPlayTogetherGame=false";
                string proto = "roblox-player://1/1"
                             + "+launchmode:play"
                             + "+gameinfo:" + ticket
                             + "+launchtime:" + now
                             + "+browsertrackerid:" + btid
                             + "+placelauncherurl:" + plu
                             + "+robloxLocale:en_us+gameLocale:en_us";

                ProcessStartInfo psi = new ProcessStartInfo(proto);
                psi.UseShellExecute = true;
                Process.Start(psi);
                return true;
            }
            catch (Exception e) { error = e.Message; return false; }
        }
    }
}
