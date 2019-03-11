using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlDownloaderLib.Enums;
using HtmlDownloaderLib.Interfaces;
using HtmlDownloaderLib.Services;

namespace HtmlDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var pageToDownload = @"https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo?view=netframework-4.7.2";
            var maximalDepth = 1;
            var localDirectoryPath = @"d:\000";

            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            ISaver contentSaver = new HtmlLocalSaver(localDirectoryPath);

            IFileExtensionConstraint fileExtensionConstraints = new FileExtensionConstraint("png,jpg");
            ITransactionConstraint transactionConstraint = new TransactionConstraint(TransactionConstraints.CurrentDomainOnly);

            IDownloader downloader = new HtmlDownloaderLib.Services.HtmlDownloader(contentSaver, 
                fileExtensionConstraints,
                transactionConstraint,
                pageToDownload,
                maximalDepth,
                cancelToken);

            try
            {
                downloader.LoadUrl();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task has been canceled: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
