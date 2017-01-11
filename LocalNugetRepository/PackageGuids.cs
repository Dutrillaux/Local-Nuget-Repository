using System;

namespace LocalNugetRepository
{
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidSynchronizeNugetRepositoryPackageString = "31509939-52ff-4c92-aa7a-07817bd0cfba";
        public const string guidSynchronizeNugetRepositoryPackageCmdSetString = "07964cc2-007f-459b-bbfd-c07d216f073e";
        public const string guidImagesString = "5999fd7f-ff31-497c-94c0-e283a7c15bcf";
        public static Guid guidSynchronizeNugetRepositoryPackage = new Guid(guidSynchronizeNugetRepositoryPackageString);
        public static Guid guidSynchronizeNugetRepositoryPackageCmdSet = new Guid(guidSynchronizeNugetRepositoryPackageCmdSetString);
        public static Guid guidImages = new Guid(guidImagesString);
    }
}