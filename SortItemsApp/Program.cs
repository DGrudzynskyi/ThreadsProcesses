using System;
using System.Linq;
using System.Threading;

namespace SortItemsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var itemsList = args.ToList();
            var selfClosing = false;

            // если программа вызвана с аргументами - сортирует аргументы и пишет результат сортировки в стандартный вывод процесса
            // иначе - предлагает юзеру ввести значения
            if (itemsList.Count == 0)
            {
                selfClosing = true;
                Console.WriteLine("SortItemsApp process is waiting for user input:");

                string inputText = "";
                do
                {
                    Console.WriteLine("Enter a line of text (or press the Enter key to stop):");

                    inputText = Console.ReadLine();
                    if (inputText.Length > 0)
                    {
                        itemsList.Add(inputText);
                    }
                } while (inputText.Length > 0);
            }

            // собственно сортировка
            itemsList.Sort();
            for (var i = 0; i < itemsList.Count; i++){
                Console.WriteLine(String.Format("item {0}: {1}", i, itemsList[i]));
            }

            // если было вызвано с аргументами - просто возвращаем результат в консоль выше.
            // если же процесс вызывался автономно и предлагал юзеру вводить что-то - убеждаемся что юзер прочитает результат до закрытия окна
            if (selfClosing) {
                Console.ReadLine();
            }
        }
    }
}
