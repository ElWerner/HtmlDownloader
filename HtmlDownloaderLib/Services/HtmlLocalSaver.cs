using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlDownloaderLib.Interfaces;
using HtmlAgilityPack;

namespace HtmlDownloaderLib.Services
{
    /// <summary>
    /// Represent a class providing save html page or content to the local path
    /// </summary>
    public class HtmlLocalSaver : ISaver
    {
        private readonly DirectoryInfo _localDirectoryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlLocalSaver"/> class
        /// </summary>
        /// <param name="localDirectory">Path to the local directory</param>
        public HtmlLocalSaver(string localDirectory)
        {
            if (string.IsNullOrEmpty(localDirectory))
            {
                throw new ArgumentNullException($"{nameof(localDirectory)} cannot be null or emptry.");
            }

            _localDirectoryPath = new DirectoryInfo(localDirectory);
        }

        /// <summary>
        /// Saves html page from htmlDocument to the local directory
        /// </summary>
        /// <param name="uri">Page uri</param>
        /// <param name="document">Html Document with html page</param>
        /// <returns>Task</returns>
        public async Task SaveHtmlPageAsync(Uri uri, HtmlDocument document)
        {
            var stream = new MemoryStream();
            document.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string directoryPath = GetDirectoryPath(uri);

            Directory.CreateDirectory(directoryPath);

            var name = GetValidFileName(document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html");
            string fileFullPath = Path.Combine(directoryPath, name);

            await SaveToFile(stream, fileFullPath);

            stream.Close();
        }

        /// <summary>
        /// Saves file from stream to the local path
        /// </summary>
        /// <param name="uri">File uri</param>
        /// <param name="fileStream">Stream with file</param>
        /// <returns></returns>
        public async Task SaveFileAsync(Uri uri, Stream fileStream)
        {
            string fileFullPath = GetDirectoryPath(uri);
            var directoryPath = Path.GetDirectoryName(fileFullPath);

            await SaveToFile(fileStream, fileFullPath);

            fileStream.Close();
        }

        /// <summary>
        /// Saves content of the stream to the local path
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="filePath">Path to the new file</param>
        /// <returns>Task</returns>
        private async Task SaveToFile(Stream stream, string filePath)
        {
            var currentStream = File.Create(filePath);

            await stream.CopyToAsync(currentStream);

            currentStream.Close();
        }

        /// <summary>
        /// Gets path to the file
        /// </summary>
        /// <param name="directory">Local directory</param>
        /// <param name="uri"></param>
        /// <returns>Path to the new directory</returns>
        private string GetDirectoryPath(Uri uri)
        {
            return Path.Combine(_localDirectoryPath.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
        }

        /// <summary>
        /// Gets file name without invalid chars 
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns></returns>
        private string GetValidFileName(string fileName)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

            return r.Replace(fileName, "");
        }
    }
}
