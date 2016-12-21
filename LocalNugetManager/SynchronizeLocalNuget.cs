using System;
using System.ComponentModel.Design;
using System.IO;
using System.Timers;
using EnvDTE;
using LocalNugetManager.Business;
using Microsoft.VisualStudio.Shell;

namespace LocalNugetManager
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class SynchronizeLocalNuget
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6ae7eb86-508f-41b3-90ec-48b6de46a4c6");

        /// <summary>
        ///     VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private Timer _timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizeLocalNuget" /> class.
        ///     Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SynchronizeLocalNuget(Package package)
        {
            try
            {
                if (package == null)
                {
                    throw new ArgumentNullException("package");
                }


                InitializeTimer();


                _package = package;

                var solutionFolder = GetSolutiondirectory();
                if (string.IsNullOrEmpty(solutionFolder))
                    return;

                var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (commandService != null)
                {
                    var menuCommandId = new CommandID(CommandSet, CommandId);
                    var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                    commandService.AddCommand(menuItem);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static string SolutionPath => Instance.GetSolutiondirectory();

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static SynchronizeLocalNuget Instance { get; private set; }

        /// <summary>
        ///     Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        private void InitializeTimer()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Elapsed -= OnTimedEvent;
                }

                if (!Settings.Default.AutoSynchronize) return;

                if (_timer == null)
                {
                    _timer = new Timer();
                }
                _timer.AutoReset = true;
                _timer.Interval = Settings.Default.DelayBetweenSynchronizationInMinute * 60 * 1000;
                _timer.Elapsed += OnTimedEvent;

                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer?.Stop();

                MenuItemCallback(null, null);

                if (_timer == null) return;

                if (Settings.Default.AutoSynchronize)
                {
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private string GetSolutiondirectory()
        {
            try
            {
                var dte = (DTE)ServiceProvider.GetService(typeof(DTE));

                if (!string.IsNullOrEmpty(dte?.Solution?.FullName))
                {
                    return Path.GetDirectoryName(dte.Solution.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return string.Empty;
        }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            try
            {
                Instance = new SynchronizeLocalNuget(package);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked.
        ///     See the constructor to see how the menu item is associated with this function using
        ///     OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Default == null || string.IsNullOrEmpty(Settings.Default.LocalFolderForRepository))
                {
                    Logger.LogMessage("Settings.Default.LocalFolderForRepository is null");
                    return;
                }

                var localFolder = Settings.Default.LocalFolderForRepository;
                var synchronizeRepo = new SynchronizeLocalRepository(localFolder);
                synchronizeRepo.JustDoIt();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}