using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.Archiving;

public static class DocumentExpressions
{
    public static Expression<Func<Document, bool>> BySourceType(SearchSourceType sourceType)
    {
        return d => d.SourceType == sourceType;
    }

    public static Expression<Func<Document, bool>> ByPhysicalFileId(Guid physicalFileId)
    {
        return d => d.PhysicalFileId == physicalFileId;
    }

    public static Expression<Func<Document, bool>> ByArchiveRecordId(Guid archiveRecordId)
    {
        return d => d.ArchiveRecordId == archiveRecordId;
    }

    public static Expression<Func<Document, bool>> ByFileType(string fileType)
    {
        return d => d.FileType == fileType;
    }

    public static Expression<Func<Document, bool>> ByDateRange(DateTime? from, DateTime? to)
    {
        return d =>
            (from == null || d.CreatedAt >= from) &&
            (to == null || d.CreatedAt <= to);
    }
}
