﻿using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Removing a user from the commit
    /// </summary>
    public class RaidCommitRemoveUserDialogElement : DialogMessageElementBase<DiscordUser>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidCommitRemoveUserDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Which user should be removed?");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override DiscordUser ConvertMessage(DiscordMessage message)
        {
            var converter = (IArgumentConverter<DiscordUser>)new DiscordUserConverter();

            return converter.ConvertAsync(message.Content, CommandContext.GetCommandContext()).Result.Value;
        }

        #endregion // DialogMessageElementBase
    }
}