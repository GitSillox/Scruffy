﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.Data.Entity.Tables.GuildAdministration
{
    /// <summary>
    /// Guilds
    /// </summary>
    [Table("Guilds")]
    public class GuildEntity
    {
        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// API-Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Id of the guild
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Discord Server
        /// </summary>
        public ulong DiscordServerId { get; set; }

        /// <summary>
        /// Id of the discord channel to post notifications
        /// </summary>
        public ulong? NotificationChannelId { get; set; }

        /// <summary>
        /// Id of the discord channel of the calendar
        /// </summary>
        public ulong? GuildCalendarChannelId { get; set; }

        /// <summary>
        /// Id of the calendar message
        /// </summary>
        public ulong? GuildCalendarMessageId { get; set; }

        /// <summary>
        /// Title of the calendar
        /// </summary>
        public string CalendarTitle { get; set; }

        /// <summary>
        /// Description of the calendar
        /// </summary>
        public string CalendarDescription { get; set; }

        /// <summary>
        /// Id of the discord channel of motd
        /// </summary>
        public ulong? MessageOfTheDayChannelId { get; set; }

        /// <summary>
        /// Id of the motd
        /// </summary>
        public ulong? MessageOfTheDayMessageId { get; set; }

        /// <summary>
        /// Reminder channel
        /// </summary>
        public ulong? ReminderChannelId { get; set;  }

        #region Navigation - Properties

        /// <summary>
        /// Discord server
        /// </summary>
        [ForeignKey(nameof(DiscordServerId))]
        public virtual ServerConfigurationEntity ServerConfiguration { get; set; }

        /// <summary>
        /// Log entries
        /// </summary>
        public virtual ICollection<GuildLogEntryEntity> GuildLogEntries { get; set; }

        #endregion // Navigation - Properties

        #endregion // Properties
    }
}
