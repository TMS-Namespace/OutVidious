namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

/// <summary>
/// Database entity for AudioCodec enum values.
/// </summary>
public class EnumEntityBase
{
    public int Id { get; set; }

    public required string Name { get; set; }
}
