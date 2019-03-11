using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using HtmlDownloaderLib.Interfaces;
using System.Threading.Tasks;
using System.Threading;

namespace HtmlDownloaderLib.Services
{
    /// <summary>
    /// Represents a class providing download of the html pages
    /// </summary>
    public class HtmlDownloader : IDownloader
    {
        private readonly ISaver _contentSaver;

        private readonly IFileExtensionConstraint _fileExtensionConstraints;
        private readonly ITransactionConstraint _transactionConstraints;

        private readonly Uri _primaryUri;

        private readonly CancellationToken _token;

        private readonly int _maxDepthLevel;

        private IList<Uri> _visitedUrls = new List<Uri>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlDownloader"/> class
        /// </summary>
        /// <param name="contentSaver">Content saver</param>
        /// <param name="fileExtensionConstrain">File extension constraints</param>
        /// <param name="transactionConstraints">Transaction constraints</param>
        /// <param name="pageUrl">Url of the page</param>
        /// <param name="depthLevel">Maximal level of transactions</param>
        /// <param name="token">Cancellation token</param>
        public HtmlDownloader(ISaver contentSaver, 
            IFileExtensionConstraint fileExtensionConstrain, 
            ITransactionConstraint transactionConstraints, 
            string pageUrl, 
            int depthLevel,
            CancellationToken token)
        {
            if (depthLevel < 0)
            {
                throw new ArgumentException($"{nameof(depthLevel)} cannot be less than zero.");
            }

            if (string.IsNullOrEmpty(pageUrl))
            {
                throw new ArgumentException($"{nameof(pageUrl)} cannot be null or emptry.");
            }

            _transactionConstraints = transactionConstraints ?? throw new ArgumentNullException($"{nameof(transactionConstraints)} cannot be null.");
            _fileExtensionConstraints = fileExtensionConstrain ?? throw new ArgumentNullException($"{nameof(fileExtensionConstrain)} cannot be null.");
            _contentSaver = contentSaver ?? throw new ArgumentNullException($"{nameof(contentSaver)} cannot be null.");

            _token = token;

            _maxDepthLevel = depthLevel;

            _primaryUri = new Uri(pageUrl);
        }

        /// <summary>
        /// Loads primary html page
        /// </summary>
        /// <returns>Task</returns>
        public async Task LoadUrl()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = _primaryUri;
                await ScanUrlAsync(httpClient, httpClient.BaseAddress, 0);
            }
        }

        /// <summary>
        /// Defines content of the current uri address
        /// </summary>
        /// <param name="httpClient">Http client</param>
        /// <param name="uri">Page uri</param>
        /// <param name="depth">Current depth of transactions</param>
        /// <returns>Task</returns>
        private async Task ScanUrlAsync(HttpClient httpClient, Uri uri, int depth)
        {
            if (depth > _maxDepthLevel || _visitedUrls.Contains(uri) || !(uri.Scheme == "http" || uri.Scheme == "https"))
            {
                return;
            }

            _visitedUrls.Add(uri);

            try
            {
                var responseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri), _token);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return;
                }

                if (responseMessage.Content.Headers.ContentType?.MediaType == "text/html")
                {
                    await ProcessHtmlDocumentAsync(httpClient, uri, depth);
                }
                else
                {
                    await ProcessFileAsync(httpClient, uri);
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Process html page at the current uri
        /// </summary>
        /// <param name="httpClient">Http Client</param>
        /// <param name="pageUri">Html page uri</param>
        /// <param name="depth">Current depth of transactions</param>
        /// <returns>Task</returns>
        private async Task ProcessHtmlDocumentAsync(HttpClient httpClient, Uri pageUri, int depth)
        {
            try
            {
                if (!_transactionConstraints.IsAcceptableUrl(pageUri, _primaryUri))
                {
                    return;
                }

                HttpResponseMessage response = await httpClient.GetAsync(pageUri, _token);

                var document = new HtmlDocument();

                document.Load(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8);

                await _contentSaver.SaveHtmlPageAsync(pageUri, document);

                var attributesWithLinks = document.DocumentNode.Descendants()
                    .SelectMany(d => d.Attributes.Where(a => (a.Name == "src" || a.Name == "href")));

                foreach (var attributesWithLink in attributesWithLinks)
                {
                    await ScanUrlAsync(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), depth + 1);
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Process file at the current uri
        /// </summary>
        /// <param name="httpClient">Http Client</param>
        /// <param name="fileUri">File uri</param>
        /// <returns>Task</returns>
        private async Task ProcessFileAsync(HttpClient httpClient, Uri fileUri)
        {
            if (!_transactionConstraints.IsAcceptableUrl(fileUri, _primaryUri) || !_fileExtensionConstraints.IsAcceptableFileType(fileUri))
            {
                return;
            }

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(fileUri, _token);

                await _contentSaver.SaveFileAsync(fileUri, response.Content.ReadAsStreamAsync().Result);
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
