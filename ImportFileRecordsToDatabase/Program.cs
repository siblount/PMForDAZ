using System.Collections.Generic;
using System.IO;
using System.Text;

public class Program
{

    private record struct OldDPProductRecord(string productName, string[] tags, 
        string[] directories, DateTime timeExtracted, string[] filesExtracted,
        string expectedExtractionRecordLocation, string expectedImageLocation, int id)
    {

    }

    private record struct OldDPExtractionRecord(string archiveName, string[] files, DateTime timeExtracted, string[] erroredFiles, int id)
    {

    }

    private static string fileRecordsLocation, databaseLocation;
    private static string[] precFiles, erecFiles;
    private static List<OldDPProductRecord> productRecords;
    private static Dictionary<string, OldDPExtractionRecord> extractionRecords;
    private static bool FindFileRecordsLocation()
    {
        var curDir = Directory.GetCurrentDirectory();
        var folders = Directory.GetDirectories(curDir);
        foreach (var folder in folders)
        {
            if (precFiles == null || precFiles?.Length == 0) precFiles = Directory.GetFiles(folder, "*.prec");
            if (erecFiles == null || erecFiles?.Length == 0) erecFiles = Directory.GetFiles(folder, "*.erec");

            if (erecFiles.Length != 0 && precFiles.Length != 0)
            {
                fileRecordsLocation = folder;
                return true;
            }
        }
        return false;
        
    }

    static private string[] HandleSeperators(string msg)
    {
        var msgLines = msg.Split("|.|");
        for (var i = 0; i < msgLines.Length; i++)
        {
            msgLines[i] = msgLines[i].Trim();
        }
        return msgLines;
    }


    static private void ProcessPREC(string filePath)
    {
        // We know filePath exists so no need to check twice.
        // Open file as texteditor, get all text since it should most definitely be less than 64KB. Otherwise, the universe is fucking with me.
        try
        {
            var text = File.ReadAllText(filePath);
            var lines = text.Split('\n');
            var productName = lines[0].Trim();
            var tags = HandleSeperators(lines[1].Trim());
            var directories = HandleSeperators(lines[2].Trim());
            var filesExtracted = HandleSeperators(lines[3].Trim());
            DateTime timeExtracted = DateTime.Parse(lines[4].Trim());
            var expectedExtractionRecordLocation = lines[5].Trim();
            if (expectedExtractionRecordLocation == "NULL") expectedExtractionRecordLocation = null;
            else expectedExtractionRecordLocation = Path.GetRelativePath(fileRecordsLocation, expectedExtractionRecordLocation);
            var expectedImageLocation = lines[6].Trim();
            if (expectedImageLocation == "NULL") expectedImageLocation = null;
            var id = int.Parse(lines[7].Trim());

            var workingPREC = new OldDPProductRecord(productName, tags, directories, timeExtracted, 
                filesExtracted, expectedExtractionRecordLocation, expectedImageLocation, id);
            productRecords.Add(workingPREC);
        }
        catch (Exception e)
        {
            Print($"Unable to successfully process product record. REASON: {e}");
        }
    }

    // EREC Format
    // Archive File Name: string
    // Files Location: string[0] or ... | seperated by |.| \n
    // Time extracted: datetime
    // Errors : int
    // Files Extracted : int
    // Missing Files: string[0] or .. | seperated by |.| \n
    // Errored Files: string[0] or .. | seperated by |.| \n
    // Archive Type : ArchiveType as int.
    // ID

    // Used for reading.
    static private void ProcessEREC(string filePath)
    {
        try
        {
            var text = File.ReadAllText(filePath);
            var lines = text.Split('\n');
            var archiveName = lines[0].Trim();
            var extractedFilesLocations = HandleSeperators(lines[1].Trim());
            DateTime timeExtracted = DateTime.Parse(lines[2].Trim());
            var erroredFiles = HandleSeperators(lines[6].Trim());
            var id = int.Parse(lines[8].Trim());
            var record = new OldDPExtractionRecord(archiveName, extractedFilesLocations,
                timeExtracted, erroredFiles, id);
            extractionRecords[Path.GetRelativePath(fileRecordsLocation, filePath)] = record;

        }
        catch (Exception e)
        {
            Print($"Unable to successfully process extraction record. REASON: {e}");
        }
    }

    public static void Print(params object[] args)
    {
        string msg = string.Join(" ", args);
        Console.WriteLine(msg);
    }
    
    private static void BuildRecords()
    {
        var r = Parallel.ForEach(precFiles, rec => ProcessPREC(rec));
        var r1 = Parallel.ForEach(erecFiles, rec => ProcessEREC(rec));
        SpinWait.SpinUntil(() => r1.IsCompleted && r.IsCompleted);
    }

    private static string JoinString(string seperator, params string[] values)
    {
        if (values == null || values.Length == 0) return string.Empty;

        StringBuilder builder = new StringBuilder(512);
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == null) continue;
            if (values[i].Trim() != string.Empty)
            {
                builder.Append(values[i] + seperator);
            }
        }
        try
        {
            builder.Remove(builder.Length - 1 - seperator.Length, seperator.Length);
        }
        catch (Exception e) { }
        return builder.ToString().Trim();
    }

    private static void AddToValues(OldDPProductRecord record)
    {
        OldDPExtractionRecord extractionRecord;
        record.Deconstruct(out string productName, out string[] tags, out string[] dirs, out DateTime time, out string[] files, out string expectedERecordLocation, out string thumbnailLocation, out var _);
        object[] pVals, eVals;
        const string n = null;
        if (extractionRecords.TryGetValue(Path.GetRelativePath(fileRecordsLocation, record.expectedExtractionRecordLocation), out extractionRecord))
        {
            extractionRecord.Deconstruct(out string archiveName, out string[] filesE, out var _, out _, out _);
            tags ??= Array.Empty<string>();
            var pVals2 = new object[] { productName, JoinString(", ", tags), null, null, time.ToFileTime(), thumbnailLocation };
            var eVals2 = new object[] { archiveName, JoinString(", ", filesE), JoinString(", ", dirs), "D:/3D/My DAZ 3D Library", null, null};
            pVals = new object[] { productName, JoinString(", ", tags), string.Empty, time.ToFileTime(), thumbnailLocation ??= "" };
            eVals = new object[] { JoinString(", ", filesE), JoinString(", ", dirs), "D:/3D/My DAZ 3D Library", archiveName };
            Database.QueueInsertion(pVals2, eVals2, tags);
        }
    }
    
    public static void Main(string[] args)
    {
        Print(FindFileRecordsLocation());
        while (true)
        {
            Console.Write("Enter database location (or type 'q' to quit): ");
            var databasePath = Console.ReadLine();
            if (databasePath == "q") return;
            if (databasePath.StartsWith('"') && databasePath.EndsWith('"'))
            {
                databasePath = databasePath.Substring(1, databasePath.Length - 2);
            }
            if (File.Exists(databasePath))
            {
                databaseLocation = databasePath;
                break;
            } else
            {
                Console.WriteLine("Given location does not exist. Try again.");
            }
        }

        productRecords = new List<OldDPProductRecord>(precFiles.Length);
        extractionRecords = new Dictionary<string, OldDPExtractionRecord>(erecFiles.Length);
        BuildRecords();
        Print(productRecords.Count);
        Database.Initialize(databaseLocation);

        foreach (var rec in productRecords)
        {
            AddToValues(rec);
        }
        //productRecords.ForEach(rec => AddToValues(rec));
        Print("All done!");

    }
}