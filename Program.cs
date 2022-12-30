using System;
using static System.Console;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

/* References
 *  Character art: https://asciiart.website/index.php?art=animals/rabbits
 *  Header art: https://www.patorjk.com/software/taag/#p=display&f=Graffiti&t=Type%20Something%20
 *  Erase and Render Methods adapted from: https://github.com/ZacharyPatten/dotnet-console-games/tree/main/Projects/Helicopter
 */

namespace FinalProject
{

	class Program
	{
		static string header = @"______/\\\\\\\\\\\__/\\\________/\\\__/\\\\____________/\\\\__/\\\\\\\\\\\\\___        
 _____\/////\\\///__\/\\\_______\/\\\_\/\\\\\\________/\\\\\\_\/\\\/////////\\\_       
  _________\/\\\_____\/\\\_______\/\\\_\/\\\//\\\____/\\\//\\\_\/\\\_______\/\\\_      
   _________\/\\\_____\/\\\_______\/\\\_\/\\\\///\\\/\\\/_\/\\\_\/\\\\\\\\\\\\\/__     
    _________\/\\\_____\/\\\_______\/\\\_\/\\\__\///\\\/___\/\\\_\/\\\/////////____    
     _________\/\\\_____\/\\\_______\/\\\_\/\\\____\///_____\/\\\_\/\\\_____________   
      __/\\\___\/\\\_____\//\\\______/\\\__\/\\\_____________\/\\\_\/\\\_____________  
       _\//\\\\\\\\\_______\///\\\\\\\\\/___\/\\\_____________\/\\\_\/\\\_____________ 
        __\/////////__________\/////////_____\///______________\///__\///______________
";

		static string lost = @"
   _                   _
 _( )                 ( )_
(_, |      __ __      | ,_)
   \'\    /  ^  \    /'/
    '\'\,/\      \,/'/'
      '\| []   [] |/'
        (_  /^\  _)
          \  ~  /
          /HHHHH\
        /'/{^^^}\'\
    _,/'/'  ^^^  '\'\,_
   (_, |           | ,_)
     (_)           (_)
";

		public enum MoveState { Default, Jump, DoubleJump, Crouch };

