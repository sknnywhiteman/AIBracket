﻿using AIBracket.API.Entities;
using AIBracket.API.Sockets;
using AIBracket.GameLogic.Pacman.Game;
using AIBracket.GameLogic.Pacman.Pacman;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using AIBracket.Data;
using AIBracket.Data.Entities;

namespace AIBracket.API
{
    public static class GameMaster
    {
        private static List<GamePacman> games; // current games running (playing atm)
        private static List<IConnectedClient> waiting_players; // current connected players
        private static List<ISocket> spectators; 
        private static bool isRunning;
        private static AIBracketContext context;
        private static object lockobj = new object();

        public static void Initialize() { // constructor for our games list and players
            games = new List<GamePacman>(); // <- create functions to add players and games
            waiting_players = new List<IConnectedClient>();
            isRunning = false;
            spectators = new List<ISocket>();
            context = new AIBracketContext();
        }

        public static bool WatchGame(ISocket spectator, string guid)
        {
            lock (lockobj)
            {
                var game = games.FirstOrDefault(x => x.Id.ToString() == guid);
                if (game != null)
                {
                    game.AddSpectator(spectator);
                    return true;
                }
            }
            return false;
        }

        public static void AddSpectator(ISocket spectator)
        {
            lock (lockobj)
            {
                spectators.Add(spectator);
                var ret = "2 ";
                foreach (var g in games)
                {
                    ret += g.Id.ToString() + " 1 " + g.User.Bot.Id.ToString() + " " + g.Game.Score + " ";
                }
                spectator.WriteData(ret.TrimEnd());
            }
        }

        public static bool IsPlayerConnected(IConnectedClient client)
        {
            lock (lockobj)
            {
                return games.Count(x => x.User.Bot.Id == client.Bot.Id) > 0;
            }
        }

        public static void AddPlayer(IConnectedClient player) {
            lock (lockobj)
            {
                if (player.Bot.Game == 1)
                {
                    var game = new GamePacman
                    {
                        Game = new PacmanGame(),
                        User = (PacmanClient)player
                    };
                    games.Add(game);
                    string ret = "0 " + game.Id.ToString() + " 1 " + game.User.Bot.Id.ToString() + " " + game.Game.Score;
                    for (var i = 0; i < spectators.Count; i++)
                    {
                        spectators[i].WriteData(ret);
                        if (!spectators[i].IsConnected)
                        {
                            spectators.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                    Console.WriteLine("Added pacman game");
                    return;
                }
                waiting_players.Add(player);
            }
        }

        public static int GetPlayerCount() {
            lock (lockobj)
            {
                return waiting_players.Count; // for creating a new game
            }
        }

        public static void Run() {
            isRunning = true;
            var start_time = DateTime.UtcNow;
            while (isRunning) {
                lock (lockobj)
                {
                    var current_time = DateTime.UtcNow;
                    var time_elapsed = current_time - start_time;
                    foreach (var g in games)
                    {
                        g.GetUserInput();
                    }
                    if (time_elapsed.TotalMilliseconds >= 500)
                    {
                        foreach (var g in games)
                        {
                            g.UpdateGame();
                        }
                        start_time = current_time;
                    }

                    for (var i = 0; i < games.Count; i++)
                    {
                        if (!games[i].IsRunning && games[i].User.Socket.IsConnected)
                        {
                            context.PacmanGames.Add(new PacmanGames
                            {
                                Id = games[i].Id,
                                BotId = games[i].User.Bot.Id,
                                StartDate = games[i].Game.TimeStarted,
                                EndDate = games[i].Game.TimeEnded,
                                Score = games[i].Game.Score,
                                Difficulty = 1
                            });
                            var ret = "3 " + games[i].Id.ToString() + " ";
                            context.SaveChanges();
                            games[i].Game = new PacmanGame();
                            games[i].Id = Guid.NewGuid();
                            ret += games[i].Id.ToString() + " 1 " + games[i].Game.Score;
                            for (int j = 0; j < spectators.Count; j++)
                            {
                                spectators[j].WriteData(ret);
                                if (!spectators[j].IsConnected)
                                {
                                    spectators.RemoveAt(j);
                                    j--;
                                    continue;
                                }
                            }
                            games[i].IsRunning = true;
                        }
                        else if (!games[i].IsRunning)
                        {
                            var ret = "1 " + games[i].Id.ToString() + " 1";
                            for (var j = 0; j < spectators.Count; j++)
                            {
                                spectators[j].WriteData(ret);
                                if (!spectators[j].IsConnected)
                                {
                                    spectators.RemoveAt(j);
                                    j--;
                                    continue;
                                }
                            }
                            spectators.AddRange(games[i].Spectators);
                            games.RemoveAt(i);
                            i--;
                        }
                    }
                    foreach (var client in waiting_players)
                    {
                        if (client.Socket.IsReady)
                        {
                            var message = client.Socket.ReadData();
                            Console.WriteLine("Game master read: " + message);
                            client.Socket.WriteData("Hello world");

                        }
                    }
                    for (var i = 0; i < spectators.Count; i++)
                    {
                        if (!spectators[i].IsConnected)
                        {
                            spectators.RemoveAt(i);
                            i--;
                            continue;
                        }
                        if (spectators[i].IsReady)
                        {
                            var message = spectators[i].ReadData().Trim();
                            Console.WriteLine("Spectator said " + message);
                            if (message == "LIST GAMES")
                            {
                                spectators[i].WriteData("Pacman: " + games.Count);
                            }
                            else if (message == "LIST PACMAN")
                            {
                                var buffer = "";
                                foreach (var game in games)
                                {
                                    buffer += game.Id.ToString() + " " + game.Game.Score + "\n";
                                }
                                Console.WriteLine("Sending " + buffer);
                                spectators[i].WriteData(buffer);
                            }
                            else if (message.StartsWith("WATCH "))
                            {
                                var guid = message.Substring(6);
                                var game = games.FirstOrDefault(x => x.Id.ToString() == guid);
                                if (game == null)
                                {
                                    spectators[i].WriteData("Failed to find game");
                                }
                                else
                                {
                                    game.AddSpectator(spectators[i]);
                                    spectators.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}
