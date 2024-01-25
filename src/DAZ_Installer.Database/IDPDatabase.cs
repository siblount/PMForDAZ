using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Database
{
    /// <summary>
    /// The interface for the database. This is used for other classes to get data from the database.
    /// All methods are executed asynchronously.
    /// </summary>
    public interface IDPDatabase
    {
        /// <summary>
        /// Search asynchroniously does a database search based on a user search query (ex: "hello world"). If you wish to retrieve 
        /// the values through an event, use a caller ID to identify the event is for that particular class at that
        /// time.
        /// </summary>
        /// <param name="searchQuery">A user search query.</param>
        /// <param name="sortMethod">The sorting method to apply for results.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The results of the search query. </returns>
        Task<List<DPProductRecordLite>> SearchQ(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None, uint callerID = 0, Action<List<DPProductRecordLite>>? callback = null);
        /// <summary>
        /// Gets product records on the page specified by <paramref name="page"/>. The page is determined by the 
        /// limit of <paramref name="limit"/>. This means that if there are 50 records, and the limit is 10, the max
        /// amount of pages is 5.
        /// </summary>
        /// <param name="sortMethod">The sorting method to apply to the results.</param>
        /// <param name="page">The page to get product records from.</param>
        /// <param name="limit">The max amount of product records per page; the max amount of records to receive.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The results of the search query. </returns> 
        Task<List<DPProductRecordLite>> GetProductRecordsQ(DPSortMethod sortMethod, uint page = 1, uint limit = 0, uint callerID = 0, Action<List<DPProductRecordLite>>? callback = null);
        /// <summary>
        /// Gets a product record by it's ID. This returns a full product record. 
        /// </summary>
        /// <param name="id">The ID of the product record to fetch.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns>The full product record.</returns>
        Task<DPProductRecord?> GetFullProductRecord(long id, Action<DPProductRecord?>? callback = null);
        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, and delete queries.
        /// </summary>
        void StopMainDatabaseOperations();
        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, delete, and get queries. 
        /// This also stops pending search queries and "view" queries.
        /// </summary>
        /// <param name="wait">Whether to block current thread until it is done.</param>
        void StopAllDatabaseOperations(bool wait);
        /// <summary>
        /// If `forceRefresh` is false, the refresh action will be queued. Otherwise, the action queue will be cleared and database will be refreshed immediately.
        /// </summary>
        /// <param name="forceRefresh">Refreshes immediately if True, otherwise it is queued.</param>
        Task RefreshDatabaseQ(bool forceRefresh = false);
        /// <summary>
        /// Returns all the values from the table specified by the <paramref name="tableName"/> table via the 
        /// callback or via ViewUpdated event. This may return null.
        /// </summary>
        /// <param name="tableName">The table name to </param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return the dataset to.</param>
        /// <returns> The table, if successfully fetched. Otherwise, null. </returns> 
        Task<DataSet?> ViewTableQ(string tableName, uint callerID = 0, Action<DataSet?>? callback = null);
        /// <summary>
        /// Adds a new product record to the database.
        /// </summary>
        /// <param name="pRecord">The new product record to add.</param>
        Task AddNewRecordEntry(DPProductRecord pRecord);
        /// <summary>
        /// Inserts a new row to the table specified by <paramref name="tableName">. It requires the columns that
        /// new values will be inserted into. Columns and values length must match.
        /// For example, if you want to insert a new row to ProductRecors but only want to insert the name for now,
        /// columns would be <c>{"Name"}</c> and values would be <c>{"poppy stick"}</c>. Columns do not have to match
        /// the columns in the database, but should include the required, non-null columns.
        /// </summary>
        /// <param name="tableName">The table to insert values into.</param>
        /// <param name="values">The values to insert.</param>
        /// <param name="columns">The corresponding columns to insert values to.</param>
        Task InsertNewRowQ(string tableName, object[] values, string[] columns);
        /// <summary>
        /// Removes a row by it's ID at the table specifed by <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">The table to insert values into.</param>
        /// <param name="id">The ID of the row to remove.</param>
        Task RemoveRowQ(string tableName, int id);
        /// <summary>
        /// Removes a product record from the database. This also removes the corresponding extraction record.
        /// </summary>
        /// <param name="record">The record to delete.</param>
        /// <param name="callback">The function to callback when the product record was removed.</param>
        /// <returns></returns>
        Task RemoveProductRecordQ(DPProductRecord record, Action<long>? callback = null);
        /// <inheritdoc cref="RemoveProductRecordQ(DPProductRecord, Action{long}?)"/>
        Task RemoveProductRecordQ(DPProductRecordLite record, Action<long>? callback = null);
        /// <summary>
        /// Removes all the values from the table. Triggers in the database are temporarly disabled for deleting.
        /// </summary>
        /// <param name="tableName"></param>
        Task ClearTableQ(string tableName);
        /// <summary>
        /// Updates a product record and extraction record. This is currently used for applying changes from the product
        /// record form.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newProductRecord"></param>
        Task UpdateRecordQ(long id, DPProductRecord newProductRecord, Action<long>? callback = null);
        /// <summary>
        /// Removes all product and extraction records from the database.
        /// </summary>
        Task RemoveAllRecordsQ();
        /// <summary>
        /// Updates the <c>ArchiveFileNames</c> variable and returns it via callback function.
        /// It returns a unique set of installed archive names.
        /// </summary>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The archive file names. </returns>
        Task<HashSet<string>> GetInstalledArchiveNamesQ(Action<HashSet<string>>? callback = null);
    }
}