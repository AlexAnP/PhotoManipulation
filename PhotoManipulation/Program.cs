using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PhotoManipulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.GetEncoding(1251);

            Console.WriteLine("Пожалуйста, выберите желаемую опцию: \n" +
                             "1. Переименование изображении в соответствии с датой сьемки. \n" +
                             "2. Добавления на изображение отметки, когда фото было сделано. \n" +
                             "3. Сортировка изображений по папкам по годам. \n" +
                             "4. Сортировка изображений по папкам по месту сьемки. \n" +
                             "Введите номер опции:");

            var choice = Console.ReadKey().KeyChar;

            Console.Clear();
            Console.WriteLine("Введите путь к файлу:");
            string path = Console.ReadLine();

            DirectoryInfo source = new DirectoryInfo(path);

            switch (choice)
            {
                case '1':
                    Change.RenameImg(path, source);
                    break;
                case '2':
                    Change.DateTaken(path, source);
                    break;
                case '3':
                    Change.SortByYear(path, source);
                    break;
                case '4':
                    Change.SortByPlace(path, source);
                    break;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }

            Console.ReadLine();
        }
    }
}
