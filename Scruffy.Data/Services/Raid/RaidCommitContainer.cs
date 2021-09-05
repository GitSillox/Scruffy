﻿using System.Collections.Generic;

namespace Scruffy.Data.Services.Raid
{
    /// <summary>
    /// Commit data container
    /// </summary>
    public class RaidCommitContainer
    {
        /// <summary>
        /// Id of the appointment
        /// </summary>
        public long AppointmentId { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        public List<RaidCommitUserData> Users { get; set; }
    }
}