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
        public static string Difficulty;

       
        static void Main(string[] args)
        {

            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int lastFoodTime = 0;
            int negativePoints = 0;
            int life = 3;
            int userPoint;
            int checkPoint = 200;
            bool gameFinish = false;
    
            //Background music 
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Waltz-music-loop.wav";



            //max - Creates an array that has four directions
            Position[] directions = new Position[]
            {
                new Position(0, 1), // right
                new Position(0, -1), // left
                new Position(1, 0), // down
                new Position(-1, 0), // up
            };


            double sleepTime = 100;
            int direction = right;
            Random randomNumbersGenerator = new Random();
            Console.BufferHeight = Console.WindowHeight;
            lastFoodTime = Environment.TickCount;

            Console.WriteLine("Please enter your name:");
            string name = Console.ReadLine();


            //add menu
            menu();
            Console.Clear();

            int multiplier = Int32.Parse(Difficulty);
            int foodDissapearTime = 18000 - (2800 * multiplier);

            //randomise obstacles
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

            //create body
            Queue<Position> snakeElements = new Queue<Position>();
            for (int i = 18; i <= 21; i++)
            {
                snakeElements.Enqueue(new Position(0, i));
            }

            //randomise food
            Position food;
            do
            {
                food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            SetFood();

            foreach (Position position in snakeElements)
            {
                SetSnakeElement(position);
            }

            //ben - create an infinite loop for user input to change the direction
            while (true)
            {
                negativePoints++;

                userPoint = (snakeElements.Count - 4) * 120 - negativePoints;
                if (userPoint < 0) userPoint = 0;
                userPoint = Math.Max(userPoint, 0);
                Console.SetCursorPosition(0, 0);
                Console.Write("Score:{0}" + "          ", userPoint);
                Console.SetCursorPosition(17, 0);
                Console.Write("|");
                Console.SetCursorPosition(0, 1);
                Console.WriteLine("Lifes:{0}" +"          ", life);
                Console.SetCursorPosition(17, 1);
                Console.Write("|");
                Console.SetCursorPosition(17, 2);
                Console.Write("|");
                Console.SetCursorPosition(0, 2);
                Console.Write("Next life at:{0} ", checkPoint);
                Console.SetCursorPosition(0, 3);
                Console.WriteLine("_________________|");

                if (Console.KeyAvailable)
                {                    
                        ConsoleKeyInfo userInput = Console.ReadKey(true);
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
                if (snakeNewHead.col < 18 && snakeNewHead.row < 4 && direction == left) snakeNewHead.col = Console.WindowWidth - 1;
                if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                //max - allows the snake to appear on the right side when the snake moves out of the left border horizontally
                if (snakeNewHead.col < 18 && snakeNewHead.row < 4 && direction == up) snakeNewHead.row = Console.WindowHeight - 1;
                if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;
                //max - allows the snake to appear on the left side when the snake moves out of the right border horizontally
                if (snakeNewHead.row >= Console.WindowHeight && snakeNewHead.col < 18) snakeNewHead.row = 4;
                if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;
                //max - allows the snake to appear at the top when the snake moves out of the bottom border vertically
                if (snakeNewHead.col >= Console.WindowWidth && snakeNewHead.row < 4) snakeNewHead.col = 18;
                if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;


                //ben - if snake head is collide with the body, decrease 1 life
                if (obstacles.Contains(snakeNewHead))
                {
                    life--;
                    // add life
                    if (life != 0)
                    {
                        negativePoints += 50;
                        //everymultiplier the snake consume an obstacle this function will add another new one
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
                        player.Stop();
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
                if (snakeNewHead.col == food.col && snakeNewHead.row == food.row || snakeNewHead.col == food.col + 1 && snakeNewHead.row == food.row)
                {

                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");

                    Console.SetCursorPosition(food.col + 1, food.row);
                    Console.Write(" ");

                    //Soundeffect added.
                    SystemSounds.Beep.Play();

                    // feeding the snake
                    AddNewFood();

                    //check current score
                    checkScore();

                    // get the lastFoodTime (in Millisecond)
                    lastFoodTime = Environment.TickCount;

                    SetFood();
                    sleepTime--;

                    for (int i = 0; i < multiplier; i++)
                    {
                        AddNewObstacle();
                    }

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
                if (snakeElements.Count == multiplier * 20)
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
                //set the food to black heart
                string cUnicode = "2665";
                int value = int.Parse(cUnicode, System.Globalization.NumberStyles.HexNumber);
                string symbol = char.ConvertFromUtf32(value).ToString();
                string foode = symbol + symbol;
                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.Write(foode);
            };

            // set the obstacle position,color,icon.
            void SetObstacle(Position obstacle)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                //set the obstacle to medium shade
                string cUnicode = "2592";
                int value = int.Parse(cUnicode, System.Globalization.NumberStyles.HexNumber);
                string symbol = char.ConvertFromUtf32(value).ToString();
                Console.OutputEncoding = System.Text.Encoding.Unicode;
                Console.Write(symbol);
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
                gameFinish = true;
                int y = Console.WindowHeight / 2;
                string text1 = "Game over!";
                string text2 = "Your points are: ";
                string text3 = "Press 1 to view leaderboard, 2 to exit the game";

                int text1length = text1.Length;
                int text2length = text2.Length;
                int text3length = text3.Length;

                int text1start = (Console.WindowWidth - text1length) / 2;
                int text2start = (Console.WindowWidth - text2length) / 2;
                int text3start = (Console.WindowWidth - text3length) / 2;

                //Set Game over to middle of the window
                Console.SetCursorPosition(text1start, y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(text1);


                int userPoints = (snakeElements.Count - 4) * 120 - negativePoints;
                if (userPoints < 0) userPoints = 0;

                //Set Score to middle of the window
                Console.SetCursorPosition(text2start, y + 1);
                userPoints = Math.Max(userPoints, 0);
                Console.WriteLine("{0}{1}", text2, userPoints);

                Console.SetCursorPosition(text3start, y + 2);
                Console.WriteLine(text3);

                //Add player score into plain text file.
                StreamWriter snakeFile = new StreamWriter("Snake_Score.txt", true);
                snakeFile.Write(name + "\n");
                snakeFile.Write(userPoints + "\n");
                snakeFile.Close();

                string input = Console.ReadLine();

                while (input != "1" && input != "2")
                {
                    Console.WriteLine("Please enter a valid number");
                    input = Console.ReadLine();
                }

                if (input == "1")
                {
                    ShowLeaderBoard(2);
                }

                else if (input == "2")
                {
                    Environment.Exit(0);
                }

            }

            void Win()
            {
                gameFinish = true;
                int y = Console.WindowHeight / 2;
                string text1 = "You Win!!!!";
                string text2 = "Your points are: ";
                string text3 = "Press 1 to view leaderboard, 2 to exit the game";

                int text1length = text1.Length;
                int text2length = text2.Length;
                int text3length = text3.Length;

                int text1start = (Console.WindowWidth - text1length) / 2;
                int text2start = (Console.WindowWidth - text2length) / 2;
                int text3start = (Console.WindowWidth - text3length) / 2;

                Console.SetCursorPosition(text1start, y);
                Console.WriteLine(text1);

                int userPoints = (snakeElements.Count - 4) * 120 - negativePoints;
                if (userPoints < 0) userPoints = 0;

                //Set Score to middle of the window
                Console.SetCursorPosition(text2start, y + 1);
                userPoints = Math.Max(userPoints, 0);
                Console.WriteLine("{0}{1}", text2, userPoints);

                Console.SetCursorPosition(text3start, y + 2);
                Console.WriteLine(text3);

                //Add player score into plain text file.
                StreamWriter snakeFile = new StreamWriter("Snake_Score.txt", true);
                snakeFile.Write(name + "\n");
                snakeFile.Write(userPoints + "\n");
                snakeFile.Close();

                string input = Console.ReadLine();
                while (input != "1" && input != "2")
                {
                    Console.WriteLine("Please enter a valid number");
                    input = Console.ReadLine();
                }

                if (input == "1")
                {
                    ShowLeaderBoard(2);
                }

                else if (input == "2")
                {
                    Environment.Exit(0);
                }
            }

            void checkScore()
            {
                int updatePoint = userPoint + 100;
                if (updatePoint >= checkPoint)
                {
                    life++;
                    checkPoint += (500 * multiplier);
                }
            }

            void AddNewObstacle()
            {
                Position obstacle = new Position();
                do
                {
                    obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(obstacle) || obstacles.Contains(obstacle) || (food.row != obstacle.row && food.col != obstacle.row) || (food.row < 18 && food.col < 4 ));
                obstacles.Add(obstacle);
                SetObstacle(obstacle);
            }

            void AddNewFood()
            {
                do
                {
                    food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(food) || obstacles.Contains(food) || (food.row < 18 && food.col < 4));
            }

            //philip
            void helpmenu()
            {
                Console.Clear();
                //Showing Guides for the Player.
                Console.WriteLine("Welcome to the Snake Game\n");
                Console.WriteLine("This Page will be guiding you how to play this game.\n");
                Console.WriteLine("The below are the symbols of the game and what are they representing\n");
                Console.WriteLine(" ***>  -  Your snake     @  - Your food     =  -  Obstacle\n");
                Console.WriteLine("The Control Keys are your Arrow Keys.\n");
                Console.WriteLine("Your snake will only move foward and you need to use your arrow keys to control its moving direction.\n");
                Console.WriteLine("The game will be having 3 lifes and 5 obstacles to start with and obstacles are randomly spawn.\n\nEach time you eat a food, which will be showing like @ symbol\n");
                Console.WriteLine("Each time you hit an Obstacle, it will disappear and respawn at a different location and you deduct one life and some scores as punishment.\n");
                Console.WriteLine("If you eat/hit your own body, you will lose directly, so please avoid that.\n");
                Console.WriteLine("There are 3 Difficulty, Easy, Normal and Hard. Details as below:\n");
                Console.WriteLine("Easy - Each 500 score will increase 1 EXTRA LIFE. Winning Requirement: Eat 20 foods\n");
                Console.WriteLine("Normal - Each 1000 score will increase 1 EXTRA LIFE. Each obstacle consume will increase 2 more obstacle and Food disappear speed is increased. Winning Requirement: Eat 40 foods\n");
                Console.WriteLine("Hard - Each 1500 score will increase 1 EXTRA LIFE. Each obstacle consume will increase 3 more obstacle and Food disappeaer speed is increase significantly. Winning Requirement: Eat 60 foods\n");
                Console.WriteLine("Your score will be recorded into the leaderboard for record everytime you WIN or Lose.\n");

                //Prompt the user to select an option after viewing leaderboard
                string userLeadInput;
                int userLIResult = 0;
                bool validInput = false;

                Console.Write("\n" + "Enter '1' to go back to main menu and '2' to exit the program\n");
                userLeadInput = Console.ReadLine();

                while (!validInput)
                {

                    if (!int.TryParse(userLeadInput, out userLIResult))
                    {
                        Console.WriteLine("Please enter '1' or '2'");
                    }
                    else if (userLIResult.Equals(0))
                    {
                        Console.WriteLine("You cannot enter zero.");
                    }
                    else
                    {
                        validInput = true;
                        if (userLIResult == 1)
                        {
                            Console.Clear();
                            menu();
                        }
                        else if (userLIResult == 2)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }

            void ShowLeaderBoard(int condition)
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
                file.Close();

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
                
                if (condition == 1)
                {
                    //Prompt the user to select an option after viewing leaderboardt;
                    string userLeadInput;

                    Console.Write("\n" + "Enter '1' to go back to main menu and '2' to exit the program\n");
                    userLeadInput = Console.ReadLine();
                    while (userLeadInput != "1" && userLeadInput != "2")
                    {
                        Console.WriteLine("Please enter a valid input");
                        userLeadInput = Console.ReadLine();

                    }

                    if (userLeadInput == "1")
                    {
                        Console.Clear();
                        menu();
                    }
                    else if (userLeadInput == "2")
                    {
                        Environment.Exit(0);
                    }
                }

                else if (condition == 2)
                {
                    string userLeadInput;
                    Console.Write("\n" + "Enter anything to exit the program\n");
                    userLeadInput = Console.ReadLine();
                    Environment.Exit(0);
                }
               

            }

            void menu()
            {
                player.Stop();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                string userOption;
                string condition = "correct";
                do
                {
                    int ystart = (Console.WindowHeight-2) / 2;
                    string text1 = "Welcome to the Snake Menu. Please choose an option below:";
                    string text2 = "\t\t\t(1) Play Game\t(2) View Leaderboard\t(3) Help\t(4) Quit Game";
                    int text1length = text1.Length;
                    int text2length = text2.Length;

                    int text1start = (Console.WindowWidth - text1length) / 2;
                    int text2start = (Console.WindowWidth - text2length) / 2;

                    //Set menu to middle of the window
                    Console.SetCursorPosition(text1start, ystart);
                    Console.SetCursorPosition(text2start, ystart + 1);
                    Console.WriteLine(text1);
                    Console.WriteLine(text2);

                    userOption = Console.ReadLine();

                    switch (userOption)
                    {
                        case "1":
                            Console.WriteLine("Please select the difficulty, 1 = Easy, 2 = Medium, 3 = Hard");
                            Difficulty = Console.ReadLine();
                            while (Difficulty != "1" && Difficulty != "2" && Difficulty != "3")
                            {
                                Console.WriteLine("Please enter a valid number");
                                Difficulty = Console.ReadLine();
                            }
                            Console.WriteLine("You have chosen option " + userOption + " -> Play the game again");
                            condition = "correct";
                            player.PlayLooping();
                            break;
                        case "2":
                            Console.WriteLine("You have chosen option " + userOption + " -> View Leaderboard");
                            condition = "correct";
                            ShowLeaderBoard(1);
                            break;
                        case "3":
                            Console.WriteLine("You have chosen option " + userOption + " -> View Help Page");
                            condition = "correct";                            
                            helpmenu();
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