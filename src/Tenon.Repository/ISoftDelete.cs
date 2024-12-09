namespace Tenon.Repository;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}