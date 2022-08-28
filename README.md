# DAZ Product Installer

This is an application currently only for Windows users to install their products regardless of the vendor's format; it accepts unique file structurs and archive extensions.

<p align="center">
[![Daz Product Installer Alpha Preview Video](https://i.postimg.cc/RhBnRmF3/image.png)](https://www.youtube.com/watch?v=FwLc-dcl8W0)
</p>

## What does this project aim to do?
Firstly, it aims to be a **free** and **open-source*** application to gain trust of users, allow others to contribute, and release a product that is useful for all.
Additonally, it - 
 - Make an application that is able to process any archive (ZIP, RAR, 7z).
 - Quickly install products regardless of file structures. **This is very useful for Renderosity and other vendor websites**.
 - Also be a library to store processed/installed products.
 - Be a database to search for products for users who have a lot of products.
 - Be a product management tool for products that are from Renderosity and other vendors (For example).


## Project Status
Alpha

Currently, the application can:
  - ✅ Recognize and intelligently install products regardless of file structure.
  - ✅ Handle any most file structures (ie: Content as root folder, or not).
  - ✅ Recursively install products in an archive; handles bundles.
  - ✅ Supports ZIP, RAR, and 7z formats
  - ✅ Create extraction & product records.
  - ✅ Choose your preferences (ie: install locations, temp location, product handling, etc).
  - ✅ Hold records of installation files (database).
  - ✅ Search products you've downloaded.

The application cannot at this stage:
  - ❌ Merge "part" archives into a single record.
  - ❌ Manage content / delete or move on request.
  - ❌ Handle certain MANIFEST tasks such as "EXECUTE"..
  - ❌ Handle password-protected archives appropriately (RAR, 7z).

## Points of Interest
`Assets` - Location for all Images used for product such as (rar logo, application logo, etc). <br>

`src\Custom Controls` - where "pages"/.NET custom controls are located. This is where most visual logic & user-event logic happens. 
  - Library Page - `Library.cs`
  - Extract Page - `Extract.cs`
  - Home Page - `Home.cs`
  - Settings Page - `Settings.cs`
  - LibraryItem - `LibraryItem.cs`
  - LibraryPanel - `LibraryPanel.cs`
  - PageButtonControl - `PageButtonControl.cs`
  - QueueControl - `QueueControl.cs`

`src\DP` - custom classes for handling various tasks - this is where most of the application logic is held.
  - `DPSettings` - Handles loading, saving, and re-generating of user settings - internal static class.
  - `DPRegistry` - Handles registry operations - typically only used to find DAZ registry values.
  - `DPAbstractFile` - **major POI** - Abstract class for all elements found in archives (including archives).
  - `DPAbstractArchive` - **major POI** - Abstract class for all supported archive files. Extends `DPAbstractFile`.
  - `DPFile` - **major POI** - Data class for all files that are not supported archive files. Extends `DPAbstractFile`.
  - `DPDazFile` - **major POI** - Class for reading .dsf and .duf files and getting meta data for tag creation. Extends `DPFile`.
  - `DPDSXFile` - **major POI** - Class for reading .dsx files which are typically Supplement and Manifest files. Extends `DPFile`.
  - `DPRARArchive` - **major POI** - Class specifically for processing RAR archives. Extends `DPAbstractArchive`.
  - `DP7zArchive` - **major POI** - Class specifically for processing 7z archives. Extends `DPAbstractArchive`. Not complete.
  - `DPZipArchive` - **major POI** - Class specifically for processing WinZip archives. Extends `DPAbstractArchive`.
  - `DPFolder` - **major POI** - Data class for folders. Holds the children files & archives, etc. DOES NOT EXTEND ANYTHING.
  - `DPProcessor` - **major POI** - Static class that determines what files get extracted & where, interacts with user, and more.
  - `DPExtractJob` - Holds user requested extract list and processes it on a new thread only when the previous job has finished.
  - `DPDSXParser` - Reads and parses .dsx files.
  - `DPDSXElement` - A data class for XML-like element. Contains attributes of the element.
  - `DPDSXElementCollection` - A collection of `DPDSXElement`s.
  - `DPProductInfo` - A struct for `DPAbstractArchive` that contians a summary of the collected information from all `DPContentInfo`s.
  - `DPContentInfo` - A struct for `DPFile` that contains information about the file. `DPDSXFile` uses this more.
  - `DPSortMethod` - Enum for sorting for the library
  - `DPProgressCombo` - A class for adding progress bars and updating progress bar text on the extract page.
  - `DPCache` - A special collection for cache collection. Used by `DPDatabase`.
  - `ContentType` - Enum for classifying the type of the file.
  - `UsefulFuncs` - `DPCommon`, `ArrayHelper`, and `PathHelper` are in this file.
  - `DPTaskManager` - class for ensuring async tasks are sequential.
  - And more.

`src\External` - Executables and source code not created by me or this community
  - `RAR.cs` - dependency for `DPProcessor`.
  - `7za.exe` - will be dependency for 7z operations. 

`src\Forms` - Similar to Custom Controls but is a .NET Form; handles visual & user-event logic for new dialog/application windows.
  - Main Form - `MainForm.cs` - The main form of this application.
  - Password Input Dialog - `PasswordInput.cs` - Special dialog for password-protected archives. 
  - Product Record Form - `ProductRecordForm.cs` - Form for displaying the record in the database (and later editing).
  - Database View - `DatabaseView.cs` - Form for displaying all contents in the database.

`src\Libs` - Currently only consists of rar.dll - used for handling RAR files.

`ImportFileRecordsToDatabase` - small program to transfer file system records to the database. Though, this is really only for me.

## How to get started?

This project requires you use **Visual Studio 2022.** Currently, we are using **.NET 6**, please make sure you have .NET 6 installed. _You might be able to use an older version like Visual Studio 2019, please let me know._

To open this project with Visual Studio, please select the solution file, located at `src\DAZ_Installer.sln`. All of the settings should be identical to mine, there may be some issues if you use Mac or Linux.

## License Summary

This project is under the Keep It Free License by Solomon Blount. A quick overview of the license, you may:
 - ✅ Copy
 - ✅ Distribute
 - ✅ Modify
 - ✅ Place Warranty
 - ✅ Commerical Use*

You may not:
 - ❌ Patent 
 - ❌ Trademark*
 - ❌ Sublicense

Protections (unless otherwise expressed):
 - 🟡 No liability
 - 🟡 No warranty

*Commerical Use is accepted however the product that uses anything under this license **MUST BE FREE**.*<br>
*Trademark is not accepted unless it follows the conditions set by this license.*

The full license is available at the root directory named "LICENSE".
