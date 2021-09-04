﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.ServiceHost.Discord
{
    /// <summary>
    /// Management of the discord bot
    /// </summary>
    public sealed class DiscordBot : IAsyncDisposable
    {
        #region Fields

        /// <summary>
        /// Client
        /// </summary>
        private DiscordClient _discordClient;

        /// <summary>
        /// Commands
        /// </summary>
        private CommandsNextExtension _commands;

        /// <summary>
        /// Prefix resolver
        /// </summary>
        private PrefixResolvingService _prefixResolver;

        /// <summary>
        /// Error handling
        /// </summary>
        private DiscordErrorHandler _errorHandler;

        /// <summary>
        /// Administration permissions validation
        /// </summary>
        private AdministrationPermissionsValidationService _administrationPermissionsValidationService;

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Start the discord bot
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            var config = new DiscordConfiguration
                         {
                             Token = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"),
                             TokenType = TokenType.Bot,
                             AutoReconnect = true,
                         };

            _discordClient = new DiscordClient(config);

            _discordClient.UseInteractivity(new InteractivityConfiguration
                                            {
                                                Timeout = TimeSpan.FromMinutes(2)
                                            });

            GlobalServiceProvider.Current.AddSingleton(_discordClient);

            _prefixResolver = new PrefixResolvingService();
            GlobalServiceProvider.Current.AddSingleton(_prefixResolver);

            _administrationPermissionsValidationService = new AdministrationPermissionsValidationService();
            GlobalServiceProvider.Current.AddSingleton(_administrationPermissionsValidationService);

            GlobalServiceProvider.Current.AddSingleton(new DiscordStatusService(_discordClient));

            _commands = _discordClient.UseCommandsNext(new CommandsNextConfiguration
                                                       {
                                                           PrefixResolver = _prefixResolver.OnPrefixResolver,
                                                           EnableDms = true,
                                                           EnableMentionPrefix = true,
                                                           CaseSensitive = false,
                                                           DmHelp = false,
                                                           EnableDefaultHelp = false,
                                                           Services = GlobalServiceProvider.Current.GetServiceProvider()
                                                       });

            _commands.SetHelpFormatter<HelpCommandFormatter>();

            _commands.RegisterCommands(Assembly.Load("Scruffy.Commands"));

            _errorHandler = new DiscordErrorHandler(_commands);

            await _discordClient.ConnectAsync().ConfigureAwait(false);
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_discordClient != null)
            {
                _errorHandler?.Dispose();
                _errorHandler = null;

                await _discordClient.DisconnectAsync().ConfigureAwait(false);

                _discordClient.Dispose();
            }
        }

        #endregion // IAsyncDisposable
    }
}
