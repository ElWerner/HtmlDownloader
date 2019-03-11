using System;

namespace HtmlDownloaderLib.Interfaces
{
    public interface ITransactionConstraint
    {
        bool IsAcceptableUrl(Uri currentUri, Uri primaryUri);
    }
}
