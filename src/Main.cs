// <copyright file="Main.cs" company="Neil Enns">
// Copyright (c) Neil Enns. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace DiscordTimestamp
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using Humanizer;
    using Humanizer.Configuration;
    using Humanizer.DateTimeHumanizeStrategy;
    using ManagedCommon;
    using Wox.Plugin;

    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public partial class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// Gets the ID of the plugin.
        /// </summary>
        public static string PluginID => "AE72E27DA82C4398AA24E9FFBF0C1ABD";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string Name => "DiscordTimestamp";

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        public string Description => "Generates timestamps in Discord format for easy pasting into Discord.";

        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            // If the default precision of .75 is used and "in 5 minutes" is the input query
            // the resulting humanized value is "in 4 minutes". Setting the precision to .85
            // ensures the result is "in 5 minutes".
            Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy(precision: .85);

            // Parse the query. If it fails to parse it means it's invalid input
            // and just return an empty list.
            var parser = new ChronicNetCore.Parser();
            var result = parser.Parse(query.Search);

            if (result == null || result.Start == null)
            {
                return [];
            }

            // The result from Chronic has a DateTimeKind of "Unspecified", which results in incorrect
            // humanized strings for inputs like "5:45am" when it is 5:30am. Specifying the kind as
            // Local resolves the problem.
            var date = DateTime.SpecifyKind(result.ToTime(), DateTimeKind.Local);
            var unixTimestamp = new DateTimeOffset(date).ToUnixTimeSeconds();

            // Humanize uses a different format for relative times than Discord does so try a hacky way
            // to convert it to a preview that looks like Discord's version. Dates in the future need
            // "from now" stripped off and "in" prefixed, so it becomes "in 5 minutes" instead of
            // "in 5 minutes from now". Dates in the past don't need any modification. This still isn't
            // perfect, Humanize says "yesterday" while Discord says "a day ago" but I can't be bothered
            // to chase down every little variation.
            var rawHumanizedRelative = date.Humanize();
            var humanizedRelative = rawHumanizedRelative.Contains(" from now") ?
                $"in {date.Humanize().Replace(" from now", string.Empty)}" :
                rawHumanizedRelative;

            return
            [
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Relative",
                    SubTitle = humanizedRelative,
                    ToolTipData = new ToolTipData("Relative", humanizedRelative),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:R>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Short time",
                    SubTitle = date.ToShortTimeString(),
                    ToolTipData = new ToolTipData("Short time", date.ToShortTimeString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:t>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Long time",
                    SubTitle = date.ToLongTimeString(),
                    ToolTipData = new ToolTipData("Long time", date.ToLongTimeString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:T>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Short date",
                    SubTitle = date.ToShortDateString(),
                    ToolTipData = new ToolTipData("Short date", date.ToShortDateString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:d>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Long date",
                    SubTitle = date.ToString("MMMM d, yyyy"),
                    ToolTipData = new ToolTipData("Long date", $"{date:MMMM d, yyyy}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:D>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Long date with short time",
                    SubTitle = $"{date:MMMM d, yyyy} {date.ToShortTimeString()}",
                    ToolTipData = new ToolTipData("Long date with short time", $"{date:MMMM d, yyyy} {date.ToShortTimeString()}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:f>");
                        return true;
                    },
                    ContextData = query.Search,
                },
                new Result
                {
                    QueryTextDisplay = query.Search,
                    IcoPath = this.IconPath,
                    Title = "Long date with day of the week",
                    SubTitle = $"{date.ToLongDateString()} {date.ToShortTimeString()}",
                    ToolTipData = new ToolTipData("Long date with day of the week", $"{date.ToLongDateString()} {date.ToShortTimeString()}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:F>");
                        return true;
                    },
                    ContextData = query.Search,
                }

            ];
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.Context.API.ThemeChanged += this.OnThemeChanged;
            this.UpdateIconPath(this.Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed || !disposing)
            {
                return;
            }

            if (this.Context?.API != null)
            {
                this.Context.API.ThemeChanged -= this.OnThemeChanged;
            }

            this.Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => this.IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/discordtimestamp.light.png" : "Images/discordtimestamp.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => this.UpdateIconPath(newTheme);
    }
}
