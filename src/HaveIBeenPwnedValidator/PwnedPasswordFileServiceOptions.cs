using Microsoft.AspNetCore.Hosting;

namespace HaveIBeenPwnedValidator
{
    /// <summary>
    /// Options for the <see cref="PwnedPasswordFileService"/>
    /// </summary>
    public class PwnedPasswordFileServiceOptions
    {
        /// <summary>
        /// The password filenames relative to the <see cref="IHostingEnvironment.ContentRootPath"/> to load pwned passwords from. 
        /// The format is a single SHA1 password per line
        /// </summary>
        public string[] Filenames { get; set; }
    }
}
