using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetManager.Business
{
    public class SynchronizeLocalRepository
    {
        private readonly string _destinationFolder;

        public SynchronizeLocalRepository(string destinationFolder)
        {
            _destinationFolder = destinationFolder;
        }

        public void JustDoIt(string solutionPath)
        {
            try
            {
                if (string.IsNullOrEmpty(_destinationFolder)) return;
                if (string.IsNullOrEmpty(solutionPath)) return;

                CleanNonVersionnedNubetPackages(_destinationFolder, true);
                var alreadyStoredPackages = GetPackagesFromCurrentPath(_destinationFolder);
                var newPackages = GetPackagesFromSolutionPath(solutionPath);

                var packageNeededToBeCopied = GetPackagesNeededToBeCopied(alreadyStoredPackages, newPackages);
                CopyPackages(packageNeededToBeCopied, _destinationFolder);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static void CopyPackages(IEnumerable<NugetPackage> packageNeededToBeCopied, string destinationFolder)
        {
            foreach (var packageToCopy in packageNeededToBeCopied)
            {
                DirectoryCopy(packageToCopy.FullPath, Path.Combine(destinationFolder, packageToCopy.Name), true);
            }
        }

        public IEnumerable<NugetPackage> GetPackagesNeededToBeCopied(List<NugetPackage> alreadyStoredPackages,
            List<NugetPackage> newPackages)
        {
            var result = newPackages.Where(x => (HasNugetfolderVersionNumber(x.FullPath) || IsBeta(x.FullPath)) && alreadyStoredPackages.All(y => y.Name != x.Name));
            return result;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs) return;

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true);
            }
        }

        private static List<NugetPackage> GetPackagesFromCurrentPath(string path)
        {
            return GetPackagesFromPathes(new List<string> {path});
        }

        private static List<NugetPackage> GetPackagesFromSolutionPath(string path, string template = "Packages")
        {
            var packages = new List<NugetPackage>();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return packages;

            var allPackagesDirectories = Directory.GetDirectories(path, template, SearchOption.AllDirectories).ToList();

            return GetPackagesFromPathes(allPackagesDirectories);
        }

        private static List<NugetPackage> GetPackagesFromPathes(List<string> listPath)
        {
            var packages = new List<NugetPackage>();

            if (!listPath.Any()) return packages;

            packages.AddRange(from packagesDirectory in listPath
                from directory in Directory.GetDirectories(packagesDirectory)
                select new NugetPackage(directory));

            return packages;
        }

        public bool HasNugetfolderVersionNumber(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return false;

            try
            {
                string lastSegment;
                if (!GetLastSegmentValue(folder, out lastSegment)) return false;
                int number;
                return int.TryParse(lastSegment, out number);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return false;
        }

        public bool IsBeta(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return false;

            try
            {
                string lastSegment;
                if (!GetLastSegmentValue(folder, out lastSegment)) return false;

                return lastSegment.ToLowerInvariant().Contains("beta");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return false;
        }

        private static bool GetLastSegmentValue(string folder, out string lastSegment)
        {
            lastSegment = string.Empty;

            var segments = folder.Replace(@"\", "").Split('.');

            if (segments.Length <= 0) return false;

            lastSegment = segments[segments.Length - 1];
            return true;
        }

        private void CleanNonVersionnedNubetPackages(string path, bool destinationPath)
        {
            if (!Directory.Exists(path)) return;

            if (!destinationPath)
            {
                var parentDirectory = Directory.GetParent(path);
                if (string.IsNullOrEmpty(parentDirectory.Name)) return;
                if (!parentDirectory.Name.Contains("packages")) return;
            }

            var folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            var foldersToDelete = GetFoldersToDelete(folders);

            foreach (var folder in foldersToDelete)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        public IEnumerable<string> GetFoldersToDelete(IEnumerable<string> folders)
        {
            var foldersToDelete = new List<string>();
            foreach (var folder in folders)
            {
                try
                {
                    if (HasNugetfolderVersionNumber(folder)) continue;
                    foldersToDelete.Add(folder);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            return foldersToDelete;
        }
    }
}