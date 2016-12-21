using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace LocalNugetManager.Component
{
    public class UpdateManager
    {
        private static bool ShouldCheckForNewVersion()
        {
            var lastCheck = Settings.Default.LastVersionCheck;
            var nextCheck = lastCheck.AddDays(1);

            return nextCheck <= DateTime.Now;
        }

        private static bool IsSubscribedToBeta()
        {
            return Settings.Default.BetaChannelUpdate;
        }

        private static bool IsUpdateModuleConfigured()
        {
            return !string.IsNullOrEmpty(Settings.Default.RemoteVersionPath) &&
                   !string.IsNullOrEmpty(Settings.Default.RemoteVsixPath);
        }

        private static bool IsUpdateBetaModuleConfigured()
        {
            return !string.IsNullOrEmpty(Settings.Default.RemoteVersionPath) &&
                   !string.IsNullOrEmpty(Settings.Default.RemoteVsixPath_Beta);
        }

        private VersionUpdate _CheckUpdateAvailable(bool betaCheck)
        {
            var remotePath = Settings.Default.RemoteVersionPath;

            if (!File.Exists(remotePath)) return VersionUpdate.NoUpdate;

            Settings.Default.LastVersionCheck = DateTime.Now;
            Settings.Default.Save();

            try
            {
                return ComputeUpdateResult(remotePath, betaCheck);
            }
            catch
            {
                return VersionUpdate.NoUpdate;
            }
        }

        private static Version GetCurrentVersion()
        {
            var currentPackageAssembly = Assembly.GetExecutingAssembly();
            var currentVersionStr = FileVersionInfo.GetVersionInfo(currentPackageAssembly.Location).FileVersion;
            return new Version(currentVersionStr);
        }

        private static VersionUpdate ComputeUpdateResult(string remotePath, bool isBetaChannel)
        {
            var remoteContent = File.ReadAllText(remotePath);
            var lines = remoteContent.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            var masterVersionStr = lines[1].Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            var betaVersionStr = lines[0].Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            var masterVersion = new Version(masterVersionStr);
            var betaVersion = new Version(betaVersionStr);

            var remoteVersion = masterVersion;
            var isBetaNewer = false;

            if (isBetaChannel && betaVersion > masterVersion)
            {
                isBetaNewer = true;
                remoteVersion = betaVersion;
            }

            var isUpdateAvailable = remoteVersion > GetCurrentVersion();
            return new VersionUpdate(isUpdateAvailable, isBetaNewer);
        }

        private VersionUpdate _IsUpdateAvailable()
        {
            if (!IsUpdateModuleConfigured()) return VersionUpdate.NoUpdate;
            if (!ShouldCheckForNewVersion()) return VersionUpdate.NoUpdate;
            
            var betaCheck = IsSubscribedToBeta() && IsUpdateBetaModuleConfigured();

            return _CheckUpdateAvailable(betaCheck);
        }

        public bool IsUpdateAvailable()
        {
            return _IsUpdateAvailable().IsUpdateAvailable;
        }

        private static bool Update(VersionUpdate updateResult)
        {
            if (!updateResult.IsUpdateAvailable) return false;

            var remoteVsix = updateResult.IsBeta
                ? Settings.Default.RemoteVsixPath_Beta
                : Settings.Default.RemoteVsixPath;
            if (!File.Exists(remoteVsix)) return false;

            Process.Start(remoteVsix);

            return true;
        }

        private static void AskAndProcessUpdate(VersionUpdate updateResult)
        {
            if (MessageBox.Show(
                "There is an update available for 'Local nuget manager' extension !" + Environment.NewLine +
                "Do you want to download it now ?", "Yeah, new shiny stuff !", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                Update(updateResult);
            }
        }

        public void ForceCheckAndProcessUpdate()
        {
            if (!IsUpdateModuleConfigured()) return;

            var betaCheck = IsSubscribedToBeta() && IsUpdateBetaModuleConfigured();
            var updateResult = _CheckUpdateAvailable(betaCheck);

            if (!updateResult.IsUpdateAvailable) return;

            AskAndProcessUpdate(updateResult);
        }

        public void CheckAndProcessUpdate()
        {
            if (!IsUpdateModuleConfigured()) return;

            var updateResult = _IsUpdateAvailable();
            if (!updateResult.IsUpdateAvailable) return;

            AskAndProcessUpdate(updateResult);
        }

        private class VersionUpdate
        {
            public static readonly VersionUpdate NoUpdate = new VersionUpdate(false, false);

            public VersionUpdate(bool isUpdateAvailable, bool isBeta)
            {
                IsUpdateAvailable = isUpdateAvailable;
                IsBeta = isBeta;
            }

            public bool IsUpdateAvailable { get; }
            public bool IsBeta { get; }
        }
    }
}