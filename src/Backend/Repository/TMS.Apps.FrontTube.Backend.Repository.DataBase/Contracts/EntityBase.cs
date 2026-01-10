using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

    public abstract class  EntityBase :  IEntity //ITrackable, IMergeable, 
    {
        public int Id { get; set; }
        
        // [NotMapped]
        // public TrackingState TrackingState { get; set; }

        // [NotMapped]
        // public ICollection<string> ModifiedProperties { get; set; }

        // [NotMapped]
        // public Guid EntityIdentifier { get; set; }
    }