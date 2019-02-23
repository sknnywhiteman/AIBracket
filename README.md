# AIBracket

This is a live document. Anything on this page is subject to change, and should be kept up to date with the current version of the application.

## Project Architecture

![Project Architecture](https://github.com/sknnywhiteman/AIBracket/blob/master/Images/Architecture.png "Project Architecture")

## Game Ideas:  
 * Pacman
   * Can Program Ghost or Pacman


## Architecture:
  * Central Database (sqlite) 
  * Data project (Interfacing with database)
  * Game project (handles all games and logic)
  * Web project (displays live games, data viewing)
  * API project (interfaces clients with game project)
  * Everything written in C#, web written in angular

## Web project:
  * Angular 7
  * Handles authentication
  * Can select between different games (pacman, etc.)
  * Displays live matches
  * Displays leaderboards
  * Displays statistics of AI
  * Search players/AIs
  * Players sign up to receive a private key
  * dotnet core serverside which communicates with common logic

## API project:
  * Validation
  * Provides a persistent client state
  * TCP socket for client communications

## Common Logic:
  * Computes games
  * Interfaces with web project to display games to users
  * Interfaces with API project to communicate actions and gamestate to users
  * Interfaces with data project to save data and log
  * Stores live game data in memory for web project, when finished store in DB 

## Data project:
  * Generates private keys for AI
  * Interfaces directly with database
  * Contains controllers for modifying player data

## Database:
 * Users
    * Username
    * UID
    * Password
	* First Name
	* Last Name
	* Email
	* Phone Number
 * AI
    * Name
    * ID
    * UID
    * Private Key
    * Associated Game
    * Created date
    * Last connected
 * Games (one table for each game, this is pacman example)
    * Player IDs
    * Game History??
    * Outcome (score of pacman)
    * Time elapsed
    * Time created

## Setup
1. Download [dotnet core 2.2](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.103-windows-x64-installer)
2. Download [NodeJS](https://nodejs.org/en/)
3. Download [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (You also probably want SSMS)
4. Configure AIBracket.Web appsettings.json with correct connection string to connect to the database you created (leave default if unsure)
5. Run `dotnet ef update` from console in AIBracket.Web directory to seed database