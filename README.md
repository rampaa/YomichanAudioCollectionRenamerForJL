# What is it?

It's a tool for renaming the Yomichan Audio Collection files, which can be downloaded from [local-audio-yomichan](https://github.com/yomidevs/local-audio-yomichan) (download the **MP3** version), into the "{Reading} - {Term}.mp3" format. This allows [JL](https://github.com/rampaa/JL) to use those local files without needing the [Local Audio Server for Yomichan](https://ankiweb.net/shared/info/1045800357) add-on or even Anki itself.

# How to use it?

1) Run the program.  
2) Specify the path of the `user_files` folder found under the unzipped Yomichan Audio Collection folder (e.g., `C:\local-yomichan-audio-collection-2023-06-11\user_files`).  
3) Specify the location where you want the renamed versions of those files to be saved (e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection`).  
4) Specify whether you want to copy the `forvo_files` folder and its subfolders to that path as well, even though they don't require renaming.  
5) Wait until the program confirms that it has successfully created the renamed versions of those files under the path you've specified.

**After that, you can add these files to JL as audio sources by following these steps:**

1. Right-click on JL's main window and select **Manage audio sources**.  
2. Click the **Add audio source** button and choose **Local Path** as the **Source Type**.
3. After specifying the path click the save button

**Example Paths for Each Source:**

- **jpod**  
  You need to add two sources for `jpod`:  
  - `<Full path of the jpod_files folder>\{Reading} - {Term}.mp3`  
    e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection\jpod_files\{Reading} - {Term}.mp3`
  
  - `<Full path of the jpod_files folder>\{Term}.mp3`  
    e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection\jpod_files\{Term}.mp3`

- **nhk16**  
  `<Full path of the nhk16_files folder>\{Reading} - {Term}.mp3`  
  e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection\nhk16_files\{Reading} - {Term}.mp3`

- **shinmeikai8**  
  `<Full path of the shinmeikai8_files folder>\{Reading} - {Term}.mp3`  
  e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection\shinmeikai8_files\{Reading} - {Term}.mp3`

- **forvo**  
  `<Full path of the akitomo/kaoring/poyotan/skent/strawberrybrown folder>\{Term}.mp3`  
  e.g., `C:\Users\User\Desktop\Folders\Local Yomichan Audio Collection\forvo_files\akitomo\{Term}.mp3`
