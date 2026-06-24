using Microsoft.AspNetCore.Http;
using RoadReady.BookingService.Interfaces;

namespace RoadReady.BookingService.Implementations;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
        {
            return string.Empty;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", folderName);
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{folderName}/{uniqueFileName}";
    }

    public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string folderName)
    {
        var savedPaths = new List<string>();

        if (files == null || !files.Any())
        {
            return savedPaths;
        }

        foreach (var file in files)
        {
            var path = await SaveFileAsync(file, folderName);
            if (!string.IsNullOrEmpty(path))
            {
                savedPaths.Add(path);
            }
        }

        return savedPaths;
    }
}