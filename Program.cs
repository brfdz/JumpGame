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

		public enum MoveState {Default, Jump, DoubleJump, Crouch};

		static void Main(string[] args)
		{
			Player p = new Player();
			p.left = 20;
			p.top = WindowHeight/ 2;
			Clear();

			WindowWidth = 100;
			WindowHeight = 30;

			CursorVisible = false;

			Random rnd = new Random();

			Stopwatch swGame = new Stopwatch();
			Stopwatch swObsticleSpawn = new Stopwatch();
			Stopwatch swObsticle = new Stopwatch();
			Stopwatch swJump = new Stopwatch();

			TimeSpan tsObsticleSpawn = TimeSpan.FromSeconds(1.5);
			TimeSpan tsObsticleMove = TimeSpan.FromMilliseconds(30);
			TimeSpan tsJump = TimeSpan.FromMilliseconds(400);
			TimeSpan tsDown = TimeSpan.FromMilliseconds(600);

			List<Obsticle> obs = new List<Obsticle>();


			//headers
			Title = "Jump";
			WriteLine(header);

			//start screen
			Console.WriteLine("\nPress any key to start...");
			Console.ReadKey();

			Floor f = new Floor();
			Console.Clear();
			Console.SetCursorPosition(0, p.top + 5);

			string item = f.print();
			
			WriteLine(item);
			
			int jumpLimit = 2;
			bool crouch = false;

			swObsticle.Restart();
			swObsticleSpawn.Restart();
			swGame.Restart();

			//initial player render
			Console.SetCursorPosition(p.left, p.top);
			Render(p.image);

			bool isLost = false;
			while (!isLost)
			{
				//print score
				Console.SetCursorPosition(0, 0);
				Console.Write("Score: " + p.score);

				if (p.score > 50)
				{
					tsObsticleMove = TimeSpan.FromMilliseconds(15);
					tsJump = tsDown = TimeSpan.FromMilliseconds(300);
				}
				else if (p.score > 20) tsObsticleMove = TimeSpan.FromMilliseconds(20);

				//Update player
				if (Console.KeyAvailable)
				{
					ConsoleKeyInfo input = ReadKey(true);
					switch (input.Key)
					{
						//jump
						case ConsoleKey.W:
						case ConsoleKey.UpArrow:
							//render jump up
							if (p.state < MoveState.DoubleJump)
							{
								
								jump(p, 3);
								if (p.state == MoveState.Jump) p.state = MoveState.DoubleJump;
								else p.state = MoveState.Jump;
								swJump.Restart();
							}
							break;

						case ConsoleKey.S:
						case ConsoleKey.DownArrow:
							if (p.state == MoveState.Default)
							{
								Console.SetCursorPosition(p.left, p.top);
								Erase(p.image);
								p.top += 1;
								Console.SetCursorPosition(p.left, p.top);
								Render(p.imageDown);
								crouch = true;
								swJump.Restart();

								p.state = MoveState.Crouch;
							}
							break;

					}
				}

				//render player
				switch (p.state) {
					case MoveState.DoubleJump: //render double jump landing
						if (swJump.Elapsed > tsJump)
						{
							Console.SetCursorPosition(p.left, p.top);
							Erase(p.image);
							p.top += 3;
							Console.SetCursorPosition(p.left, p.top);
							Render(p.image);
							swJump.Restart();
							jumpLimit++;
							p.state = MoveState.Jump;
						}
						break;

					case MoveState.Jump: //Render single jump landing
						if (swJump.Elapsed > tsJump)
						{
							Console.SetCursorPosition(p.left, p.top);
							Erase(p.image);
							p.top += 3;
							Console.SetCursorPosition(p.left, p.top);
							Render(p.image);
							swJump.Restart();
							jumpLimit++;
							p.state = MoveState.Default;
						}
						break;

					case MoveState.Crouch:
						if (swJump.Elapsed > tsDown)
						{
							Console.SetCursorPosition(p.left, p.top);
							Erase(p.imageDown);
							p.top -= 1;
							Console.SetCursorPosition(p.left, p.top);
							Render(p.image);
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
						ob.top = WindowHeight / 2 + 1;
					}
					else
					{
						ob.top = rnd.Next(2) == 1 ? WindowHeight / 2 + 3 : WindowHeight / 2;
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
						//check collision
						//if collides game over

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

					if (isCollision(p, ob))
					{
						isLost = true;
						//Thread.Sleep(20);
					}
				}

			}

			//Clear();
			Console.SetCursorPosition(WindowHeight/2, 0);
			ForegroundColor = ConsoleColor.Red;
			Write("\nGame Over\n");
			/*Write(@"
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
");*/

			
			Console.WriteLine("Press any key to exit...");
			ResetColor();
			Console.ReadKey();
		}

		static bool isCollision(Player p, Obsticle ob)
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

			//let it pass from below
			
			//front hit
			if (ob.left <= pRight && obRight > pRight && pBottom > ob.top && obBottom > p.top)
			{
				
				return true;
			} //bottom hit
			else if (pBottom >= ob.top && ob.left <= pRight && p.left <= obRight) return true;

			return false;
		}

		static void jump(Player p, int peak)
		{
			Console.SetCursorPosition(p.left, p.top);
			Erase(p.image);
			p.top -= peak;
			Console.SetCursorPosition(p.left, p.top);
			Render(p.image);
			
			
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
		public string image = @"
\\ 
  ()   
  /)r
o()_";
		public string imageDown = @"
    \\
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
		public string image;

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

				image += "\n";
			}

			return image;
		}
	}
}
