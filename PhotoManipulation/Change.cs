using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net;
using System.Xml;
using System.Linq;

namespace PhotoManipulation
{
    class Change
    {
        public static void RenameImg(string path, DirectoryInfo source)
        {
            Directory.CreateDirectory($@"{path}\Renamed\");
            DirectoryInfo target = new DirectoryInfo($@"{path}\Renamed\");

            foreach (FileInfo file in source.GetFiles("*.jpg"))
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource img = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)img.Metadata;
                    var date = md.DateTaken;

                    string name;

                    if (date == null)
                        name = file.CreationTime.ToShortDateString() + "_" + file.Name;
                    else name = date.Replace(':', '-') + "_" + file.Name;

                    file.CopyTo(target + name, false);
                }
            }
        }

        public static void DateTaken(string path, DirectoryInfo source) //чекнуть размер шрифта
        {
            Directory.CreateDirectory($@"{path}\DateTaken\");
            DirectoryInfo target = new DirectoryInfo($@"{path}\DateTaken\");

            foreach (FileInfo file in source.GetFiles("*.jpg"))
            {
                string date;

                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource image = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)image.Metadata;
                    date = md.DateTaken;

                    if (date == null)
                        date = file.CreationTime.ToShortDateString();
                }

                Image img = Bitmap.FromFile(file.FullName);

                Graphics g = Graphics.FromImage(img);
                var myFont = new Font("Arial", 20);
                g.DrawString(date, myFont, new SolidBrush(Color.White), img.Width - img.Width / 15, 10);
                img.Save($@"X:\\Photos\Dubai\DateTaken\{file.Name}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                g = null;
                img = null;
            }
        }

        public static void SortByYear(string path, DirectoryInfo source)
        {
            foreach (FileInfo file in source.GetFiles("*.jpg"))
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource img = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)img.Metadata;
                    var date = md.DateTaken;
                    var creationTime = Convert.ToDateTime(date);
                    var i = Convert.ToString(creationTime.Year);

                    if (date == null)
                        date = Convert.ToString(file.CreationTime.Year);
                    else date = i;

                    Directory.CreateDirectory($@"{path}\{date}\");
                    DirectoryInfo target = new DirectoryInfo($@"{path}\{date}\");

                    file.CopyTo(target + file.Name, false);
                }
            }
        }

        public static void SortByPlace(string path, DirectoryInfo source)
        {
            foreach (FileInfo file in source.GetFiles("*.jpg"))
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource img = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)img.Metadata;

                    var image = Image.FromStream(fs);

                    string lat = null;
                    string lon = null;
                    var items = image.PropertyIdList;
                    foreach (var item in items) // знаю, топорно немного, но надо было как-то запилить проверку на 
                                                // существование атрибута, иначе выкидывает ошибку (проверка на null не работает)
                    {
                        if (item == 2)
                            lat = DecodeRational64u(image.GetPropertyItem(2));
                        if (item == 4)
                            lon = DecodeRational64u(image.GetPropertyItem(4));
                    }
                    if (lat == null || lon == null) continue;

                    string longlat = $"{lon},{lat}";

                    string requestUri = string.Format("https://geocode-maps.yandex.ru/1.x/?apikey=e8df3ad3-17cc-4557-b36f-854383906960&geocode={0}&kind=locality&results=1", longlat);

                    WebRequest request = WebRequest.Create(requestUri);
                    WebResponse response = request.GetResponse();

                    string location = null;

                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(reader.ReadToEnd());

                            var components = doc.GetElementsByTagName("Component");

                            foreach (XmlNode component in components)
                            {
                                if (component["kind"].InnerText == "locality")
                                    location = component["name"].InnerText;
                            }
                        }
                    }
                    response.Close();

                    Directory.CreateDirectory($@"{path}\{location}\");
                    DirectoryInfo target = new DirectoryInfo($@"{path}\{location}\");

                    file.CopyTo(target + file.Name, true);
                }
            }

            string DecodeRational64u(PropertyItem propertyItem) // честно подсмотрено в интернетах
            {
                uint dN = BitConverter.ToUInt32(propertyItem.Value, 0);
                uint dD = BitConverter.ToUInt32(propertyItem.Value, 4);
                uint mN = BitConverter.ToUInt32(propertyItem.Value, 8);
                uint mD = BitConverter.ToUInt32(propertyItem.Value, 12);
                uint sN = BitConverter.ToUInt32(propertyItem.Value, 16);
                uint sD = BitConverter.ToUInt32(propertyItem.Value, 20);

                decimal deg;
                decimal min;
                decimal sec;

                if (dD > 0) { deg = (decimal)dN / dD; } else { deg = dN; }
                if (mD > 0) { min = (decimal)mN / mD; } else { min = mN; }
                if (sD > 0) { sec = (decimal)sN / sD; } else { sec = sN; }

                return string.Format("{0}.{1}{2:###}", deg, min, sec.ToString().Replace(".", string.Empty));
            }
        }
    }
}
