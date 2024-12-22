namespace Tenon.Repository.EfCore;

public class EfAuditable : IAuditable<long>
{
    public long UserId { get; set; }
}