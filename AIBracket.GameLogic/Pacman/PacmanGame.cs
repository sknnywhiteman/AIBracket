﻿using AIBracket.GameLogic.Pacman.Board;
using AIBracket.GameLogic.Pacman.Pacman;
using AIBracket.GameLogic.Pacman.Ghost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIBracket.GameLogic.Pacman.Coordinate;

namespace AIBracket.GameLogic.Pacman.Game
{
   
    public class PacmanGame
    {
        private PacmanBoard board;
        private PacmanPacman pacman;
        private PacmanGhost[] ghosts;
        public  DateTime TimeStarted { get; private set; }
        public DateTime TimeEnded { get; private set; }
        private int score;
        private int SpawnGhostCounter, PoweredUpCounter;
        public bool GameRunning { get; private set; }

        public PacmanGame()
        {
            PacmanPacman.Lives = 3;
            board = new PacmanBoard();
            pacman = new PacmanPacman();
            ghosts = new PacmanGhost[4]
            {
                new PacmanGhost(),
                new PacmanGhost(),
                new PacmanGhost(),
                new PacmanGhost()
            };
            score = 0;
            SpawnGhostCounter = 3;
            PoweredUpCounter = 0;
            TimeStarted = DateTime.Now;
            GameRunning = true;
        }

        /// <summary>
        /// Updates score based on tiles that pacman enters
        /// </summary>
        /// <param name="t">Tile entered by pacman</param>
        private void UpdateScore(PacmanBoard.Tile t)
        {
            if(t == PacmanBoard.Tile.dot)
            {
                score += 1;
            }
            else if(t == PacmanBoard.Tile.fruit)
            {
                score += 2;
            }
            else if(t == PacmanBoard.Tile.powerUp)
            {
                score += 3;
            }
            return;
        }

        /// <summary>
        /// Checks the result of executing a direction based on the position of an entity
        /// </summary>
        /// <param name="d">Direction entity is trying to move towards</param>
        /// <param name="pos">Position of entity currently</param>
        /// <returns>Whether a move is valid</returns>
        public bool ValidMove(PacmanPacman.Direction d, PacmanCoordinate pos)
        {
            switch(d)
            {
                case PacmanPacman.Direction.start:
                    return true;
                case PacmanPacman.Direction.up:
                    return board.GetTile(pos.Xpos, pos.Ypos - 1) != PacmanBoard.Tile.wall;
                case PacmanPacman.Direction.down:
                    return board.GetTile(pos.Xpos, pos.Ypos + 1) != PacmanBoard.Tile.wall;
                case PacmanPacman.Direction.left:
                    return board.GetTile(pos.Xpos - 1, pos.Ypos) != PacmanBoard.Tile.wall;
                case PacmanPacman.Direction.right:
                    return board.GetTile(pos.Xpos + 1, pos.Ypos) != PacmanBoard.Tile.wall;
            }
            return false;
        }

        /// <summary> This method is called after a move is made by pacman to determine whether or not to update the board.
        /// It also calls update score if a fruit or dot was consumed </summary>
        public void CheckTile(PacmanCoordinate pos) 
        {
            switch (board.GetTile(pos))
            {
                case PacmanBoard.Tile.portal:
                    pacman.Location = board.GetCorrespondingPortal(pos);
                    if(pacman.Facing == PacmanPacman.Direction.left)
                    {
                        pacman.Facing = PacmanPacman.Direction.right;
                    }
                    else
                    {
                        pacman.Facing = PacmanPacman.Direction.left;
                    }
                    break;
                case PacmanBoard.Tile.powerUp:
                    foreach (var g in ghosts)
                    {
                        if(!g.IsVulnerable && !g.IsDead)
                        {
                            g.IsVulnerable = true;
                            PoweredUpCounter = 20;
                        }
                    }
                    UpdateScore(board.GetTile(pos));
                    break;
                case PacmanBoard.Tile.dot:
                case PacmanBoard.Tile.fruit:
                    board.UpdateTile(pos);
                    UpdateScore(board.GetTile(pos));
                    break;
                default:
                    break;
            }
            return;
        }

