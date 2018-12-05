using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;



namespace ConsoleApp15
{
	class Program
	{
		
		static List<int> list1 = new List<int>();
		static Mutex mutex = new Mutex();
		static void Main(string[] args)
		{
			Console.WriteLine("\n");
			var list = Process.GetProcesses()			//Процессы
	.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
	.Select(p => p.Id + "   " +  p.MainWindowTitle + "  , время запуска программы :  " + p.StartTime.ToString() + "  , полное время процессора для этого процесса : " + p.TotalProcessorTime)
	.ToList();
			foreach (object o in list)
				Console.WriteLine(o);

			Console.WriteLine("\n" + "--------------------------------------------------------------------------------------------------------" + "\n");

			AppDomain domain = AppDomain.CurrentDomain;		//Текущей домен
			Console.WriteLine("Name: {0}", domain.FriendlyName);
			Console.WriteLine("Base Directory: {0}", domain.BaseDirectory);
			Console.WriteLine();

			Assembly[] assemblies1 = domain.GetAssemblies();
			foreach (Assembly asm in assemblies1)
				Console.WriteLine(asm.GetName().Name);

			Console.WriteLine("\n" + "--------------------------------------------------------------------------------------------------------" + "\n");

			Assembly asm2 = Assembly.LoadFrom("ConsoleApp2.dll");		//Load

			AppDomain secondaryDomain = AppDomain.CreateDomain("Secondary domain");
			// событие загрузки сборки
			secondaryDomain.AssemblyLoad += Domain_AssemblyLoad;
			// событие выгрузки домена
			secondaryDomain.DomainUnload += SecondaryDomain_DomainUnload;


			Console.WriteLine("Домен: {0}", secondaryDomain.FriendlyName);
			secondaryDomain.Load(asm2.ToString());
			Assembly[] assemblies = secondaryDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
				Console.WriteLine(asm.GetName().Name);
			// выгрузка домена
			AppDomain.Unload(secondaryDomain);

			Console.WriteLine("\n" + "--------------------------------------------------------------------------------------------------------" + "\n");

			var sleepingThread = new Thread(Program.SleepIndefinitely);
			sleepingThread.Name = "Sleeping";
			sleepingThread.Start();
			Thread.Sleep(2000);
			sleepingThread.Interrupt();

			Thread.Sleep(1000);

			sleepingThread = new Thread(Program.SleepIndefinitely);
			sleepingThread.Name = "Sleeping2";
			sleepingThread.Start();
			Thread.Sleep(2000);
			sleepingThread.Abort();

			Console.WriteLine("\n" + "--------------------------------------------------------------------------------------------------------" + "\n");
			Thread thread1 = new Thread(Add1);
			Thread thread2 = new Thread(Add2);

			thread2.Priority = ThreadPriority.Normal;

			thread1.Start();
			thread2.Start();

			thread1.Join();
			thread2.Join();

			foreach (int i in list1)
			{
				Console.WriteLine(i);
			}

			Console.ReadKey();
		}
		private static void SecondaryDomain_DomainUnload(object sender, EventArgs e)
		{
			Console.WriteLine("Домен выгружен из процесса");
		}

		private static void Domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Console.WriteLine("Сборка загружена");
		}
		private static void SleepIndefinitely()
		{
			Console.WriteLine("Thread '{0}' sleep.",
							  Thread.CurrentThread.ManagedThreadId,
							  Thread.CurrentThread.Name, Thread.CurrentThread.IsAlive);
			try
			{
				Thread.Sleep(Timeout.Infinite);
			}
			catch (ThreadInterruptedException)
			{
				Console.WriteLine("Thread '{0}' awoken.",
								  Thread.CurrentThread.Name);
			}
			catch (ThreadAbortException)
			{
				Console.WriteLine("Thread '{0}' aborted.",
								  Thread.CurrentThread.Name);
			}
			finally
			{
				Console.WriteLine("Thread '{0}' executing finally block.",
								  Thread.CurrentThread.Name);
			}
			Console.WriteLine("Thread '{0} finishing normal execution.",
							  Thread.CurrentThread.Name);
			Console.WriteLine();
		}
		static void Add1()
		{
			for(int i = 1; i <= 10; i++)
			{
				if(i % 2 != 0)
				{
					mutex.WaitOne();	
					list1.Add(i);
					mutex.ReleaseMutex();
				}
			}
		}
		static void Add2()
		{
			for (int i = 1; i <= 10; i++)
			{
				if (i % 2 == 0)
				{
					mutex.WaitOne();
					list1.Add(i);
					mutex.ReleaseMutex();
				}
			}
		}
	}
}
