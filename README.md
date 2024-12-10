# Discord timestamp for PowerToys Run

A [PowerToys Run](https://learn.microsoft.com/en-us/windows/powertoys/run) plugin for generating Discord timestamps,
inspired by the amazing [Discord Timestamp](https://discordtimestamp.com/) website.

To use type `dt` followed by a date and/or time. Dates and times can be specified in [wide range of natural
language forms](https://github.com/mojombo/chronic?tab=readme-ov-file#examples). Select the desired Discord
timestamp format from the resulting list then paste it into Discord.

![Screenshot of the plugin showing results for "in 5 minutes"](Docs/results-example.png)

The result when pasted into Discord:

## Development notes

This project is based on the [Community.PowerToys.Run.Plugin.Templates](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates)
template.

This repo uses GitHub workflows to add CI/CD for a PowerToys plugin project. All pull requests get validated
by building for x64 and ARM64. Releases are automated as well and the plugin gets the version of the GitHub
release. See the `.github/workflows` folder for the workflows that take care of this.
