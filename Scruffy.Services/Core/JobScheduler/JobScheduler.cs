﻿using System;
using System.Linq;
using System.Threading.Tasks;

using FluentScheduler;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Reminder;
using Scruffy.Services.Fractals;
using Scruffy.Services.Reminder;

namespace Scruffy.Services.Core.JobScheduler
{
    /// <summary>
    /// Scheduling jobs
    /// </summary>
    public class JobScheduler : IAsyncDisposable
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public JobScheduler()
        {
            JobManager.Initialize();
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Starting the job server
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async Task StartAsync()
        {
            await Task.Run(JobManager.Start).ConfigureAwait(false);

            JobManager.AddJob<FractalDailyRefreshJob>(obj => obj.ToRunEvery(1).Days().At(0, 0));

            // fractal reminders
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                var fractalReminderService = serviceProvider.GetService<FractalReminderService>();

                await fractalReminderService.CreateNextReminderJobAsync()
                                            .ConfigureAwait(false);

                await fractalReminderService.CreateReminderDeletionJobsAsync()
                                            .ConfigureAwait(false);
            }

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                // one tine reminders
                var oneTimeReminders = await dbFactory.GetRepository<OneTimeReminderRepository>()
                                                      .GetQuery()
                                                      .Where(obj => obj.IsExecuted == false)
                                                      .Select(obj => new
                                                                     {
                                                                         obj.Id,
                                                                         obj.TimeStamp
                                                                     })
                                                      .ToListAsync().ConfigureAwait(false);

                foreach (var oneTimeReminder in oneTimeReminders)
                {
                    JobManager.AddJob(new OneTimeReminderJob(oneTimeReminder.Id), obj => obj.ToRunOnceAt(oneTimeReminder.TimeStamp));
                }

                // weekly reminders
                var weeklyReminders = await dbFactory.GetRepository<WeeklyReminderRepository>()
                                                      .GetQuery()
                                                      .Select(obj => new
                                                                     {
                                                                         obj.Id,
                                                                         obj.DayOfWeek,
                                                                         obj.PostTime,
                                                                         obj.DeletionTime
                                                                     })
                                                      .ToListAsync().ConfigureAwait(false);

                foreach (var weeklyReminder in weeklyReminders)
                {
                    AddWeeklyReminder(weeklyReminder.Id, weeklyReminder.DayOfWeek, weeklyReminder.PostTime, weeklyReminder.DeletionTime);
                }
            }
        }

        /// <summary>
        /// Adding a one time reminder
        /// </summary>
        /// <param name="timeStamp">Timestamp</param>
        /// <param name="id">Database id of the entity</param>
        public void AddOneTimeReminder(DateTime timeStamp, long id)
        {
            JobManager.AddJob(new OneTimeReminderJob(id), obj => obj.ToRunOnceAt(timeStamp));
        }

        /// <summary>
        /// Adding a weekly reminder
        /// </summary>
        /// <param name="id">Id of the reminder</param>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="postTime">Post time</param>
        /// <param name="deletionTime">Deletion time</param>
        public void AddWeeklyReminder(long id, DayOfWeek dayOfWeek, TimeSpan postTime, TimeSpan deletionTime)
        {
            var postTimeStamp = DateTime.Today.Add(postTime);

            while (postTimeStamp < DateTime.Now
                || postTimeStamp.DayOfWeek != dayOfWeek)
            {
                postTimeStamp = postTimeStamp.AddDays(1);
            }

            JobManager.AddJob(new WeeklyReminderPostJob(id),
                              obj => obj.ToRunOnceAt(postTimeStamp).AndEvery(7).Days().At(postTime.Hours, postTime.Minutes));

            var deletionTimeStamp = DateTime.Today.Add(deletionTime);

            while (deletionTimeStamp < DateTime.Now
                || deletionTimeStamp.DayOfWeek != dayOfWeek)
            {
                deletionTimeStamp = deletionTimeStamp.AddDays(1);
            }

            JobManager.AddJob(new WeeklyReminderDeletionJob(id),
                              obj => obj.ToRunOnceAt(deletionTimeStamp).AndEvery(7).Days().At(postTime.Hours, postTime.Minutes));
        }

        /// <summary>
        /// Add a job
        /// </summary>
        /// <param name="job">Job</param>
        /// <param name="timeStamp">Time stamp to run the job</param>
        /// <returns>Name of the added job</returns>
        public string AddJob(IJob job, DateTime timeStamp)
        {
            var jobName = Guid.NewGuid().ToString();

            JobManager.AddJob(job, obj => obj.WithName(jobName).ToRunOnceAt(timeStamp));

            return jobName;
        }

        /// <summary>
        /// Removes the job by the given name
        /// </summary>
        /// <param name="jobName">Name of the job</param>
        public void RemoveJob(string jobName)
        {
            JobManager.RemoveJob(jobName);
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns> A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await Task.Run(JobManager.StopAndBlock).ConfigureAwait(false);
        }

        #endregion // IAsyncDisposable
    }
}
