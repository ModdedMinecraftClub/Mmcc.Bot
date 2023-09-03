# Mmcc.Bot
Minecraft servers network management system disguised as a Discord bot.

The bot is tightly integrated with [polychat2](https://github.com/ModdedMinecraftClub/polychat2) which is a Forge/Bukkit server-side Minecraft mod needed for the incoming messages to be understood and handled correctly.

## Available features

To view all the commands, simply type `!help` in a server that has the bot, once it is running.

### Minecraft servers integration

The bot integrates with Minecraft servers, providing the ability to view online servers and information about them (IP, name, amount of online players), as well as the ability for staff to perform administrative tasks such as restarting the servers and executing commands on them from Discord.

### Member applications

The bot automatically detects applications for the [Member role](https://wiki.moddedminecraft.club/index.php?title=How_to_earn_the_Member_rank) posted to the #member-apps Discord channel and allows Staff to manage them (approve/reject), notifying the players of the outcome.

### Moderation

The bot synchronises Discord moderation with Minecraft moderation, allowing for bans/warns to be issued on Discord and on all Minecraft servers simultanously.

### Tags

Tags system which allows moderators to add tags which are available via a command. Useful for adding answers to FAQs etc.

### Diagnostics

Comamnds that make it easier for Staff to diagnose problems so that they do not have to SSH in, for simple tasks such as checking the available SSD space on the dedicated server (a common issue).

## Planned features

- Issues
- Mutes

## Deployment

The bot has two deployment methods:
- Docker
- Native (directly on the host machine)

### Common prerequisites (for both Docker and native)

1. Set up [polychat2](https://github.com/ModdedMinecraftClub/polychat2). Instructions for how to do so can be found in its [Quickstart tutorial](https://github.com/ModdedMinecraftClub/polychat2/blob/master/README.md#quickstart). **IMPORTANT: ONLY SET UP THE CLIENTS, SERVER IS NOT NEEDED AND WILL INTERFERE WITH `Mmcc.Bot` IF USED**.

2. Clone this repository. From now on the root of the cloned repository will be refered to as `./`

3. Initialise the submodules by running `git submodule update --init --recursive` in `./`.

### Docker deployment

1. Install [Docker](https://www.docker.com/).

2. Go to `./env`. If the folder does not exist, create it. Create a file called `mariadb.env`, with the following contents:
```env
MARIADB_ROOT_PASSWORD=placeholder
MARIADB_DATABASE=placeholder
MARIADB_USER=placeholder
MARIADB_PASSWORD=placeholder
```
Replace the placeholder with whatever values you want.

*If you prefer setting environment variables in a different way, you can see all the available options docker has in the [docker-compose manual](https://docs.docker.com/compose/).*

3. Go to `.src/Mmcc.Bot`. Locate the file called `appsettings.default.json` and rename it to `appsettings.json`. Open it and fill it in. **IMPORTANT: In the `MySql` section keep the IP set to `db` and port to `3306`.**

4. Run `docker compose up` in `./`.


### Native deployment via building from source

1. Install [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)

2. Install [Entity Framework Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools)

3. Install [MariaDB](https://mariadb.com/kb/en/getting-installing-and-upgrading-mariadb/) or MySQL and start it. We strongly recommend MariaDB.

4. Create an empty database for the bot.

5. Create a user for the bot with all the necessary permissions (creating/deleting tables, read/write to the tables) in that database.

6. Go to `.src/Mmcc.Bot`. Locate the file called `appsettings.default.json` and rename it to `appsettings.json`. Open it and fill it in. Remember to change the IP in the `MySql` section to `localhost`.

7. Duplicate the file and name the duplicate `appsettings.Development.json`.

8. Open a terminal tab/window in the `./src/Mmcc.Bot.Database` directory and run `dotnet ef database update`. This will apply all the necessary database migrations to the database you have specified in `appsettings.Development.json`.

9. Open a terminal tab/window in `./`. Run `dotnet publish ./src/Mmcc.Bot/Mmcc.Bot.csproj -c Release -o ./out`.

10. Start the bot by opening a terminal window/tab in `./out` and running `dotnet Mmcc.Bot.dll`.