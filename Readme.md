#Multilingual App Toolkit SYSTRAN translation provider

Multilingual App Toolkit is solution provided by Microsoft for application localization:
>The Multilingual App Toolkit v4.0 supports Windows 10 Universal, Windows 8.1 Shared project, WPF, WinForms, Windows Phone, ASP.Net apps and VS + Xamarin based iOS / Android projects.

Multilingual App Toolkit includes a dedicated xliff editor and an extension for Visual Studio. More details [here](https://visualstudiogallery.msdn.microsoft.com/6dab9154-a7e1-46e4-bbfa-18b5e81df520).

This provider add support for translation made by [SYSTRAN Platform REST](https://platform.systran.net/) using [Translation API](https://platform.systran.net/reference/translation).

##SYSTRAN Platform API Key

You need to get a valid API key from [SYSTRAN Platform here](https://platform.systran.net/) (Registration is required, but it is free). 

**Beware**: You must create a Server API Key (in Create API Key, you must chose Type Server). 

Then set it in the `Key` variable in SYSTRAN.DataProvider.json.

##How to build

This sample is built referencing **Microsoft.Multilingual.Translation.dll** 4.0.1605.0 directly from Multilingual App Toolkit installation folder. You need to install MAT first to be able to build this solution.

If you are targeting a different build of MAT, you will need to build against that version of the DLL as well as adjust for any API changes.

##How to install

You need to first install MAT to be able to build this solution.

Once the project is built, you will need to add it to the MAT Provider configuration files. Following the steps below will enable this provider with your MAT 4.0 installation:

1. Place the SYSTRAN.TranslationProvider.dll in `%CommonProgramFiles(x86)%\Multilingual App Toolkit` (we also use Newtonsoft.Json but same version is already in this directory).
2. Copy the SYSTRAN.DataProvider.json file to the `%ALLUSERSPROFILE%\Multilingual App Toolkit\SYSTRAN.DataProvider` folder.
   This file contains SYSTRAN 8 server Url (usually https://api-platform.systran.net/) and API Key:
   ```json
   {
   		"Url": "https://api-platform.systran.net/",
   		"Key": "xxxxxxxxxxxxxxxxxxx"
   	}
   ```
   3. Update `%ALLUSERSPROFILE%\Multilingual App Toolkit\TranslationManager.xml` to enable the
   SYSTRAN provider by adding the following XML configuration (Requires admin rights).  
   Note: Providers are used in the order listed in the configuration file.  It is recommended to
   place this sample provider at the top to ensure it is given priority during translation and
   suggestions.

   ```xml
   ...
    <Provider>
      <ID>5EEC87B4-4D5B-4898-A388-48E1977B2555</ID>
      <Name>SYSTRAN8TranslationProvider</Name>
      <ConfigFile>SYSTRAN.DataProvider\SYSTRAN.DataProvider.json</ConfigFile>
      <AssemblyPath>SYSTRAN.TranslationProvider.dll</AssemblyPath>
    </Provider>
   ...
   ```

To test the provider after it is built and configured:

1. Create a MAT enabled project using en-US as the source language
2. Add the source string "Do you want to save changes?"
3. Added fr-FR as the target language.
4. Build
5. Open the French XLF file in the Multilingual Editor.  
6. Highlight the resource string "Do you want to save changes?" and select Suggest from the ribbon (It should display the SYSTRAN logo)

That should get you working.

##Trouble shooting

If you compile using Debug configuration, a log file is created here: `C:\\Users\\*user_name*\\AppData\\Local\\Temp\\SYSTRAN.TranslationProvider.log`

**Q**: How can I tell if the provider is loaded?

**A**: The quickest way is to ensure the provider is listed first in the configuration file as the editor only displays the first supported provider's image based on language pairs (e.g.: en-US -> fr-FR).

**Q**: Everything is installed, but the provider is not loading

**A**: The provider needs to be compiled against the same build as the Microsoft.Multilingual.Translation.dll installed on your system. If you try to translate a resource, the load error will be displayed in the Editor Message tab or in  Visual Studio's output panel. The message should provide the details of the error.

##Additional information
###Links related to the Multilingual App Toolkit
* [Installation](https://visualstudiogallery.msdn.microsoft.com/6dab9154-a7e1-46e4-bbfa-18b5e81df520)
* [Blogs](http://blogs.msdn.com/b/matdev/)
* [User Voice Site](http://multilingualapptoolkit.uservoice.com)

###Links related to SYSTRAN
* [SYSTRAN Platform Rest APIs](https://platform.systran.net/)
