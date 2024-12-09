namespace Tenon.Repository;

public interface IFullAuditable<TKey> : IBasicAuditable where TKey : struct
{
    TKey CreatedBy { get; set; }
    TKey? UpdatedBy { get; set; }
    TKey? DeletedBy { get; set; }
}