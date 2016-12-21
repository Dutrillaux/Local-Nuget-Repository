using System;

namespace LocalNugetManager.Business
{
    public class NugetPackage
    {
        private string _name;
        private string _path;

        public NugetPackage(string fullPath)
        {
            FullPath = fullPath;
        }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                    return _name;

                ExtractValuesFromFullPath(FullPath);

                return _name;
            }
        }

        public string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(_path))
                    return _path;

                ExtractValuesFromFullPath(FullPath);

                return _path;
            }
        }

        public string FullPath { get; }


        private void ExtractValuesFromFullPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return;

            var indexOfLastSlash = fullPath.LastIndexOf(@"\", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfLastSlash <= 0) return;

            _name = fullPath.Substring(indexOfLastSlash + 1);
            _path = fullPath.Replace(Name, "");
        }
    }
}