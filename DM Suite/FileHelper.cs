using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

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
                    Debug.WriteLine("WriteToFile failed: " + ex.ToString());
                }
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }
        }
    }
}
