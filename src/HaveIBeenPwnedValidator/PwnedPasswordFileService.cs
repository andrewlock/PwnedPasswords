using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace HaveIBeenPwnedValidator
{
    /// <summary>
    /// Check file lists to verify whether the password has been pwned
    /// If the password exists in a password list, the password has been pwned
    /// </summary>
    public class PwnedPasswordFileService : IPwnedPasswordService
    {
        private readonly PwnedPasswordFileServiceOptions _options;
        private readonly ILogger _logger;
        private readonly IFileProvider _fileProvider;
        private readonly ICollection<IFileInfo> _files;

        public PwnedPasswordFileService(
            ILogger<PwnedPasswordFileService> logger,
            PwnedPasswordFileServiceOptions options,
            IHostingEnvironment environment)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (environment == null) { throw new ArgumentNullException(nameof(environment)); }
            if (environment.ContentRootFileProvider == null) { throw new ArgumentNullException($"{nameof(environment)}.{nameof(environment.ContentRootFileProvider)}"); }
            if ((_options.Filenames?.Length ?? 0) == 0) throw new ArgumentException($"No files provided in {nameof(options)}.{nameof(options.Filenames)}");
            _fileProvider = environment?.ContentRootFileProvider;
            _files = GetFileInfos();
        }

        private ICollection<IFileInfo> GetFileInfos() => _options.Filenames
                .Select(filename => _fileProvider.GetFileInfo(filename))
                .ToList();


        /// <inheritdoc />
        public Task<bool> HasPasswordBeenPwned(string password)
        {
            var sha1Password = SHA1Util.SHA1HashStringForUTF8String(password);
            foreach (var file in _files)
            {
                var isPwned = IsInFile(sha1Password, file);
                if (isPwned)
                {
                    //stop as soon as we find it!
                    return Task.FromResult(true);
                }
            }

            // checked all files, must be ok!
            return Task.FromResult(false);
        }

        private bool IsInFile(string sha1Password, IFileInfo fileInfo)
        {
            _logger.LogDebug("Checking for pwned passwords in file {FilePath}", fileInfo.PhysicalPath);

            // Loop through every file and check every line
            // Unfortunately the files will be too large to read into memory entirely
            using (var stream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.Equals(sha1Password, line, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
