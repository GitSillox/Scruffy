﻿using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.GuildAdministration.DialogElements
{
    /// <summary>
    /// Acquisition of the calendar title
    /// </summary>
    public class GuildAdministrationCalendarTitleDialogElement : DialogMessageElementBase<string>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationCalendarTitleDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the calendar title.");

        #endregion // DialogMessageElementBase<string>
    }
}