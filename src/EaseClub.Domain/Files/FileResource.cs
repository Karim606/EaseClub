using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files.Enums;
using EaseClub.Domain.MembershipApplications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Files
{
    public class FileResource : AuditableEntity
    {
        public Guid Id { get; private set; }

        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
        public long Size { get; private set; }
        public bool IsPrivate { get; private set; }
        public bool IsTemporary { get; private set; }
        public FileOwnerType OwnerType { get; private set; }
        public Guid OwnerId { get; private set; }
        public FilePurpose Purpose { get; private set; }

        private FileResource() { }

        private FileResource(Guid id, string fileName,string filePath, string contentType, long size,bool isPrivate, bool isTemporary, FilePurpose purpose, FileOwnerType ownerType, Guid ownerId) :base(id)
        {
            FileName = fileName;
            FilePath = filePath;
            ContentType = contentType;
            Size = size;
            IsPrivate = isPrivate;
            IsTemporary = isTemporary;
            Purpose = purpose;
            OwnerType = ownerType;
            OwnerId = ownerId;
        }

        public static Result<FileResource> Create(
        Guid id,
        string fileName,
        string filePath,
        string contentType,
        long size,
        Guid uploadedBy,
        bool isPrivate,
        Guid ownerId,
        FilePurpose purpose,
        FileOwnerType ownerType
        )
        {
            if ((purpose == FilePurpose.ClubBanner || purpose == FilePurpose.ClubLogo)&& ownerType != FileOwnerType.Club)
                return Error.Validation("Club files must have Club as owner.");
            
            if (purpose == FilePurpose.UserProfileImage && ownerType != FileOwnerType.User)
                return Error.Validation("User profile images must have User as owner.");

            if (purpose == FilePurpose.ApplicationDocument && ownerType != FileOwnerType.Application)
                return Error.Validation("Application documents must have Application as owner.");

            if (purpose == FilePurpose.EventImage && ownerType != FileOwnerType.Club)
                return Error.Validation("Event images must have Club as owner.");
 
            var file = new FileResource(id, fileName, filePath, contentType, size,isPrivate, true, purpose, ownerType, ownerId);

            if (purpose == FilePurpose.ApplicationDocument) file.IsPrivate = true;
                file.SetCreated(uploadedBy);
            return file;
        }

        public void MarkAsPermanent()
        {
            IsTemporary = false;
        }
    }
}
