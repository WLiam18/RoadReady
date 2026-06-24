using Microsoft.AspNetCore.Http;

namespace RoadReady.BookingService.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folderName);
    Task<List<string>> SaveFilesAsync(List<IFormFile> files, string folderName);
}