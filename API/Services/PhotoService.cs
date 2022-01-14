using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
  public class PhotoService : IPhotoService
  {
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
      var acc = new Account(
          config.Value.CloudName,
          config.Value.ApiKey,
          config.Value.ApiSecret
      );
      // use the info in appsettings file to create an account, and use it to create cloudinary object
      _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
      var uploadResult = new ImageUploadResult();

      // if there is a file, then we upload it
      if (file.Length > 0)
      {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
          File = new FileDescription(file.FileName, stream),
          Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
        };
        uploadResult = await _cloudinary.UploadAsync(uploadParams);
      }
      return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
      // use the publicId to get params then delete the image
      var deleteParams = new DeletionParams(publicId);
      var deleteResult = await _cloudinary.DestroyAsync(deleteParams);
      return deleteResult;
    }
  }
}