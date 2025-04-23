using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services;

public class mangeFilesService
{
      public async Task<List<string>> UploadAttachments(List<IFormFile> files, string folderName,int itemId)
        {
            if (files == null || !files.Any())
                return null!;

            var uploadsFolder = Path.Combine("wwwroot", "uploads", folderName);
             
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" }.Contains(fileExtension.ToLower()))
                        throw new InvalidOperationException("Invalid file type.");

                    var filePath = Path.Combine(uploadsFolder, $"{folderName}_{itemId}_{Path.GetFileName(file.FileName)}");
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        filePaths.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading file {file.FileName}: {ex.Message}");
                    }
                }
            }
            return filePaths;
        }
        
        public async Task<List<string>> UploadImage(IFormFile file,string folderName, int itemId)
        {
            if (file == null)
                return null!;

            var uploadsFolder = Path.Combine("wwwroot", "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePaths = new List<string>();
            if (file.Length > 0)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(fileExtension.ToLower()))
                    throw new InvalidOperationException("Invalid file type.");

                   var filePath = Path.Combine(uploadsFolder, $"{folderName}_{itemId}_{Path.GetFileName(file.FileName)}");

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        filePaths.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading file {file.FileName}: {ex.Message}");
                    }
                }
            return filePaths;
        }
        public async Task<bool> RemoveAttachmentsAsync(string folderName,int itemId)
        {
            var attachmentsFolder = Path.Combine("wwwroot", "uploads", folderName);

            if (!Directory.Exists(attachmentsFolder))
            {
                Console.WriteLine("Attachments folder does not exist.");
                return false;
            }
            string searchPattern = $"{folderName}_{itemId}_*";

            try
            {
                var files = Directory.GetFiles(attachmentsFolder, searchPattern);

                if (files.Length == 0)
                {
                    Console.WriteLine("No attachments found for the given student ID.");
                    return false;
                }

                bool allDeleted = true;

                foreach (var file in files)
                {
                    try
                    {
                        // Asynchronously delete the file
                        await Task.Run(() => File.Delete(file));
                        Console.WriteLine($"Deleted attachment: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting attachment {Path.GetFileName(file)}: {ex.Message}");
                        allDeleted = false;
                    }
                }

                return allDeleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while removing attachments: {ex.Message}");
                return false;
            }
        }

        public bool RemoveFile(string folderName,int itemId)
        {
            // Define the uploads folder path
            var uploadsFolder = Path.Combine("wwwroot", "uploads", folderName);

            // Check if the uploads folder exists
            if (!Directory.Exists(uploadsFolder))
            {
                return false;
            }
            string searchPattern = $"{folderName}_{itemId}_*";

            try
            {
                var files = Directory.GetFiles(uploadsFolder, searchPattern);

                if (files.Length == 0)
                {
                    return false;
                }

                bool allDeleted = true;

                // Iterate through each file and attempt to delete it
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        allDeleted = false;
                    }
                }

                return allDeleted;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
}
