using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces
{
    public interface IEntity
    {
        /// <summary>
        /// Database primary key identifier.
        /// </summary>
        public int Id { get; set; }
    }
}