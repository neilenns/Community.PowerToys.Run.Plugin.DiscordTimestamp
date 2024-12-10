using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Wox.Plugin;

namespace DiscordTimestamp
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "AE72E27DA82C4398AA24E9FFBF0C1ABD";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "DiscordTimestamp";

        /// <summary>
        /// Description of the plugin.
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
            var search = query.Search;

            var date = DateTime.Now;
            var unixTimestamp = new DateTimeOffset(date).ToUnixTimeSeconds();

            return
            [
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Relative",
                    SubTitle = "1 minute ago",
                    ToolTipData = new ToolTipData("Relative", "1 minute ago"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:t>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Short time",
                    SubTitle = date.ToShortTimeString(),
                    ToolTipData = new ToolTipData("Short time", date.ToShortTimeString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:t>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Long time",
                    SubTitle = date.ToLongTimeString(),
                    ToolTipData = new ToolTipData("Long time", date.ToLongTimeString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:T>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Short date",
                    SubTitle = date.ToShortDateString(),
                    ToolTipData = new ToolTipData("Short date", date.ToShortDateString()),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:d>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Long date",
                    SubTitle =  date.ToString("MMMM d, yyyy"),
                    ToolTipData = new ToolTipData("Long date", $"{date:MMMM d, yyyy}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:D>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Long date with short time",
                    SubTitle = $"{date:MMMM d, yyyy} at {date.ToShortTimeString()}",
                    ToolTipData = new ToolTipData("Long date with short time", $"{date:MMMM d, yyyy} at {date.ToShortTimeString()}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:f>");
                        return true;
                    },
                    ContextData = search,
                },
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Long date with day of the week",
                    SubTitle = $"{date.ToLongDateString()} at {date.ToShortTimeString()}",
                    ToolTipData = new ToolTipData("Long date with day of the week", $"{date.ToLongDateString()} at {date.ToShortTimeString()}"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject($"<t:{unixTimestamp}:F>");
                        return true;
                    },
                    ContextData = search,
                }
            ];
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string search)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy to clipboard (Ctrl+C)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            Clipboard.SetDataObject(search);
                            return true;
                        },
                    }
                ];
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/discordtimestamp.light.png" : "Images/discordtimestamp.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
