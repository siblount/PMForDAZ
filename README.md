# DAZ Product Installer

This is an application currently only for Windows users to install their products regardless of the vendor's format; it accepts unique file structurs and archive extensions.


## What does this project aim to do?
Firstly, it aims to be a **free** and **open-source*** application to gain trust of users, allow others to contribute, and release a product that is useful for all.
Additonally, it - 
 - Make an application that is able to process any archive (ZIP, RAR, 7z).
 - Quickly install products regardless of file structures. **This is very useful for Renderosity and other vendor websites**.
 - Also be a library to store processed/installed products.
 - Be a database to search for products for users who have a lot of products.
 - [ In debate ] Be a product management for products that are from Renderosity and other vendors.


## Project Status
Alpha

Currently, the application can:
  - ✅ Recognize and intelligently install products regardless of file structure.
  - ✅ Handle any most file structures (ie: Content as root folder, or not).
  - ✅ Recursively install products in an archive; handles bundles.
  - ✅ Supports WIN-ZIP, RAR formats
  - ✅ Create extraction & product records.
  - ✅ Choose your preferences (ie: install locations, temp location, product handling, etc).

The application cannot at this stage:
  - ❌ Handle 7z files (partially implemented).
  - ❌ Detect product IDs from DAZ consistently.
  - ❌ Fetch thubmnails consistently - error prone.
  - ❌ Merge "part" archives into a single record.
  - ❌ Manage content / delete or move on request.
  - ❌ Handle certain MANIFEST tasks such as "EXECUTE".
  - ❌ Consistently create tags based off of file names, content, etc (creates some tags but stops due to some error).
  - ❌ Handle password-protected archives appropriately (RAR, 7z).

## Points of Interest
`Assets` - Location for all Images used for product such as (rar logo, application logo, etc). <br>

`src\Custom Controls` - where "pages"/.NET custom controls are located. This is where most visual logic & user-event logic happens. 
  - Library Page - `Library.cs` & `Library.designer.cs`
  - Extract Page - `extractControl.cs` & `extractControl.designer.cs` * will be renamed *
  - Home Page - `userControl1.cs`
  - Settings Page - `Settings.cs`
`src\DP` - custom classes for handling various tasks - this is where most of the application logic is held.
  - `DPSettings` - Handles loading, saving, and re-generating of user settings - internal static class.
  - `DPRegistry` - Handles registry operations - typically only used to find DAZ registry values.
  - `DPFile` - **major POI** - Data class for all found items in archives excluding file types that is a supported archive type.
  - `DPArchive` - **major POI** - Data class for all supported archive types.
  - `DPFolder` - **major POI** - Data class for folders. Holds the children files & archives, etc.
  - `DPProcessor` - **major POI** - Static class that handles the discovery, extraction, and movement of archives/products.
  - `DPExtractJob` - Holds user requested extract list and processes it on a new thread only when the previous job has finished.
  - and many more
`src\External` - Executables and source code not created by me or this community
  - `RAR.cs` - dependency for `DPProcessor`.
  - `7za.exe` - will be dependency for 7z operations. 
`src\Forms` - Similar to Custom Controls but is a .NET Form; handles visual & user-event logic for new dialog/application windows.
  - Main Form - `Form1.cs`
  - Password Input Dialog - `PasswordInput.cs`
  - Product Record Form - `ProductRecordForm.cs`
  - etc
`src\Libs` - Currently only consists of rar.dll - used for handling RAR files.

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
