using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;
using System.Media;

namespace Snake
{
    //Philip - The code below is to make a structure that holds the row's variable and column's variable as global to be used
    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int lastFoodTime = 0;
            int foodDissapearTime = 16000;
            int negativePoints = 0;
            int life = 3;

            Console.WriteLine("Please enter your name:");
            string name = Console.ReadLine();
            Console.Clear();

            //Background music 
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Waltz-music-loop.wav";
            player.PlayLooping();

            //add menu
            /*Menu m = new Menu();
            m.menu();*/
            menu();
            Console.Clear();

            //max - Creates an array that has four directions
            Position[] directions = new Position[]
            {
                new Position(0, 1), // right
                new Position(0, -1), // left
                new Position(1, 0), // down
                new Position(-1, 0), // up
            };

            //max - Sets the time to 100 milliseconds
            double sleepTime = 100;
            //max - Sets the direction of the snake
            int direction = right;
            //max - Randomly generate a number
            Random randomNumbersGenerator = new Random();
            //max - Set the height of the console
            Console.BufferHeight = Console.WindowHeight;
            //max - Set the time for the lastFoodTime
            lastFoodTime = Environment.TickCount;

            //philip - This List is to list out where would the obstacles will be appearing in the game by using X, Y Coordinator
            List<Position> obstacles = new List<Position>();

            for (int i = 0; i < 5; i++)
            {
                obstacles.Add(new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            }

            //max - Setting the color, position and the 'symbol' (which is '=') of the obstacle.
            foreach (Position obstacle in obstacles)
            {
                SetObstacle(obstacle);
            }

            //ben - create 5 bodies of the snake 
            Queue<Position> snakeElements = new Queue<Position>();
            for (int i = 5; i <= 8; i++)
            {
                snakeElements.Enqueue(new Position(0, i));
            }

            //Philip - This part of code is to randomly spawn the food to any row and col, 
            //while the food is eaten by snake or spawn at the obstacles' or snake's position, it will respawn again.
            Position food;
            do
            {
                food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            SetFood();

            //max - Setting the color, position and the 'symbol' (which is '*') of the snakeElements.
            foreach (Position position in snakeElements)
            {
                SetSnakeElement(position);
            }

            //ben - create an infinite loop for user input to change the direction
            while (true)
            {
                negativePoints++;

                int userPoint = (snakeElements.Count - 4) * 100 - negativePoints;
                if (userPoint < 0) userPoint = 0;
                userPoint = Math.Max(userPoint, 0);
                Console.SetCursorPosition(0, 1);
                Console.WriteLine("Lifes:{0}", life);
                Console.SetCursorPosition(0, 0);
                Console.Write("Score:{0}", userPoint);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userInput = Console.ReadKey();
                    if (userInput.Key == ConsoleKey.LeftArrow)
                    {
                        if (direction != right) direction = left;
                    }
                    if (userInput.Key == ConsoleKey.RightArrow)
                    {
                        if (direction != left) direction = right;
                    }
                    if (userInput.Key == ConsoleKey.UpArrow)
                    {
                        if (direction != down) direction = up;
                    }
                    if (userInput.Key == ConsoleKey.DownArrow)
                    {
                        if (direction != up) direction = down;
                    }
                }

                //philip - Snakeelements' last array number will be the snakeHead's position.
                Position snakeHead = snakeElements.Last();
                //nextDirection's value will be the direction's that is input by the user.
                Position nextDirection = directions[direction];
                //snakeNewHead will be using the if statement at line after 122 to calculate and get the result.
                Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                    snakeHead.col + nextDirection.col);

                //max - allows the snake to appear at the bottom when the snake moves out of the top border vertically
                if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                //max - allows the snake to appear on the right side when the snake moves out of the left border horizontally
                if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;
                //max - allows the snake to appear on the left side when the snake moves out of the right border horizontally
                if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;
                //max - allows the snake to appear at the top when the snake moves out of the bottom border vertically
                if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;


                //ben - if snake head is collide with the body, show the word "Game over!" and show the points
                if (obstacles.Contains(snakeNewHead))
                {
                    life--;
                    // add life
                    if (life != 0)
                    {
                        negativePoints += 50;
                        //everytime the snake consume an obstacle this function will add another new one
                        foreach (Position obstacle in obstacles.ToList())
                        {
                            if (obstacle.col == snakeNewHead.col && obstacle.row == snakeNewHead.row)
                            {
                                obstacles.Remove(obstacle);
                            }
                        }

                        AddNewObstacle();
                    }

                    else
                    {
                        Console.SetCursorPosition(0, 1);
                        Console.WriteLine("Lifes:{0}", life);
                        Lose();
                        return;
                    }
                }

                // if snake eat his body, minus 1 live, decrease 1 element, and decrease the score
                if (snakeElements.Contains(snakeNewHead))
                {
                    life--;
                    if (life != 0)
                    {
                        negativePoints += 50;
                        Position last = snakeElements.Dequeue();
                        Console.SetCursorPosition(last.col, last.row);
                        Console.Write(" ");
                    }

                    else
                    {

                        Console.SetCursorPosition(0, 1);
                        Console.WriteLine("Lifes:{0}", life);
                        Lose();
                        return;
                    }
                }

                //philip - Base on where the snakehead's position,produce gray color * for the snake body
                Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");

                //max - Add the 'snakeNewHead' to the queue
                snakeElements.Enqueue(snakeNewHead);
                //max - Set the position of the snakeNewHead
                Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                //max - set the color of the snake head
                Console.ForegroundColor = ConsoleColor.Gray;
                //max - controls the direction of the snake
                if (direction == right) Console.Write(">");
                if (direction == left) Console.Write("<");
                if (direction == up) Console.Write("^");
                if (direction == down) Console.Write("v");

                //ben - if snake head reached the food, the snake elements increase by 1 and add a new food and an obstacle.
                if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                {
                    //Soundeffect added.
                    SystemSounds.Beep.Play();
                    // feeding the snake
                    AddNewFood();
                    // get the lastFoodTime (in Millisecond)
                    lastFoodTime = Environment.TickCount;

                    SetFood();
                    sleepTime--;
                    AddNewObstacle();
                }
                else
                {
                    // moving...
                    //remove the last element of the snake elements and return it to the begining
                    Position last = snakeElements.Dequeue();
                    Console.SetCursorPosition(last.col, last.row);
                    //replace the last element with blank
                    Console.Write(" ");
                }
                //philip - This if statement is to reposition the food's position 
                //from its last location if the tickcount is more than the foodDissapearTime
                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    negativePoints = negativePoints + 50;
                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");
                    AddNewFood();
                    lastFoodTime = Environment.TickCount;
                }

