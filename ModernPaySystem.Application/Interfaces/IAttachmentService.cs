using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Attachment service CRUD operations.
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Get all attachments.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetAllAsync();

    /// <summary>
    /// Get paged attachments.
    /// </summary>
    Task<Result<PagedList<Attachment>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get attachment by id.
    /// </summary>
    Task<Result<Attachment>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get attachments by file type.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetByFileTypeAsync(string fileType);

    /// <summary>
    /// Get attachment by file name.
    /// </summary>
    Task<Result<Attachment>> GetByFileNameAsync(string fileName);

    /// <summary>
    /// Create new attachment.
    /// </summary>
    Task<Result<Attachment>> CreateAsync(Attachment attachment);

    /// <summary>
    /// Update attachment.
    /// </summary>
    Task<Result<Attachment>> UpdateAsync(Guid id, Attachment attachment);

    /// <summary>
    /// Delete attachment.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
