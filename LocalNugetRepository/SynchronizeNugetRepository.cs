//------------------------------------------------------------------------------
// <copyright file="SynchronizeNugetRepository.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Timers;
using EnvDTE;
using LocalNugetManager;
using LocalNugetRepository.Business;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LocalNugetRepository
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SynchronizeNugetRepository
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("07964cc2-007f-459b-bbfd-c07d216f073e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizeNugetRepository"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SynchronizeNugetRepository(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this._package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            InitializeTimer();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SynchronizeNugetRepository Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            try
            {
                Instance = new SynchronizeNugetRepository(package);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
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
                synchronizeRepo.JustDoIt(SolutionPath);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static string SolutionPath => Instance.GetSolutiondirectory();

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
    }
}
