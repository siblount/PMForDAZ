using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Database
{
    public interface IDPDatabase
    {
        /// <summary>
        /// Search asynchroniously does a database search based on a user search query (ex: "hello world"). Do not 
        /// use this function for regex expressions; Instead use <c>RegexSearch</c>. This function does not return
        ///  the search results but instead returns it to the callback and/or an event. If you wish to retrieve 
        /// the values through an event, use a caller ID to identify the event is for that particular class at that
        /// time.
        /// </summary>
        /// <param name="searchQuery">A user search query.</param>
        /// <param name="sortMethod">The sorting method to apply for results.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The results of the search query. </returns>
        Task<DPProductRecord[]> SearchQ(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None, uint callerID = 0, Action<DPProductRecord[]>? callback = null);
        /// <summary>
        /// RegexSearch asynchroniously does a database search based on a user regex pattern (ex: "[^abc]").
        /// This function does not return the search results but instead returns it to the callback and/or an event. 
        /// If you wish to retrieve the values through an event, use a caller ID to identify the event is for that particular
        /// class at that time.
        /// </summary>
        /// <param name="regex">A regex expression.</param>
        /// <param name="sortMethod">The sorting method to apply for results.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The results of the search query. </returns> 
        Task<DPProductRecord[]> RegexSearchQ(string regex, DPSortMethod sortMethod = DPSortMethod.None, uint callerID = 0, Action<DPProductRecord[]>? callback = null);
        /// <summary>
        /// Gets product records on the page specified by <paramref name="page"/>. The page is determined by the 
        /// limit of <paramref name="limit"/>. This means that if there are 50 records, and the limit is 10, the max
        /// amount of pages is 5. This function does not return any results but will return the results via the callback
        /// and via MainQueryCompleted event.
        /// </summary>
        /// <param name="sortMethod">The sorting method to apply to the results.</param>
        /// <param name="page">The page to get product records from.</param>
        /// <param name="limit">The max amount of product records per page; the max amount of records to receive.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The results of the search query. </returns> 
        Task<DPProductRecord[]> GetProductRecordsQ(DPSortMethod sortMethod, uint page = 1, uint limit = 0, uint callerID = 0, Action<DPProductRecord[]>? callback = null);
        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, and delete queries.
        /// </summary>
        void StopMainDatabaseOperations();
        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, delete, and get queries. 
        /// This also stops pending search queries and "view" queries.
        /// </summary>
        void StopAllDatabaseOperations();
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
        /// Queries/Adds a new product record to the database.
        /// </summary>
        /// <param name="pRecord">The new product record to add.</param>
        Task AddNewRecordEntry(DPProductRecord pRecord);
        /// <summary>
        /// Queries/Adds a new product and extraction record to the database.
        /// </summary>
        /// <param name="pRecord">The new product record to add.</param>
        /// <param name="eRecord">The new extraction record to add.</param>
        Task AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord);
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
        Task RemoveProductRecord(DPProductRecord record, Action<uint>? callback = null);
        /// <summary>
        /// Removes all the values from the table. Triggers in the database are temporarly disabled for deleting.
        /// </summary>
        /// <param name="tableName"></param>
        Task ClearTableQ(string tableName);
        
        /// <summary>
        /// Not fully implemented. Do not use.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <param name="columns"></param>
        Task UpdateValuesQ(string tableName, object[] values, string[] columns, int id);
        /// <summary>
        /// Updates a product record and extraction record. This is currently used for applying changes from the product
        /// record form.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newProductRecord"></param>
        /// <param name="newExtractionRecord"></param>
        Task UpdateRecordQ(uint id, DPProductRecord newProductRecord, DPExtractionRecord newExtractionRecord, Action<uint>? callback = null);
        /// <summary>
        /// Removes all product records (and corresonding extraction records) from the database that contain a tag
        /// specified in tags. Basically, for every record in product records, if the product record contains ANY 
        /// of the tags specified in <paramref name="tags"/>, it is removed from the database.
        /// </summary>
        /// <param name="tags">Product records' tags to specify for deletion.</param>
        Task RemoveProductRecordsViaTagsQ(string[] tags);
        /// <summary>
        /// Removes all product records that satisfy the condition specified by <paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The prerequisite for removing a row that must be met.</param>
        Task RemoveProductRecordsQ(Tuple<string, object> condition);
        /// <summary>
        /// Removes all product records that satisfy the conditions specified by <paramref name="conditions"/>.
        /// </summary>
        /// <param name="conditions">The prerequisites for removing a row that must be met.</param>
        Task RemoveProductRecordsQ(Tuple<string, object>[] conditions);
        /// <summary>
        /// Removes all rows that satisfy the condition specified by <paramref name="condition"/>.
        /// </summary>
        /// <param name="tableName">The table to remove rows from.</param>
        /// <param name="condition">The prerequisite for removing a row that must be met.</param>
        Task RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition);
        /// <summary>
        /// Removes all rows that satisfy the conditions specified by <paramref name="conditions"/>.
        /// </summary>
        /// <param name="tableName">The table to remove rows from.</param>
        /// <param name="conditions">The prerequisite for removing a row that must be met.</param>
        Task RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions);
        /// <summary>
        /// Removes all product and extraction records from the database.
        /// </summary>
        Task RemoveAllRecordsQ();
        /// <summary>
        /// Removes all tags associated with the product ID specified by <paramref name="pid"/> from the database.
        /// </summary>
        /// <param name="pid"></param>
        Task RemoveTagsQ(uint pid);
        /// <summary>
        /// Gets the extraction records associated with the extraction record ID specified by <paramref name="eid"/> 
        /// from the database. This function returns the records via the callback function or via the RecordQueryCompleted
        /// event. This may return null if the record does not exist in the database or an internal error occurred.
        /// </summary>
        /// <param name="eid">The extraction record ID to get.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The extraction record, if successfully fetched. Otherwise, null. </returns>
        Task<DPExtractionRecord?> GetExtractionRecordQ(uint eid, uint callerID = 0, Action<DPExtractionRecord>? callback = null);
        /// <summary>
        /// Updates the <c>ArchiveFileNames</c> variable and returns it via callback function.
        /// It returns a unique set of installed archive names.
        /// </summary>
        /// <param name="callback">The function to return values to.</param>
        /// <returns> The archive file names. </returns>
        Task<HashSet<string>> GetInstalledArchiveNamesQ(Action<HashSet<string>>? callback = null);
    }
}