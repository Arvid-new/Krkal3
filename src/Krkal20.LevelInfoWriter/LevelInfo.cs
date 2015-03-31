using Krkal.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krkal20.LevelInfoWriter
{

    // vyznamy bitu u Level Tagu
    [Flags]
    enum eMMLevelTags
    {
        eMMLTcompleted = 1,		// zda je level dohrany
        eMMLTaccessible = 2,		// zda je pristupny k hrani
        eMMLTskipAccess = 4,		// zda je level pristupny diky necemu jinymu, nez ze ses k nemu dohral (pristupny vzdy, editoval s ho)
        eMMLTeditable = 8,		// editovatelny
        eMMLTlocked = 16,		// zamceny
        eMMLTnewNoAccess = 32,		// nove nakopirovany level, zatim neni pristupny kvuli restrikcim v pravech
        eMMLTdeleted = 64,		// smazany level, v postupu stale zabira pozici
        eMMLTedited = 128,		// tys ho editoval
        eMMLTopened = 256,		// odemceny level
        eMMLTskipable = 512,		// zda je level preskocitelny (neblokuje pozici), zda je adresar preskocitelny nebo je zanorovaci
        eMMLTalwEditable = 1024,		// zda je vzdy editovatelny
        eMMLTalwAccess = 2048,		// zda je vzdy pristupny
    };

    class LevelInfo
    {
        public String LVersion;
        public String Author;
        public String Game;
        public String Comment;
        public String Directory;
        public String Password;
        public double Difficulty;
        public eMMLevelTags Tags;
        public Dictionary<String, String> LocalNames;

        public LevelInfo(FSRegister reg, string fileName)
        {
            FSRegKey key;
            if (!(key = reg.FindKey("LVersion")).IsNull)
                LVersion = key.StringRead();
            if (LVersion == null)
                LVersion = fileName.Substring(fileName.Length - 19);
            if (!(key = reg.FindKey("Author")).IsNull)
                Author = key.StringRead();
            if (!(key = reg.FindKey("Game")).IsNull)
                Game = key.StringRead();
            if (!(key = reg.FindKey("Comment")).IsNull)
                Comment = key.StringRead();
            if (!(key = reg.FindKey("Directory")).IsNull)
                Directory = key.StringRead();
            if (!(key = reg.FindKey("Password")).IsNull)
                Password = key.StringRead();
            if (!(key = reg.FindKey("Difficulty")).IsNull)
                Difficulty = key.ReadD();
            if (!(key = reg.FindKey("Tags")).IsNull)
                Tags = (eMMLevelTags)key.ReadI();
            if (!(key = reg.FindKey("LocalNames")).IsNull)
            {
                LocalNames = new Dictionary<string, string>();
                key = key.Subregister.GetFirstKey();

                while (!key.IsNull)
                {
                    LocalNames.Add(key.Name, key.StringRead());
                    key = key.NextKey;
                }

            }

        }

        public LevelInfo(StreamReader reader)
        {
            Author = ReadStr(reader, "Author");
            Game = ReadStr(reader, "Game");
            Comment = ReadStr(reader, "Comment");
            Difficulty = double.Parse(ReadStr(reader, "Difficulty"));
            Tags = (eMMLevelTags)Enum.Parse(typeof(eMMLevelTags), ReadStr(reader, "Tags"));

            int localNamesCount = int.Parse(ReadStr(reader, "LocalNames"));
            if (localNamesCount > 0)
            {
                LocalNames = new Dictionary<string, string>();
                for (int f = 0; f < localNamesCount; f++)
                {
                    String line = reader.ReadLine();
                    String[] parts = line.Split(new char[] { ':' }, 3);
                    LocalNames.Add(parts[1], parts[2]);
                }
            }
        }



        private string ReadStr(StreamReader reader, string name)
        {
            String line = reader.ReadLine();
            if (!line.StartsWith(name))
                throw new InvalidOperationException(String.Format("Ocekaval jsem radku s '{0}' ale precetl jsem: {1}", name, line));
            String ret = line.Substring(name.Length+1);
            if (String.IsNullOrEmpty(ret))
                ret = null;

            return ret;
        }

        internal void Write(StreamWriter writer)
        {
            WriteStr(writer, "Author", Author);
            WriteStr(writer, "Game", Game);
            WriteStr(writer, "Comment", Comment);
            WriteValue(writer, "Difficulty", Difficulty);
            WriteValue(writer, "Tags", Tags);
            WriteValue(writer, "LocalNames", LocalNames == null ? 0 : LocalNames.Count);
            if (LocalNames != null) 
            {
                foreach (var pair in LocalNames)
                {
                    WriteStr(writer, "LocalNames:" + pair.Key, pair.Value);
                }
            }
        }

        private void WriteValue<T>(StreamWriter writer, string name, T value)
        {
            writer.WriteLine(name + ":" + value.ToString());
        }

        private void WriteStr(StreamWriter writer, string name, string value)
        {
            value = value ?? String.Empty;
            writer.WriteLine(name + ":" + value);
        }




        internal void Adjust(LevelInfo input)
        {
            this.Author = input.Author;
            this.Game = input.Game;
            this.Comment = input.Comment;
            this.Difficulty = input.Difficulty;
            this.Tags = input.Tags;
            this.LocalNames = input.LocalNames;
        }


        internal void SaveTo(FSRegister reg)
        {
            DeleteKey(reg, "LVersion");
            DeleteKey(reg, "Author");
            DeleteKey(reg, "Game");
            DeleteKey(reg, "Comment");
            DeleteKey(reg, "Directory");
            DeleteKey(reg, "Password");
            DeleteKey(reg, "Difficulty");
            DeleteKey(reg, "Tags");
            DeleteKey(reg, "LocalNames");


            if (Author != null) reg.AddKey("Author", FSRegKeyType.String).StringWrite(Author);
            if (Game != null) reg.AddKey("Game", FSRegKeyType.String).StringWrite(Game);
            if (Comment != null) reg.AddKey("Comment", FSRegKeyType.String).StringWrite(Comment);
            reg.AddKey("Difficulty", FSRegKeyType.Double).WriteD(Difficulty);
            reg.AddKey("Tags", FSRegKeyType.Int).WriteI((int)Tags);

            if (LocalNames != null)
            {
                var r = reg.AddKey("LocalNames", FSRegKeyType.Register).Subregister;
                foreach (var pair in LocalNames)
                {
                    r.AddKey(pair.Key, FSRegKeyType.String).StringWrite(pair.Value);
                }
            }

            if (Directory != null) reg.AddKey("Directory", FSRegKeyType.String).StringWrite(Directory);
            if (Password != null) reg.AddKey("Password", FSRegKeyType.String).StringWrite(Password);
            reg.AddKey("LVersion", FSRegKeyType.String).StringWrite(LVersion);
        }

        private static void DeleteKey(FSRegister reg, String keyName)
        {
            FSRegKey key;
            if (!(key = reg.FindKey(keyName)).IsNull)
                reg.DeleteKey(key);
        }
    }
}
