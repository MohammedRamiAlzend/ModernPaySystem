using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Specifications;

public static class RequestIncludes
{
    public static IQueryable<Request> IncludeFull(this IQueryable<Request> query) =>
        query.Include(x => x.RequestTemplateValues)!.ThenInclude(x => x!.Template)
             .Include(x => x.RequestTemplateValues)!.ThenInclude(x => x!.InputValues)
             .Include(x => x.Requester)!.ThenInclude(r => r!.Department)
             .Include(x => x.Approver)!.ThenInclude(a => a!.Department);

    public static IQueryable<Request> IncludeWithAttachments(this IQueryable<Request> query) =>
        query.IncludeFull()
             .Include(x => x.RequestAttachments)!.ThenInclude(x => x!.Attachment)
             .Include(x => x.OutgoingRelations)!.ThenInclude(r => r.TargetRequest);

    public static IQueryable<Response> IncludeFullResponse(this IQueryable<Response> query) =>
        query.Include(r => r.Request)!.ThenInclude(r => r!.RequestAttachments)
             .Include(r => r.Request)!.ThenInclude(r => r!.RequestTemplateValues)!.ThenInclude(x => x!.Template)
             .Include(r => r.Request)!.ThenInclude(r => r!.RequestTemplateValues)!.ThenInclude(x => x!.InputValues)
             .Include(r => r.Request)!.ThenInclude(r => r!.Approver)!.ThenInclude(a => a!.Department)
             .Include(r => r.Request)!.ThenInclude(r => r!.Requester)!.ThenInclude(r => r!.Department)
             .Include(r => r.Request)!.ThenInclude(r => r!.OutgoingRelations)!.ThenInclude(r => r.TargetRequest);

    public static IQueryable<ArchiveRecord> IncludeFormValues(this IQueryable<ArchiveRecord> query) =>
        query.Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues);
}
