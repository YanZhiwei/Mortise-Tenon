namespace Tenon.Repository.EfCore;

public class EfUserAuditInfo : IUserAuditInfo<long>
{
    public long UserId { get; set; }
}