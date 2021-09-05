﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Committing the raid appointment
    /// </summary>
    public class RaidCommitDialogElement : DialogEmbedReactionElementBase<bool>
    {
        #region Fields

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<bool>> _reactions;

        /// <summary>
        /// Commit data
        /// </summary>
        private RaidCommitContainer _commitData;

        /// <summary>
        /// Localization service
        /// </summary>
        private LocalizationService _localizationService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commitData">Commit data</param>
        /// <param name="localizationService">Localization service</param>
        public RaidCommitDialogElement(LocalizationService localizationService, RaidCommitContainer commitData)
            : base(localizationService)
        {
            _localizationService = localizationService;
            _commitData = commitData;
        }

        #endregion // Constructor

        #region DialogEmbedReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("CommitTitle", "Raid points commit"));
            builder.WithDescription(LocalizationGroup.GetText("CommitText", "The following points will be committed:"));
            builder.WithColor(DiscordColor.Green);
            builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64");
            builder.WithTimestamp(DateTime.Now);

            var message = new StringBuilder();

            foreach (var user in _commitData.Users
                                            .OrderByDescending(obj => obj.Points))
            {
                var discordUser = await CommandContext.Client
                                                      .GetUserAsync(user.UserId)
                                                      .ConfigureAwait(false);

                message.AppendLine($"{Formatter.InlineCode(user.Points.ToString("#.##"))} - {DiscordEmojiService.GetGuildEmoji(CommandContext.Client, user.DiscordEmoji)} {discordUser.Mention}");
            }

            message.AppendLine("\u200b");

            builder.AddField(LocalizationGroup.GetText("Users", "Users"), message.ToString());
        }

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<bool>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<bool>>
                                  {
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add user", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var data = await RunSubForm<RaidCommitUserFormData>().ConfigureAwait(false);

                                                     var user = _commitData.Users
                                                                           .FirstOrDefault(obj => obj.UserId == data.User.Id);

                                                     if (user != null)
                                                     {
                                                         user.Points = data.Points;
                                                     }
                                                     else
                                                     {
                                                         _commitData.Users
                                                                    .Add(new RaidCommitUserData
                                                                         {
                                                                             Points = data.Points,
                                                                             UserId = data.User.Id
                                                                         });
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("SetPointsCommand", "{0} Set points", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var data = await RunSubForm<RaidCommitUserFormData>().ConfigureAwait(false);

                                                     var user = _commitData.Users
                                                                           .FirstOrDefault(obj => obj.UserId == data.User.Id);

                                                     if (user != null)
                                                     {
                                                         user.Points = data.Points;
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("RemoveCommand", "{0} Remove user", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var discordUser = await RunSubElement<RaidCommitRemoveUserDialogElement, DiscordUser>(new RaidCommitRemoveUserDialogElement(_localizationService)).ConfigureAwait(false);

                                                     var user = _commitData.Users
                                                                           .FirstOrDefault(obj => obj.UserId == discordUser.Id);

                                                     if (user != null)
                                                     {
                                                         _commitData.Users.Remove(user);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CommitCommand", "{0} Commit", DiscordEmojiService.GetCheckEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         foreach (var user in _commitData.Users)
                                                         {
                                                             dbFactory.GetRepository<RaidRegistrationRepository>()
                                                                      .AddOrRefresh(obj => obj.AppointmentId == _commitData.AppointmentId
                                                                                        && obj.UserId == user.UserId,
                                                                                    obj =>
                                                                                    {
                                                                                        if (obj.Id == 0)
                                                                                        {
                                                                                            obj.AppointmentId = _commitData.AppointmentId;
                                                                                            obj.RegistrationTimeStamp = DateTime.Now;
                                                                                            obj.UserId = user.UserId;
                                                                                        }

                                                                                        obj.Points = user.Points;
                                                                                    });
                                                         }

                                                         var dateLimit = DateTime.Today;

                                                         while (dateLimit.DayOfWeek != DayOfWeek.Monday)
                                                         {
                                                             dateLimit = dateLimit.AddDays(-1);
                                                         }

                                                         dateLimit = dateLimit.AddDays(-7 * 14);

                                                         foreach (var user in await dbFactory.GetRepository<RaidRegistrationRepository>()
                                                                                             .GetQuery()
                                                                                             .Where(obj => obj.Points != null
                                                                                                        && obj.RaidAppointment.TimeStamp > dateLimit)
                                                                                             .GroupBy(obj => obj.UserId)
                                                                                             .Select(obj => new
                                                                                                            {
                                                                                                                UserId = obj.Key,
                                                                                                                Points = obj.Sum(obj2 => (double)obj2.Points)
                                                                                                            })
                                                                                             .ToListAsync()
                                                                                             .ConfigureAwait(false))
                                                         {
                                                             dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                                                      .AddOrRefresh(obj => obj.UserId == user.UserId,
                                                                                    obj =>
                                                                                    {
                                                                                        obj.UserId = user.UserId;
                                                                                        obj.Points = user.Points;
                                                                                    });
                                                         }

                                                         var nextAppointment = new RaidAppointmentEntity();

                                                         dbFactory.GetRepository<RaidAppointmentRepository>()
                                                                  .Refresh(obj => obj.Id == _commitData.AppointmentId,
                                                                           obj =>
                                                                           {
                                                                               obj.IsCommitted = true;

                                                                               nextAppointment.ConfigurationId = obj.ConfigurationId;
                                                                               nextAppointment.TemplateId = obj.TemplateId;
                                                                               nextAppointment.TimeStamp = obj.TimeStamp.AddDays(7);
                                                                               nextAppointment.Deadline = obj.Deadline.AddDays(7);
                                                                           });

                                                         dbFactory.GetRepository<RaidAppointmentRepository>()
                                                                  .Add(nextAppointment);
                                                     }

                                                     return false;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                                          Func = () => Task.FromResult(false)
                                      },
                                  };
        }

        /// <summary>
        /// Returns the title of the commands
        /// </summary>
        /// <returns>Commands</returns>
        protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override bool DefaultFunc() => false;

        #endregion // DialogEmbedReactionElementBase<bool>
    }
}