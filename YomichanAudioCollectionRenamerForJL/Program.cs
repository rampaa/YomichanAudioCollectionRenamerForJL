using System.Text;
using System.Text.Json;

namespace YomichanAudioCollectionRenamerForJL;

file static class Program
{
    public static void Main()
    {
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;

        string? unzippedYomichanAudioCollectionUserFilesPath;
        while (true)
        {
            Console.WriteLine("Please enter the path of the 'user_files' folder found in the unzipped Yomichan Audio Collection folder.");
            unzippedYomichanAudioCollectionUserFilesPath = Console.ReadLine()?.Trim('"', ' ');
            if (!Directory.Exists(unzippedYomichanAudioCollectionUserFilesPath))
            {
                Console.WriteLine("The folder specified does not exist!");
            }
            else
            {
                if (Directory.Exists(Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "jpod_files"))
                    && Directory.Exists(Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "nhk16_files"))
                    && Directory.Exists(Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "shinmeikai8_files")))
                {
                    break;
                }

                Console.WriteLine("Incorrect directory!");
            }
        }

        string? outputPath;
        while (true)
        {
            Console.WriteLine("Please enter the path of the directory to save the renamed files.");
            outputPath = Console.ReadLine()?.Trim('"', ' ');
            if (Directory.Exists(outputPath))
            {
                break;
            }

            Console.WriteLine("Invalid file path!");
        }

        bool copyForvoFolder = GetAnswerOfYesNoQuestion("Forvo files don't need renaming. Should they be copied to the save folder regardless? (Y/N)");

        string nhk16JsonFilePath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "nhk16_files/entries.json");
        string nhk16AudioFolderPath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "nhk16_files/audio");
        string nhk16SaveDir = Path.Join(outputPath, "nhk16_files");

        string jpodJsonFilePath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "jpod_files/index.json");
        string jpodAudioFolderPath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "jpod_files/media");
        string jpodSaveDir = Path.Join(outputPath, "jpod_files");

        string shinmeikai8JsonFilePath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "shinmeikai8_files/index.json");
        string shinmeikai8AudioFolderPath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "shinmeikai8_files/media");
        string shinmeikai8SaveDir = Path.Join(outputPath, "shinmeikai8_files");

        Console.WriteLine("Creating the renamed versions of the files. This may take a while...");

        Task.WaitAll(RenameNhk16Files(nhk16JsonFilePath, nhk16AudioFolderPath, nhk16SaveDir),
            RenameShinmeikaiAndJPodFiles(shinmeikai8JsonFilePath, shinmeikai8AudioFolderPath, shinmeikai8SaveDir),
            RenameShinmeikaiAndJPodFiles(jpodJsonFilePath, jpodAudioFolderPath, jpodSaveDir));

        if (copyForvoFolder)
        {
            string forvoAudioFolderPath = Path.Join(unzippedYomichanAudioCollectionUserFilesPath, "forvo_files");
            string forvoSaveDir = Path.Join(outputPath, "forvo_files");
            foreach (string forvoSubfolder in Directory.EnumerateDirectories(forvoAudioFolderPath))
            {
                string subFolderSaveDir = Path.Join(forvoSaveDir, Path.GetFileName(forvoSubfolder));
                _ = Directory.CreateDirectory(subFolderSaveDir);
                foreach (string audioFilePath in Directory.EnumerateFiles(forvoSubfolder))
                {
                    string newAudioFilePath = Path.Join(subFolderSaveDir, Path.GetFileName(audioFilePath));
                    if (!File.Exists(newAudioFilePath))
                    {
                        File.Copy(audioFilePath, newAudioFilePath);
                    }
                }
            }
        }

        Console.WriteLine($"A renamed version of the files was successfully created under {outputPath}!");
        Console.WriteLine("Press any key to exit...");
        _ = Console.ReadKey();
    }

    private static async Task RenameShinmeikaiAndJPodFiles(string jsonFilePath, string audioFolderPath, string saveDir)
    {
        JsonElement jsonElement;

        FileStream fileStream = File.OpenRead(jsonFilePath);
        await using (fileStream.ConfigureAwait(false))
        {
            jsonElement = await JsonSerializer.DeserializeAsync<JsonElement>(fileStream).ConfigureAwait(false);
        }

        _ = Directory.CreateDirectory(saveDir);

        Dictionary<string, List<string>> primarySpellingToFileNames = new(129765);
        foreach (JsonProperty headWordElement in jsonElement.GetProperty("headwords").EnumerateObject())
        {
            if (!primarySpellingToFileNames.TryGetValue(headWordElement.Name, out List<string>? fileNames))
            {
                fileNames = new List<string>(headWordElement.Value.GetArrayLength());
                primarySpellingToFileNames[headWordElement.Name] = fileNames;
            }

            foreach (JsonElement fileNameElement in headWordElement.Value.EnumerateArray())
            {
                fileNames.Add(fileNameElement.GetString()!);
            }
        }

        Dictionary<string, string> fileNameToReading = new(134044);
        foreach (JsonProperty fileElement in jsonElement.GetProperty("files").EnumerateObject())
        {
            if (fileElement.Value.TryGetProperty("kana_reading", out JsonElement kanaReadingElement))
            {
                fileNameToReading.Add(fileElement.Name, kanaReadingElement.GetString()!);
            }
        }

        foreach ((string primarySpelling, List<string> fileNames) in primarySpellingToFileNames)
        {
            foreach (string fileName in fileNames)
            {
                string audioFilePath = Path.Join(audioFolderPath, fileName);
                if (File.Exists(audioFilePath))
                {
                    string newAudioFilePath = Path.Join(saveDir,
                        fileNameToReading.TryGetValue(fileName, out string? reading)
                            ? $"{reading} - {primarySpelling}.mp3"
                            : $"{primarySpelling}.mp3");

                    if (!File.Exists(newAudioFilePath))
                    {
                        File.Copy(audioFilePath, newAudioFilePath);
                    }
                }
            }
        }
    }

    private static async Task RenameNhk16Files(string jsonFilePath, string audioFolderPath, string saveDir)
    {
        List<JsonElement> jsonElements;

        FileStream fileStream = File.OpenRead(jsonFilePath);
        await using (fileStream.ConfigureAwait(false))
        {
            jsonElements = (await JsonSerializer.DeserializeAsync<List<JsonElement>>(fileStream).ConfigureAwait(false))!;
        }

        _ = Directory.CreateDirectory(saveDir);

        foreach (JsonElement jsonElement in jsonElements)
        {
            string? fileName = null;
            JsonElement accentsElement = jsonElement.GetProperty("accents");
            foreach (JsonElement accent in accentsElement.EnumerateArray())
            {
                fileName = accent.GetProperty("soundFile").GetString();
                if (fileName is not null)
                {
                    break;
                }
            }

            if (fileName is null)
            {
                continue;
            }

            string reading = jsonElement.GetProperty("kana").GetString()!;
            JsonElement kanjiArrayElement = jsonElement.GetProperty("kanji");
            int kanjiArrayElementCount = kanjiArrayElement.GetArrayLength();
            string audioFilePath = Path.Join(audioFolderPath, fileName);

            if (kanjiArrayElementCount > 0)
            {
                foreach (JsonElement kanjiElement in kanjiArrayElement.EnumerateArray())
                {
                    string newAudioFilePath = Path.Join(saveDir, $"{reading} - {kanjiElement.GetString()!}.mp3");
                    if (File.Exists(audioFilePath) && !File.Exists(newAudioFilePath))
                    {
                        File.Copy(audioFilePath, newAudioFilePath);
                    }
                }
            }
            else
            {
                string newAudioFilePath = Path.Join(saveDir, $"{reading} - {reading}.mp3");
                if (File.Exists(audioFilePath) && !File.Exists(newAudioFilePath))
                {
                    File.Copy(audioFilePath, newAudioFilePath);
                }
            }
        }
    }

    private static bool GetAnswerOfYesNoQuestion(string question)
    {
        while (true)
        {
            Console.WriteLine(question);
            string? userInput = Console.ReadLine();
            if (string.Equals(userInput, "Y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userInput, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(userInput, "N", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userInput, "No", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Invalid input!");
        }
    }
}
