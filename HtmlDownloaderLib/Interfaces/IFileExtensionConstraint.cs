using System;

namespace HtmlDownloaderLib.Interfaces
{
    public interface IFileExtensionConstraint
    {
        bool IsAcceptableFileType(Uri uri);
    }
}
