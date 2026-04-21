# Overview
Discord Event Manager is a bot created to manage events within Discord guilds. This bot aims to streamline the user experience for events within Discord, providing automatic interest tracking, calendar integration and availability polling.

# Features
## Event tracking
The bot tracks all events that are registered within all the guilds it has been added to.
When an event is registered, the following actions are done:
1) A message will be sent to the configured guild event channel
2) A thread will be started from this message, this thread can be used to discuss the event
3) A link to the created thread will be added to the event description to make it easy to get to the thread from the event menu

When an event is edited the bot will register the changes, but it will not take any additional action.

When an event is canceled or completed, the bot will either repeat the event if configured, or delete the message in the event channel.

## User interest tracking
When a user sets interested on an event, they are automatically added to the event thread. 
When they set themselves to not interested, they are removed from the event thread. 
After an event repetition, the bot keeps the user as interested, even though they have not set themselves interested in the new event.

To see which events a user is interested in, the command `/get-user-interested ` can be used. By default this shows which events the current user is interested, but optionally another user can be provided through a parameter.<br>
To see which users are interested in an event, the command `/get-interested-for-event event_id:<event>` can be used.

## Availability polling
To determine availability of all interested users, the bot is able to post a list of dates which the users can react on using emoji reactions (✅, 〽️, ❌). 
Based on the reactions from the user the bot will add a colored box reaction (🟩, 🟨, 🟧) indicating how many people are available on a given day.

To use this feature, the command `/generate-dates datefrom:<dd-MM-yyyy> dateuntil:<dd-MM-yyyy> days:<daysOfWeekPattern>` can be used. 

Within this command a startdate and an enddate can be provided, these parameters also contain an autocompletion system to provide sensible suggestions to the user.
The days parameter can be used to set which days of the week should be generated for (e.g. _____00 is Saturday/Sunday and \_0\_\_\_0\_ is Tuesday/Saturday), by default the options All, Weekdays, Weekends, and Long weekends will be provided. 

## Event repetition
For some events it's desirable to make it recurring, this can also be configured within the bot.
To configure this for an event, the command `/set-repeat event_id:<event> forward_time:<days>` can be used. <br>
To disable event repetition use the command `/disable-repeat event_id:<event>`.

When an event is repeated, it is created as an exact copy of the previous event occurence. The name is prefixed with `[AUTOREPEAT] ` to clearly indicate that the repeated event has not been edited by a person yet.
When the event is edited, the `[AUTOREPEAT]` tag will be automatically removed.

Additionally it is also possible to automatically start availability polling when the event is repeated.
This can be configured using the command `/set-date-generation event_id:<event> start_from:<dayofweek> days:<daysOfWeekPattern> number_of_weeks:<weeks>`. 
In this command a day of the week can be configured, date generation will start from the first occurence of that day after the new event date. Dates will be generated for the specified days of the week and for the specified number of weeks.<br>
Use the command `/disable-date-generation event_id:<event>` to turn date generation off.

#### Additional commands
`/get-all-repeat` Show repeat information for all events in guild <br>
`/get-repeat event_id:<event>` Show repeat information for selected event

## Thread refreshing
If a thread is inactive too long, Discord will automatically hide the thread from users channel list. This is not always desirable, so the bot is able to automatically refresh threads on a specific time of day.
This is done by first archiving all active threads in the event channel. Then all the threads for events that are still active will be unarchived, thus resetting their cooldown timer.
This feature can be enabled with the command `/set-guild-threadrefreshtime refresh_time:<HH:mm>` within Discord.
It can be turned off again using `/disable-guild-threadrefresh `.

Additionally it is possible to configure how long threads should be kept alive after an event has ended.
This can be done using the command `/set-guild-threadkeepalivetime keep_alive_time:<days>`. 
By default this will be set to 0, so once an event is finished the thread will not be restored once it is archived.

## Calendar integration
The bot is able to create calendars for guilds or users, taking into account which events exist for the guild and which ones the user is interested in.
Automatically repeated events are ignored until they have been edited by a user.
Users are able to get links to the calendar using the `/get-server-calendar` or `/get-user-calendar` commands within Discord. The link that is provided should be configured within appsettings.json.

# Used libraries
* [Entity Framework](https://learn.microsoft.com/en-us/aspnet/entity-framework)
* [MySQL Database](https://www.mysql.com/)
* [Pomelo Entity Framework MySQL](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
* [Netcord C# Discord library](https://github.com/NetCordDev/NetCord).

# Installation
1) Clone this repository
2) Ensure dotnet 9 is installed
3) Create a MySQL database 
4) Fill the necessary fields in appsettings.json, [appsettings_template.json](https://github.com/Ricorvs/DiscordEventManager/blob/d9372fd7aeb77daab971d8539713a701edf0a1a8/EventManager/appsettings_template.json) can be used as a starting point. 
   It's important to fill at least the Discord token and MySQL connection string.
5) Build the project `dotnet build`
6) Run `dotnet migrations update database`
7) Run the bot `dotnet run`

For basic functionality a channel should be configured that can be used for posting event threads, this can be done using the following command with Discord:
`/set-guild-eventchannel channel:<chosen_channel>`
