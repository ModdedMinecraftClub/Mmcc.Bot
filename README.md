# Mmcc.Bot
Discord bot to fulfill all of MMCC's needs - communication with servers, diagnostics, applications, moderation, issues.

The bot is tightly integrated with [polychat2](https://github.com/ModdedMinecraftClub/polychat2) which it uses to communicate with Minecraft servers. It implements its protocol via [Ssmp](https://github.com/john01dav/ssmp).

## Available features

To view all the commands, simply type `!help` in a server that has the bot, once it is running.

### Minecraft servers integration

The bot integrates with MMCC Minecraft servers, providing the ability to view online servers and information about them (IP, name, amount of online players), as well as the ability for staff to perform administrative tasks such as restarting the servers and executing commands on them from Discord.

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

2. Install [.NET 5 SDK](https://dotnet.microsoft.com/download).

3. Install [Entity Framework Core 5 Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet).

4. Clone this repository. From now on the root of the cloned repository will be refered to as `./`

5. Initialise the protos submodule by running `git submodule update --init --recursive` in `./`.

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

3. Go to `.src/Mmcc.Bot`. Locate the file called `appsettings.default.json` and rename it to `appsettings.json`. Open it and fill it. In the `MySql` section keep the IP set to `db` and port to `3306`.

4. Duplicate the file and name the duplicate `appsettings.Development.json` but this time set the IP in the `MySql` section to `localhost`. Leave the rest the same.

5. Start the MariaDB docker container by running `docker compose up db` in the `./` directory.

6. Open another terminal tab/window in the `./src/Mmcc.Bot.Database` directory and run `dotnet ef database update`. This will apply all the necessary database migrations.

7. Go back to the terminal running `docker compose up db` and stop it by pressing `CTRL + C`.

8. Run the bot, db and phpmyadmin for the db via `docker compose up`.


### Native deployment

1. Install [MariaDB](https://mariadb.com/kb/en/getting-installing-and-upgrading-mariadb/) or MySQL and start it. We strongly recommend MariaDB.

2. Create an empty database for the bot.

3. Create a user for the bot with all the necessary permissions (creating/deleting tables, read/write to the tables) in that database.

4. Go to `.src/Mmcc.Bot`. Locate the file called `appsettings.default.json` and rename it to `appsettings.json`. Open it and fill it in. Remember to change the IP in the `MySql` section to `localhost`.

5. Duplicate the file and name the duplicate `appsettings.Development.json`.

6. Open a terminal tab/window in the `./src/Mmcc.Bot.Database` directory and run `dotnet ef database update`. This will apply all the necessary database migrations to the database you have specified in `appsettings.Development.json`.

7. Open a terminal tab/window in `./`. Run `dotnet publish ./src/Mmcc.Bot/Mmcc.Bot.csproj -c Release -o ./out`.

8. Start the bot by opening a terminal window/tab in `./out` and running `dotnet Mmcc.Bot.dll`.