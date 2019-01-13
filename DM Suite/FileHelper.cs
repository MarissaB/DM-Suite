using DM_Suite.Services.LoggingServices;
using MetroLog;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace DM_Suite
{
    public class FileHelper
    {
        public static async void WriteToFile(string fileText, string fileName)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.SuggestedFileName = fileName;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    await FileIO.WriteTextAsync(file, fileText);
                }
                catch (Exception ex)
                {
                    LoggingServices.Instance.WriteLine<FileHelper>("WriteToFile failed: " + ex.ToString(), LogLevel.Error);
                }
            }
            else
            {
                LoggingServices.Instance.WriteLine<FileHelper>("Cancelled write to file.", LogLevel.Info);
            }
        }
    }
}
