using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PopulatorBot
{
    class Program
    {
        static ITelegramBotClient botClient;
        static long CHANNEL = Secrets.channelID;
        static String FOLDER = Secrets.folderPath;
        static DirectoryInfo dir = new DirectoryInfo(FOLDER);
        static int waiter = 1000;

        static void Main(string[] args)
        {
            botClient = new TelegramBotClient(Secrets.apiToken);
            init();
            Console.WriteLine("Running...");
            while (true)
            {
                Console.Read();
            }
        }

        static void init()
        {
            Thread thread = new Thread(new ThreadStart(Populate));
            thread.Start();
            botClient.StartReceiving();
        }

        static async void Populate()
        {
            int loops = 0;
            int tries = 0;
            while (true)
            {
                try
                {
                    if (tries < 3)
                    {
                        if (dir.GetFiles()[0].FullName.Contains("desktop.ini"))
                        {
                            dir.GetFiles()[0].Delete();
                            Console.WriteLine("desktop.ini has been deleted on place 0");
                        }
                        if (dir.GetFiles()[1].FullName.Contains("desktop.ini"))
                        {
                            dir.GetFiles()[1].Delete();
                            Console.WriteLine("desktop.ini has been deleted on place 1");
                        }
                        if (dir.GetFiles().Length >= 2)
                        {
                            InputMediaPhoto picone = new InputMediaPhoto(Upload(dir.GetFiles()[0].FullName));
                            InputMediaPhoto pictwo = new InputMediaPhoto(Upload(dir.GetFiles()[1].FullName));

                            var First = dir.GetFiles()[0].FullName;
                            var Second = dir.GetFiles()[1].FullName;

                            InputMediaPhoto[] pics = new InputMediaPhoto[] { picone, pictwo };
                            await botClient.SendMediaGroupAsync(pics, CHANNEL);

                            System.IO.File.Delete(First);
                            System.IO.File.Delete(Second);
                            tries = 0;
                            Console.WriteLine("Sent: " + ((loops+1)*2));
                            loops++;
                        }
                    }
                    else if (tries == 3)
                    {
                        System.IO.File.Move(dir.GetFiles()[0].FullName, Secrets.dumpPath + dir.GetFiles()[0].Name);
                        System.IO.File.Move(dir.GetFiles()[0].FullName, Secrets.dumpPath + dir.GetFiles()[0].Name);
                        tries = 0;
                    }
                }
                catch(Exception e)
                {
                    tries++;
                    if (tries < 3)
                    {
                        Console.WriteLine(e.StackTrace + "\n\n" + e.Message + "\n\n" + dir.GetFiles()[0].FullName + "\n" + dir.GetFiles()[1].FullName + "\nTry: " + (tries+1));
                    }
                    else if (tries == 3)
                    {
                        Console.WriteLine(e.StackTrace + "\n\n" + e.Message + "\n\n" + dir.GetFiles()[0].FullName + "\n" + dir.GetFiles()[1].FullName + "\nfiles moved");
                    }
                    if(e.Message.ToLower().Contains("too many requests"))
                    {
                        waiter = 10000;
                    }
                    else
                    {
                        waiter = 6000;
                    }
                }
                await Task.Delay(waiter);
            }
        }

        static InputMedia Upload(String LocalURL)
        {
            return new InputMedia(new FileStream(LocalURL, FileMode.Open), LocalURL.Split('\\')[LocalURL.Split('\\').Length-1]);
        }
    }
}
