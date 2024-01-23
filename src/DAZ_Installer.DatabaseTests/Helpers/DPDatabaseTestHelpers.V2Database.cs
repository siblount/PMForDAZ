using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Serilog;
using System.Data;
using Microsoft.Data.Sqlite;
namespace DAZ_Installer.Database.Tests
{
    internal static partial class DPDatabaseTestHelpers
    {
        internal static void CreateV2Database(SqliteConnection c)
        {
            const string txt = @"
            CREATE TABLE ""ProductRecords"" (

                ""ID""    INTEGER NOT NULL UNIQUE,

                ""Product Name""  TEXT NOT NULL,
	            ""Tags""  TEXT,
	            ""Author""    TEXT,
	            ""SKU""   TEXT,
                ""Date Created"" INTEGER,
                ""Extraction Record ID""  INTEGER UNIQUE,
                ""Thumbnail Full Path""	TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );
            CREATE TABLE ""ExtractionRecords"" (

                ""ID""    INTEGER NOT NULL UNIQUE,

	            ""Files"" TEXT,
                ""Folders"" TEXT,
	            ""Destination Path""  TEXT,
                ""Errored Files""   TEXT,
	            ""Error Messages""    TEXT,
	            ""Archive Name""  TEXT,
	            ""Product Record ID"" INTEGER UNIQUE,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );
            CREATE TABLE ""DatabaseInfo"" (

                ""Version""   INTEGER NOT NULL DEFAULT 2,
	            ""Product Record Count""  INTEGER NOT NULL DEFAULT 0,
	            ""Extraction Record Count""   INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE ""Tags"" (
	            ""ID""	INTEGER,
	            ""Tag""	TEXT NOT NULL COLLATE NOCASE,
	            PRIMARY KEY(""ID"",""Tag"")
            );
            CREATE TABLE ""CachedSearches"" (

                ""Search String"" TEXT NOT NULL UNIQUE,
                ""Result Product IDs""    TEXT,
	            PRIMARY KEY(""Search String"")
            );
            CREATE INDEX ""idx_DateCreatedToPID"" ON ""ProductRecords"" (
                ""Date Created"" ASC,
                ""ID""	            
            );
            CREATE INDEX ""idx_PIDtoTag"" ON ""Tags"" (
                ""Product Record ID"" ASC,
                ""Tag""	COLLATE NOCASE
            );
            CREATE INDEX ""idx_ProductNameToPID"" ON ""ProductRecords"" (
                ""Product Name"" ASC,
                ""ID""	            
            );
            CREATE INDEX ""idx_TagToPID"" ON ""Tags"" (
                ""Tag""   COLLATE NOCASE ASC,
	            ""Product Record ID""
            );
";
            using var command = c.CreateCommand();
            command.CommandText = txt;
            command.ExecuteNonQuery();
        }
        internal static void FinishV2Database(SqliteConnection c)
        {
            var txt = @"
            CREATE TRIGGER delete_on_extraction_removal
                            AFTER DELETE ON ExtractionRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NULL WHERE ""Extraction Record ID"" = old.ID;
                        END;
            CREATE TRIGGER delete_on_product_removal
                            AFTER DELETE ON ProductRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                            DELETE FROM ExtractionRecords WHERE ID = old.""Extraction Record ID"";
                            DELETE FROM TAGS WHERE ""Product Record ID"" = old.ID;
                        END;
            CREATE TRIGGER update_on_extraction_add
	                        AFTER INSERT ON ExtractionRecords FOR EACH ROW
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ExtractionRecords SET ""Product Record ID"" = (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1) WHERE ID = NEW.ID;
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NEW.ID WHERE ID IN (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1);
                        END;
            CREATE TRIGGER update_product_count
	                        AFTER INSERT ON ProductRecords
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                        END;";
            using var command = c.CreateCommand();
            command.CommandText = txt;
            command.ExecuteNonQuery();
        }

        internal static void AssertOldSchemaRemoved(SqliteConnection c)
        {
            var query = @"SELECT name FROM sqlite_master WHERE type=""trigger"" OR type=""table"" OR type=""index"";";
            using var command = c.CreateCommand();
            command.CommandText = query;
            using var reader = command.ExecuteReader();
            var names = new List<string>(reader.FieldCount);
            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }
            CollectionAssert.DoesNotContain(names, "ProductRecords", "ProductRecords still exist");
            CollectionAssert.DoesNotContain(names, "ExtractionRecords", "ExtractionRecords still exist");
            CollectionAssert.DoesNotContain(names, "Tags", "Tags still exist");
            CollectionAssert.DoesNotContain(names, "CachedSearches", "CachedSearches still exist");
            CollectionAssert.DoesNotContain(names, "update_on_extraction_add", "update_on_extraction_add still exists");
            CollectionAssert.DoesNotContain(names, "delete_on_extraction_removal", "delete_on_extraction_removal still exists");
            CollectionAssert.DoesNotContain(names, "idx_ProductNameToPID", "idx_ProductNameToPID still exists");
            CollectionAssert.DoesNotContain(names, "idx_PIDToTag", "idx_PIDToTag still exists");
            CollectionAssert.DoesNotContain(names, "idx_TagToPID", "idx_TagToPID still exists");
            CollectionAssert.DoesNotContain(names, "idx_DateCreatedToPID", "idx_DateCreatedToPID still exists");
        }
    }
}