                SetFood();

                //Add winning requirement
                if (snakeElements.Count == 30)
                {
                    Win();              
                    return;
                }

                sleepTime -= 0.01;
                Thread.Sleep((int)sleepTime);


            }

            // set the food postion,color,icon.
            void SetFood()
            {
                Console.SetCursorPosition(food.col, food.row);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("@");
            };

            // set the obstacle position,color,icon.
            void SetObstacle(Position obstacle)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                Console.Write("=");
            }

            //set the snake element postion,color,icon
            void SetSnakeElement(Position position)
            {
                Console.SetCursorPosition(position.col, position.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");
            }

            //lose
            void Lose()
            {
                int y = Console.WindowHeight / 2;
                string text1 = "Game over!";
                string text2 = "Your points are: ";
                //string text3 = "Press any key to view the leaderboard";

                int text1length = text1.Length;
                int text2length = text2.Length;
                //int text3length = text3.Length;

                int text1start = (Console.WindowWidth - text1length) / 2;
                int text2start = (Console.WindowWidth - text2length) / 2;
                //int text3start = (Console.WindowWidth - text3length) / 2;

                //Set Game over to middle of the window
                Console.SetCursorPosition(text1start,y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(text1);
            
				
                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;
                if (userPoints < 0) userPoints = 0;

                //Set Score to middle of the window
                Console.SetCursorPosition(text2start, y+1);
                userPoints = Math.Max(userPoints, 0);
                Console.WriteLine("{0}{1}", text2, userPoints);

                //Add player score into plain text file.
                StreamWriter snakeFile = new StreamWriter("Snake_Score.txt", true);
                snakeFile.Write(name + "\n");
                snakeFile.Write(userPoints+"\n");
                snakeFile.Close();

                //Set instruction to middle of window
                //Console.SetCursorPosition(text3start, y+2);
                //Console.WriteLine(text3);

                /*Console.ReadLine();
                ShowLeaderBoard();*/
                menu();
            }

            void Win()
            {
                int y = Console.WindowHeight / 2;
                string text1 = "You Win!!!!";
                string text2 = "Your points are: ";
                //string text3 = "Press any key to view the leaderboard";

                int text1length = text1.Length;
                int text2length = text2.Length;
                //int text3length = text3.Length;

                int text1start = (Console.WindowWidth - text1length) / 2;
                int text2start = (Console.WindowWidth - text2length) / 2;
                //int text3start = (Console.WindowWidth - text3length) / 2;

                Console.SetCursorPosition(text1start, y);
                Console.WriteLine(text1);

                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;
                if (userPoints < 0) userPoints = 0;

                //Set Score to middle of the window
                Console.SetCursorPosition(text2start, y+1);
                userPoints = Math.Max(userPoints, 0);
                Console.WriteLine("{0}{1}", text2, userPoints);

                //Add player score into plain text file.
                StreamWriter snakeFile = new StreamWriter("Snake_Score.txt", true);
                snakeFile.Write(name + "\n");
                snakeFile.Write(userPoints + "\n");
                snakeFile.Close();

                //Set instruction to middle of window
                //Console.SetCursorPosition(text3start, y+2);
                //Console.WriteLine(text3);

                /*Console.ReadLine();
                ShowLeaderBoard();*/
                menu();

            }

            void AddNewObstacle()
            {
                Position obstacle = new Position();
                do
                {
                    obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(obstacle) ||obstacles.Contains(obstacle) ||(food.row != obstacle.row && food.col != obstacle.row));
                obstacles.Add(obstacle);
                SetObstacle(obstacle);
            }

            void AddNewFood()
            {
                do
                {
                    food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(food) || obstacles.Contains(food));
            }

            //display leaderboard
            void ShowLeaderBoard()
            {
                player.Stop();
                Console.Clear();

                string line;

                List<string> playerlist = new List<string>();
                List<int> scorelist = new List<int>();
                List<string> playerlist2 = new List<string>();
                List<int> scorelist2 = new List<int>();

                int z = 1;

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("");
                }

                Console.ForegroundColor = ConsoleColor.White;

                System.IO.StreamReader file = new System.IO.StreamReader("Snake_Score.txt");

                int counter = 0;
                int index = 0;

                while ((line = file.ReadLine()) != null)
                {
                    if (counter % 2 == 0)
                    {
                        playerlist.Add(line);
                    }

                    else
                    {
                        scorelist.Add(Int32.Parse(line));
                    }

                    counter++;
                }

                int length = playerlist.Count();

                // find top 10 highest
                for (int i = 0; i < length; i++)
                {
                    int highest = scorelist[0];
                    index = 0;

                    for (int q = 0; q < scorelist.Count(); q++)
                    {
                        if (scorelist[q] > highest)
                        {
                            highest = scorelist[q];
                            index = q;
                        }
                    }

                    playerlist2.Add(playerlist[index]);
                    scorelist2.Add(scorelist[index]);
                    playerlist.RemoveAt(index);
                    scorelist.RemoveAt(index);
                }

                if (length > 10)
                {
                    length = 10;
                }

                //display the leaderboard
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Leaderboard" + "\n");
                Console.WriteLine("  " + "Player" + "     " + "Score");
                Console.WriteLine("  " + "==========" + " " + "===========");
                for (int i = 0; i < length; i++)
                {
                    Console.WriteLine(z + "." + playerlist2[i] + "\t" + "\t" + scorelist2[i]);
                    z++;
                }
                Console.WriteLine("Press enter to exit");
                file.Close();
                Console.Read();
                return;
            }

            void menu()
            {
                string userOption;
                string condition = "correct";
                do
                {
                    int ystart = (Console.WindowHeight -2 ) / 2;
                    string text1 = "Welcome to the Snake Menu. Please choose an option below:";
                    string text2 = "\t\t\t(1) Play Again\t(2) View Leaderboard\t(3) Help\t(4) Quit Game";

                    int text1length = text1.Length;
                    int text2length = text2.Length;

                    int text1start = (Console.WindowWidth - text1length) / 2;
                    int text2start = (Console.WindowWidth - text2length) / 2;

                    //Set menu to middle of the window
                    Console.SetCursorPosition(text1start, ystart);
                    Console.SetCursorPosition(text2start, ystart+1);
                    Console.WriteLine(text1);
                    Console.WriteLine(text2);

                    userOption = Console.ReadLine();

                    switch (userOption)
                    {
                        case "1":
                            Console.WriteLine("You have chosen option " + userOption + " -> Play the game again");
                            condition = "correct";
                            //Program prgm = new Program();
                            break;
                        case "2":
                            Console.WriteLine("You have chosen option " + userOption + " -> View Leaderboard");
                            condition = "correct";
                            ShowLeaderBoard();
                            break;
                        case "3":
                            Console.WriteLine("You have chosen option " + userOption + " -> View Help Page");
                            condition = "correct";
                            //Add in help method
                            break;
                        case "4":
                            Console.WriteLine("You have chosen option" + userOption + " -> Exit the game");
                            condition = "correct";
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid user input. Please try again.\n");
                            condition = "incorrect";
                            break;
                    }
                } while (condition == "incorrect");
            }
        }
    }
}