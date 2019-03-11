using System;
using HtmlDownloaderLib.Interfaces;
using HtmlDownloaderLib.Enums;

namespace HtmlDownloaderLib.Services
{
    /// <summary>
    /// Represents a class providing verification of the transaction to the page
    /// </summary>
    public class TransactionConstraint : ITransactionConstraint
    {
        private TransactionConstraints _currentTransactionConstrait;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionConstraint"/> class
        /// </summary>
        /// <param name="constarit"></param>
        public TransactionConstraint(TransactionConstraints constarit)
        {
            _currentTransactionConstrait = constarit;
        }

        /// <summary>
        /// Checks if transction from primary uri to the current uri is allowable 
        /// </summary>
        /// <param name="currentUri">Current uri</param>
        /// <param name="primaryUri">Primary uri</param>
        /// <returns>True if transaction is allowable, false otherwise</returns>
        public bool IsAcceptableUrl(Uri currentUri, Uri primaryUri)
        {
            if (_currentTransactionConstrait == TransactionConstraints.WithoutConstraints)
            {
                return true;
            }

            if (_currentTransactionConstrait == TransactionConstraints.CurrentDomainOnly && currentUri.DnsSafeHost == primaryUri.DnsSafeHost)
            {
                return true;
            }

            if (_currentTransactionConstrait == TransactionConstraints.DescendingPagesOnly && primaryUri.IsBaseOf(currentUri))
            {
                return true;
            }

            return false;
        }
    }
}
