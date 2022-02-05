﻿using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Reminder.Jobs;

/// <summary>
/// Deletion of a weekly reminder
/// </summary>
public class WeeklyReminderDeletionJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Id of the reminder
    /// </summary>
    private long _id;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">Id</param>
    public WeeklyReminderDeletionJob(long id)
    {
        _id = id;
    }

    #endregion // Constructor

    #region  AsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteAsync()
    {
        var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var data = dbFactory.GetRepository<WeeklyReminderRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == _id)
                                    .Select(obj => new
                                                   {
                                                       ChannelId = obj.DiscordChannelId,
                                                       MessageId = obj.DiscordMessageId
                                                   })
                                    .FirstOrDefault();

                if (data?.MessageId != null)
                {
                    var discordClient = serviceProvider.GetService<DiscordSocketClient>();

                    var channel = await discordClient.GetChannelAsync(data.ChannelId).ConfigureAwait(false);
                    if (channel is ITextChannel textChannel)
                    {
                        var message = await textChannel.GetMessageAsync(data.MessageId.Value).ConfigureAwait(false);

                        await textChannel.DeleteMessageAsync(message).ConfigureAwait(false);

                        dbFactory.GetRepository<WeeklyReminderRepository>()
                                 .Refresh(obj => obj.Id == _id,
                                          obj => obj.DiscordMessageId = null);
                    }
                }
            }
        }
    }

    #endregion // AsyncJob
}