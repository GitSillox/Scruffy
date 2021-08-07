﻿using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Queryable.Raid;
using Scruffy.Data.Entity.Repositories.Base;
using Scruffy.Data.Entity.Tables.Raid;

namespace Scruffy.Data.Entity.Repositories.Raid
{
    /// <summary>
    /// Repository for accessing <see cref="RaidRoleLineupAssignmentEntity"/>
    /// </summary>
    public class RaidRoleLineupAssignmentRepository : RepositoryBase<RaidRoleLineupAssignmentQueryable, RaidRoleLineupAssignmentEntity>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"><see cref="DbContext"/>-object</param>
        public RaidRoleLineupAssignmentRepository(ScruffyDbContext dbContext)
            : base(dbContext)
        {
        }

        #endregion // Constructor
    }
}