		static void Main(string[] args)
		{
			/*Player p = new Player();
			p.left = 20;
			p.top = WindowHeight / 2;*/
			Clear();

			WindowWidth = 100;
			WindowHeight = 30;

			CursorVisible = false;

			Random rnd = new Random();

			Stopwatch swObsticleSpawn = new Stopwatch();
			Stopwatch swObsticle = new Stopwatch();
			Stopwatch swJump = new Stopwatch();

			TimeSpan tsObsticleSpawn = TimeSpan.FromSeconds(1.5);
			TimeSpan tsObsticleMove = TimeSpan.FromMilliseconds(15);
			TimeSpan tsJump = TimeSpan.FromMilliseconds(350);
			TimeSpan tsDown = TimeSpan.FromMilliseconds(300);

			List<Obsticle> obs = new List<Obsticle>();

			//headers
			Title = "Jump";
			WriteLine(header);

			//start screen
			Console.WriteLine("\nPress any key to start...");
			Console.ReadKey();

			bool playerLost;
			int highest = 0;
			bool gameEnd = false;
			do
			{
				Player p = new Player();
				p.left = 20;
				p.top = WindowHeight / 2;
				//initial setup
				playerLost = false;
				Console.Clear();
				Console.SetCursorPosition(0, p.top + 4);
				WriteLine(new Floor().print());
				Console.SetCursorPosition(p.left, p.top);
				Render(p.image);

				swObsticle.Restart();
				swObsticleSpawn.Restart();
				
			
				while (!playerLost)
				{
					//Update score header
					Console.SetCursorPosition(0, 0);
					Write("Score: " + p.score);
					ForegroundColor = ConsoleColor.Cyan;
					Write("\tHighest score: " + highest);
					ResetColor();

					//Update player
					if (Console.KeyAvailable)
					{
						ConsoleKeyInfo input = ReadKey(true);
						switch (input.Key)
						{
							//Jump
							case ConsoleKey.W:
							case ConsoleKey.UpArrow:
								//Render jump up
								if (p.state < MoveState.DoubleJump)
								{
									Move(p, -3);
									if (p.state == MoveState.Jump) p.state = MoveState.DoubleJump;
									else p.state = MoveState.Jump;
									swJump.Restart();
								}
								break;

							case ConsoleKey.S:
							case ConsoleKey.DownArrow:
								if (p.state == MoveState.Default)
								{
									p.state = MoveState.Crouch;
									Move(p, 1);
									swJump.Restart();
								}
								break;

						}
					}

					//render player
					switch (p.state)
					{
						case MoveState.DoubleJump: //render double jump landing
							if (swJump.Elapsed > tsJump)
							{
								Move(p, 3);
								swJump.Restart();
								p.state = MoveState.Jump;
							}
							break;

						case MoveState.Jump: //Render single jump landing
							if (swJump.Elapsed > tsJump)
							{
								Move(p, 3);
								swJump.Restart();
								p.state = MoveState.Default;
							}
							break;

						case MoveState.Crouch:
							if (swJump.Elapsed > tsDown)
							{
								Move(p, -1);
								swJump.Restart();
								p.state = MoveState.Default;
							}
							break;
					}

					//Update obsticle
					if (swObsticleSpawn.Elapsed > tsObsticleSpawn)
					{
						int height = rnd.Next(2) == 1 ? 2 : 4;
						Obsticle ob = new Obsticle(2, height, '█');
						ob.left = WindowWidth;
						if (height == 4)
						{
							ob.top = WindowHeight / 2;
						}
						else
						{
							ob.top = rnd.Next(2) == 1 ? WindowHeight / 2 + 2 : WindowHeight / 2 - 1;
						}


						obs.Add(ob);
						swObsticleSpawn.Restart();
						tsObsticleSpawn = TimeSpan.FromSeconds(1 + rnd.NextDouble() * 2);
					}

					if (swObsticle.Elapsed > tsObsticleMove)
					{
						for (int i = 0; i < obs.Count; i++)
						{
							Obsticle ob = obs[i];
							if (ob.left < WindowWidth)
							{
								Console.SetCursorPosition(ob.left, ob.top);
								Erase(ob.frame);
							}
							ob.left--;

							//add points if passed
							if (ob.left + 2 <= p.left)
							{
								p.score += ob.point;
								ob.point = 0;
							}

							if (ob.left <= 0)
							{
								Console.SetCursorPosition(ob.left, ob.top);
								Erase(ob.frame);
								obs.Remove(ob);
							}
						}
						swObsticle.Restart();
					}


					//render obsticles
					foreach (Obsticle ob in obs)
					{
						if (ob.left < WindowWidth)
						{
							Console.SetCursorPosition(ob.left, ob.top);
							Render(ob.frame);
						}

						if (isCollided(p, ob))
						{
							playerLost = true;
						}
					}
				}

				//Game over
				ForegroundColor = ConsoleColor.Red;
				Console.SetCursorPosition(45, 3);
				Write("Game Over\n");
				Console.SetCursorPosition(37, 5);
				Render(lost);

				Console.SetCursorPosition(37, WindowHeight - 9);
				Write("Press [Enter] to continue...");
				ResetColor();
				ConsoleKeyInfo answer = ReadKey(true);
				if (answer.Key.CompareTo(ConsoleKey.Enter) == 0)
					gameEnd = false;
				else
					gameEnd = true;


				//Reset the game
				highest = Math.Max(highest, p.score);
				Console.Clear();
				obs.Clear();


			} while (!gameEnd);

			Console.WriteLine(@"
                                     ;                                                  
                  :           :      ED.                                                
                 t#,         t#,     E#Wi                                             ,;
          .Gt   ;##W.       ;##W.    E###G.             .                           f#i 
         j#W:  :#L:WE      :#L:WE    E#fD#W;            Ef.        f.     ;WE.    .E#t  
       ;K#f   .KG  ,#D    .KG  ,#D   E#t t##L           E#Wi       E#,   i#G     i#W,   
     .G#D.    EE    ;#f   EE    ;#f  E#t  .E#K,         E#K#D:     E#t  f#f     L#D.    
    j#K;     f#.     t#i f#.     t#i E#t    j##f        E#  E#f.   E#t G#i    :K#Wfff;  
  ,K#f   ,GD;:#G     GK  :#G     GK  E#t    :E#K:       E#WEE##Wt  E#jEW,     i##WLLLLt 
   j#Wi   E#t ;#L   LW.   ;#L   LW.  E#t   t##L         E##Ei;;;;. E##E.       .E#L     
    .G#D: E#t  t#f f#:     t#f f#:   E#t .D#W;          E#DWWt     E#G           f#E:   
      ,K#fK#t   f#D#;       f#D#;    E#tiW#G.           E#   #K;   E#t            ,WW;  
        j###t    G#t         G#t     E#K##i             E#Dfff##E, E#t             .D#; 
         .G#t     t           t      E##D.              jLLLLLLLLL;EE.               tt 
           ;;                        E#t                           t                    
                                     L:                                                 
");
		}

		static bool isCollided(Player p, Obsticle ob)
		{ 
			int width;
			int height;
			switch (p.state)
			{
				case MoveState.Crouch:
					width = 8;
					height = 3;
					break;
				default:
					width = 5;
					height = 4;
					break;
			}

			int pRight = p.left + width;
			int obRight = ob.left + ob.width;
			int pBottom = p.top + height;
			int obBottom = ob.top + ob.height;

			//front hit
			if (ob.left <= pRight && obRight > pRight && pBottom > ob.top && p.top < obBottom)
			{
				return true;
			} //below or above obsticle
			else if (ob.left <= pRight && p.left <= obRight)
			{
				//touches obsticle from above
				if (pBottom - 1 >= ob.top && p.top < obBottom - 1)
				{
					return true;
				}
				//touches obsticle from below
				if (p.top < obBottom && pBottom > ob.top)
				{
					return true;
				}
			}

			return false;
		}

		static void Move(Player p, int distance)
		{
			Console.SetCursorPosition(p.left, p.top);
			Erase((distance == -1)? p.imageDown: p.image);
			p.top += distance;
			Console.SetCursorPosition(p.left, p.top);
			Render((distance == 1)? p.imageDown: p.image);
			
		}

		static void Erase(string @string)
		{
			int x = Console.CursorLeft;
			int y = Console.CursorTop;
			foreach (char c in @string)
				if (c is '\n')
					Console.SetCursorPosition(x, ++y);
				else if (Console.CursorLeft < WindowWidth - 1 && c != ' ')
					Console.Write(' ');
				else if (Console.CursorLeft < WindowWidth - 1 && Console.CursorTop < WindowHeight - 1)
					Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
		}

		static void Render(string @string)
		{
			int x = Console.CursorLeft;
			int y = Console.CursorTop;
			foreach (char c in @string)
			{
				if (c is '\n')
					Console.SetCursorPosition(x, ++y);
				else if (Console.CursorLeft < WindowWidth - 1 && c != ' ')
					Console.Write(c);
				else if (Console.CursorLeft < WindowWidth - 1 && Console.CursorTop < WindowHeight - 1)
					Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
			}
		}

	}

	class Player
	{
		public int left;
		public int top;
		public int score;
		public Program.MoveState state;
		public string image = @"\\ 
  ()   
  /)r
o()_";
		public string imageDown = @"    \\
  .---()
o()_-\_";

		public Player()
		{
			score = 0;
			state = Program.MoveState.Default;
		}

		 
	}

	class Floor : Block
	{
		public Floor()
		{
			height = 1;
			width = Console.WindowWidth;
			style = '=';
		}


	}

	class Obsticle : Block
	{
		public int left;
		public int top;
		public string frame;
		public int point;
		//create an obsticle with the required sizes and style
		public Obsticle(int w, int h, char style)
		{
			width = w;
			height = h;
			this.style = style;
			frame = print();
			point = height / 2;
		}

		
	}

	class Block
	{
		public int height;
		public int width;
		public char style;

		//create the block as a string
		public string print()
		{
			string image = "";
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					image += style;
				}

				if(i < height - 1) image += "\n";
			}

			return image;
		}
	}
}
