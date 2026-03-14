using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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
        public FileCategory Category { get; private set; } 
        public bool IsTemporary { get; private set; }
        public Guid? ClubId { get; private set; }     // which club owns it
        public Guid? ApplicationId { get; private set; } // linked membership application
        public Club? Club { get; private set; }  // Navigation property
        public MembershipApplication? Application { get; private set; }

        private FileResource() { }

        private FileResource(Guid id, string fileName,string filePath, string contentType, long size,FileCategory category,bool isPrivate, bool isTemporary):base(id)
        {
            FileName = fileName;
            FilePath = filePath;
            ContentType = contentType;
            Size = size;
            Category = category;
            IsPrivate = isPrivate;
            IsTemporary = isTemporary;
        }

        public static Result<FileResource> Create(
        Guid id,
        string fileName,
        string filePath,
        string contentType,
        long size,
        FileCategory category,
        Guid uploadedBy,
        bool isPrivate,
        Guid? clubId = null,
        Guid? applicationId = null)
        {
            var file = new FileResource(id, fileName, filePath, contentType, size, category,isPrivate, true);
            file.SetCreated(uploadedBy);
            file.ClubId = clubId;
            file.ApplicationId = applicationId;
            return file;
        }

        public void MarkAsPermanent()
        {
            IsTemporary = false;
        }
    }
}
