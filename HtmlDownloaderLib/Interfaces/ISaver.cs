using HtmlAgilityPack;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HtmlDownloaderLib.Interfaces
{
    public interface ISaver
    {
        Task SaveFileAsync(Uri uri, Stream fileStream);

        Task SaveHtmlPageAsync(Uri uri, HtmlDocument document);
    }
}