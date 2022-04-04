# BountyBot [![Build Status](https://dev.azure.com/WordlessSafe1/BountyBot/_apis/build/status/WordlessSafe1.BountyBot?branchName=master)](https://dev.azure.com/WordlessSafe1/BountyBot/_build/latest?definitionId=6&branchName=master)
This is a simple Discord bot created to handle multiple tasks autonomously in the POPULATION: ONE Bounty Hunters Discord server, primarily the management of bounties.

### Usage
In order to run this bot, a valid Discord Bot token is required. It should be placed in a file titled `token.tok` and located in the executable's directory.

Because of the small target audience, this bot does not utilize Discord's global command registration system.
Instead, commands are registered to individual servers by using their Guild Id. 
To declare which servers commands are deployed to, the Guild Ids should be placed on separate lines in a file titled `servers.cfg` and located in the executable's directory. 
Additionally, you can declare a 'deployment' guild by following the Id with `, true`. 
This will prevent the bot from registering commands to these servers when launched with the `DEBUG` argument. 
