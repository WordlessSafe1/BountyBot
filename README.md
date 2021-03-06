# BountyBot [![Build Status](https://dev.azure.com/WordlessSafe1/BountyBot/_apis/build/status/WordlessSafe1.BountyBot?branchName=master)](https://dev.azure.com/WordlessSafe1/BountyBot/_build/latest?definitionId=6&branchName=master) [![Build Status](https://vsrm.dev.azure.com/WordlessSafe1/_apis/public/Release/badge/9fda80a0-dfbb-4431-9288-bf3cac0411ba/1/1)](https://dev.azure.com/WordlessSafe1/BountyBot/_build/latest?definitionId=6&branchName=master)
This is a simple Discord bot created to handle multiple tasks autonomously in the POPULATION: ONE Bounty Hunters Discord server, primarily the management of bounties.

## Usage
### Token
In order to run this bot, a valid Discord Bot token is required. It should be placed in a file titled `token.tok` and located in the executable's directory.

### Servers
Because of the small target audience, this bot does not utilize Discord's global command registration system.
Instead, commands are registered to individual servers by using their Guild Id. 
To declare which servers commands are deployed to, the Guild Ids should be placed on separate lines in a file titled `servers.cfg` and located in the executable's directory. 
Additionally, you can declare a 'deployment' guild by following the Id with `, true`. 
This will prevent the bot from registering commands to these servers when launched with the `DEBUG` argument. 

### Data Storage
This bot interfaces with a PostGreSQL server, either locally or remotely installed. In order to run this bot, you must have access to one, and know the Host Name, Port, Database, username, and Password. These will be fed to the program by creating a psql.conf file, using the Npgsql 'Connection string' format. For example: `Host=myserver;Port=5432;Username=mylogin;Password=mypass;Database=mydatabase`.
