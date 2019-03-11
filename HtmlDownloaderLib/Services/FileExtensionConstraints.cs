using System;
using System.Collections.Generic;
using System.Linq;
using HtmlDownloaderLib.Interfaces;

namespace HtmlDownloaderLib.Services
{
    /// <summary>
    /// Represents a class providing verification of the file extension
    /// </summary>
    public class FileExtensionConstraint : IFileExtensionConstraint
    {
        private List<string> _acceptableFileExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExtensionConstraint"/> class
        /// </summary>
        /// <param name="fileExtensionsString">String that contains allowable file extensions</param>
        public FileExtensionConstraint(string fileExtensionsString)
        {
            _acceptableFileExtensions = new List<string>();

            GetFileExtensionsFromString(fileExtensionsString);
        }

        /// <summary>
        /// Checks if current file has allowable extension
        /// </summary>
        /// <param name="uri">File uri</param>
        /// <returns>True if current file uri has allowable extension, false otherwise</returns>
        public bool IsAcceptableFileType(Uri uri)
        {
            var fileExtension = uri.Segments.Last();

            return _acceptableFileExtensions.Any(e => fileExtension.EndsWith(e));
        }

        /// <summary>
        /// Converts string with file extensions to list
        /// </summary>
        /// <param name="fileExtensionsString"></param>
        private void GetFileExtensionsFromString(string fileExtensionsString)
        {
            var extensions = fileExtensionsString.Split(',');

            foreach (var extension in extensions)
            {
                _acceptableFileExtensions.Add("." + extension);
            }
        }
    }
}
