using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PB.ITOps.Messaging.PatLite.Tools
{
    public class FileTokenCache : TokenCache
    {
        private readonly string _cacheFilePath;
        private static readonly object FileLock = new object();

        public FileTokenCache()
        {
            _cacheFilePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "PatLite","TokenCache.dat");
            var info = new FileInfo(_cacheFilePath);
            info.Directory.Create();

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            lock (FileLock)
            {
                Deserialize(File.Exists(_cacheFilePath) ?
                    ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath),
                        null,
                        DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        public override void Clear()
        {
            base.Clear();
            lock (FileLock)
            {
                File.Delete(_cacheFilePath);
            }
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                Deserialize(File.Exists(_cacheFilePath) ?
                    ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath),
                        null,
                        DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                lock (FileLock)
                {
                    File.WriteAllBytes(_cacheFilePath,
                        ProtectedData.Protect(Serialize(),
                            null,
                            DataProtectionScope.CurrentUser));
                    HasStateChanged = false;
                }
            }
        }
    }
}