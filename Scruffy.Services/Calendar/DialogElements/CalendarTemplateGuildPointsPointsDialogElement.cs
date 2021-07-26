﻿using DSharpPlus.Entities;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Acquisition of the calendar template guild points
    /// </summary>
    public class CalendarTemplateGuildPointsPointsDialogElement : DialogMessageElementBase<int>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarTemplateGuildPointsPointsDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the number of guild points which can be earned by this event.");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override int ConvertMessage(DiscordMessage message)
        {
            return int.TryParse(message.Content, out var value) ? value : 0;
        }

        #endregion // DialogMessageElementBase<string>
    }
}