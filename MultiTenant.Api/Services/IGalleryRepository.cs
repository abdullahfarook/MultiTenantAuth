using System;
using System.Collections.Generic;
using MultiTenant.Api.Entities;

namespace MultiTenant.Api.Services
{
    public interface IGalleryRepository
    {
        IEnumerable<Image> GetImages(string ownerId);
        bool IsImageOwner(Guid id, string ownerId);
        Image GetImage(Guid id);
        bool ImageExists(Guid id);
        void AddImage(Image image);
        void UpdateImage(Image image);
        void DeleteImage(Image image);
        bool Save();
    }
}
