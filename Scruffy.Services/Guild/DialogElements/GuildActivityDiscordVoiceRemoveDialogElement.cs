﻿using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.DialogElements;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a role which should be deleted
/// </summary>
public class GuildActivityDiscordVoiceRemoveDialogElement : DiscordRoleSelectionDialogElement
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildActivityDiscordVoiceRemoveDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DiscordRoleSelectionDialogElement

    /// <summary>
    /// Additional role filter
    /// </summary>
    public override Func<IRole, bool> RoleFilter
    {
        get
        {
            if (base.RoleFilter == null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var existingRoles = dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                            && obj.Type == DiscordActivityPointsType.Voice)
                                                 .Select(obj => obj.RoleId)
                                                 .ToList();

                    base.RoleFilter = role => existingRoles.Contains(role.Id);
                }
            }

            return base.RoleFilter;
        }
        set => base.RoleFilter = value;
    }

    #endregion // DiscordRoleSelectionDialogElement
}