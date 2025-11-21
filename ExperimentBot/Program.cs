namespace ExperimentBot
{
    internal class Program
    {

        static Dictionary<string, string> allPaths;

        static void Initialization()
        {
            allPaths = new Dictionary<string, string>()
            {
                { "answers", "F:\\Visual Studio\\Projects\\AppsWPF\\ExperimentBot\\bot\\data\\answers.txt" } 
            };
            Console.WriteLine("Бот инициализирован");
        }

        static void Thinking(string word)
        {
            Console.WriteLine($"Бот думает над запросом...");
            var searching = Task.Run(() => SearchInFile(word));
            while(!searching.IsCompleted)
            {
                Console.Write("\r\\");
                Thread.Sleep(50);
                Console.Write("\r|");
                Thread.Sleep(50);
                Console.Write("\r/");
                Thread.Sleep(50);
                Console.Write("\r-");
                Thread.Sleep(50);
            }
            if(searching.IsCompleted)
            {
                if(searching.Result)
                {
                    Console.WriteLine("\rНайдет ответ!");
                }
                else
                {
                    Console.WriteLine("\rОтвет не найден.");
                }
            }
            Task.WaitAll(searching);
            searching.Dispose();
        }

        static bool SearchInFile(string search)
        {
            bool answer = false;
            using (StreamReader sr = new StreamReader(allPaths["answers"]))
            {
                var text = sr.ReadToEnd();
                if (text.Split("\n").Contains(search.ToLower()+"\r"))
                {
                    answer = true;
                }
            }
            return answer;
        }

        static void CreatingAnswer(string request)
        {
            using (StreamWriter sw = new(allPaths["answers"],true))
            {
                sw.Write("{0}\r\n", request);
            }
            Console.WriteLine($"{request} добавлен в список.");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Бот включён.");
            Initialization();
            var thinking = Task.Run(() => Thinking("боб"));
            var writing = Task.Run(() => CreatingAnswer("боб"));
            Task.WaitAll(thinking,writing);
            thinking.Dispose();
            writing.Dispose();
            Console.WriteLine("Бот выключается");
            Console.ReadLine();
        }
    }
}
