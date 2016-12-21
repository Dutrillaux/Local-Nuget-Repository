using System.Collections.Generic;
using System.Linq;
using LocalNugetManager.Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LocalNugetManager.Tests
{
    [TestClass]
    public class SynchronizeRepoTests
    {
        private const string PathNugetWithVersion = @"C:\NugetLocal\Global.Mobile.Api.Auth.16.52.383921";
        private const string PathNugetWithVersion2 = @"C:\NugetLocal\Polly.4.2.0";
        private const string PathNugetWithVersion3 = @"C:\NugetLocal\modernhttpclient.2.4.2";

        private const string PathNubet = @"C:\NugetLocal\Global.Mobile.Api.Auth";

        [TestMethod]
        public void Validate_versionned_Number()
        {
            var synchronizeLocalRepository = new SynchronizeLocalRepository("");
            Assert.IsTrue(synchronizeLocalRepository.HasNugetfolderVersionNumber(PathNugetWithVersion));
            Assert.IsFalse(synchronizeLocalRepository.HasNugetfolderVersionNumber(PathNubet));
        }

        [TestMethod]
        public void Validate_Folder_To_Delete()
        {
            var synchronizeLocalRepository = new SynchronizeLocalRepository("");

            var result = synchronizeLocalRepository.GetFoldersToDelete(new List<string>
            {
                PathNugetWithVersion,
                PathNubet
            }).ToList();

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result[0].Equals(PathNubet));
        }

        [TestMethod]
        public void Validate_Number_Of_NugetPackages_To_Be_Copied()
        {
            var alreadyStoredPackages = new List<NugetPackage>
            {
                new NugetPackage(PathNugetWithVersion),
                new NugetPackage(PathNugetWithVersion2)
            };
            var newPackages = new List<NugetPackage>
            {
                new NugetPackage(PathNugetWithVersion2),
                new NugetPackage(PathNugetWithVersion3),
                new NugetPackage(PathNubet)
            };
            
            var synchronizeLocalRepository = new SynchronizeLocalRepository("");
            var result = synchronizeLocalRepository.GetPackagesNeededToBeCopied(alreadyStoredPackages, newPackages).ToList();
            
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First(x => x.FullPath == PathNugetWithVersion3) != null);
        }
    }
}