        public void SpawnGhost()
        {
            if(SpawnGhostCounter == 0)
            {
                foreach (var g in ghosts)
                {
                    if(g.IsDead == true)
                    {
                        g.IsDead = false;
                        SpawnGhostCounter = 10;
                        return;
                    }
                }
            }
            SpawnGhostCounter--;
        }

        /// <summary>
        /// Checks whether a ghost has collided with pacman and handles resetting positions after death
        /// </summary>
        public void PacmanGhostCollide()
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (ghosts[i].GetPosition() == pacman.GetPosition())
                {
                    if (ghosts[i].IsVulnerable)
                    {
                        ghosts[i].IsDead = true;
                        ghosts[i].IsVulnerable = false;
                        ghosts[i].Location.Xpos = 13;
                        ghosts[i].Location.Ypos = 11;
                        score += 5;
                    }
                    else
                    {
                        if(PacmanPacman.Lives == 0)
                        {
                            GameRunning = false;
                        }
                        else
                        {
                            pacman.Location.Xpos = 13;
                            pacman.Location.Ypos = 17;
                            PacmanPacman.Lives--;
                        }
                    }
                }
            }
        }
        
        /// <summary>Processes every tick of the game base on directions of each entity passed in the array
        /// p should be passed 5 directions
        /// p[0] represents Pacman
        /// p[1] through p[4] represent ghosts in order passed </summary>
        
        public void UpdateGame(PacmanPacman.Direction[] p)
        {
            if (p.Count() != 5)
            {
                Console.Error.WriteLine("Error: UpdateGame passed array of {0} length", p.Count());
                return;
            }

            SpawnGhost();

            // Move Pacman
            if(ValidMove(p[0], pacman.GetPosition()))
            {
                pacman.Facing = p[0];
            }
            if(ValidMove(pacman.Facing, pacman.GetPosition()))
            {
                pacman.Move();
                CheckTile(pacman.GetPosition());
            }            

            // Move ghosts
            for(int i = 0; i < ghosts.Length; i++)
            {
                if(ValidMove(p[i + 1], ghosts[i].GetPosition()))
                {
                    ghosts[i].Facing = p[i + 1];
                }
                if(ValidMove(ghosts[i].Facing, ghosts[i].GetPosition()))
                {
                    ghosts[i].Move();
                }
            }

            PacmanGhostCollide();

            if (PoweredUpCounter == 0)
            {
                foreach (var g in ghosts)
                {
                    g.IsVulnerable = false;
                }
            }
            PoweredUpCounter--;
        }

        public void PrintBoard()
        {
            char tile;
            for (int i = 0; i < 31; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    if (pacman.Location.Xpos == j && pacman.Location.Ypos == i)
                    {
                        tile = '<';
                        Console.Write("{0} ", tile);
                        continue;
                    }/*
                    else
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (ghosts[k].Location.Xpos == j && ghosts[k].Location.Ypos == i)
                            {
                                tile = '~';
                                Console.Write("{0} ", tile);
                                continue;
                            }
                        }
                    }*/
                    switch (board.GetTile(j, i))
                    {
                        case PacmanBoard.Tile.blank:
                            tile = ' ';
                            break;
                        case PacmanBoard.Tile.dot:
                            tile = '.';
                            break;
                        case PacmanBoard.Tile.fruit:
                            tile = 'F';
                            break;
                        case PacmanBoard.Tile.powerUp:
                            tile = '!';
                            break;
                        case PacmanBoard.Tile.wall:
                            tile = 'X';
                            break;
                        default:
                            tile = 'F';
                            break;
                    }
                    Console.Write("{0} ", tile);
                }
                Console.Write("\n");
            }
        }
    }
}
