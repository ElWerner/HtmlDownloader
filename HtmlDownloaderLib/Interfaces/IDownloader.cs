using System.Threading;
using System.Threading.Tasks;

namespace HtmlDownloaderLib.Interfaces
{
    public interface IDownloader
    {
        Task LoadUrl();
    }
}