using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class FileService
{
    // Method to save the file to a given path on the server
    public async Task<string> FileSaveToServer(IFormFile file, string folderPath)
    {
        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Get the original file name and generate a unique name to avoid conflicts
        var fileName = Path.GetFileName(file.FileName);
        var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        // Combine folder path and file name to get the full file path
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Save the file to the server
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return the saved file's name or path (adjust based on your requirements)
        return uniqueFileName;
    }
}
