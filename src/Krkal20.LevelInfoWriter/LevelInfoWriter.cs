using Krkal.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krkal20.LevelInfoWriter
{
    class LevelInfoWriter
    {
        private readonly String levels;
        private readonly String cfgFile;
        private readonly FS fs;

        public LevelInfoWriter(String levels, String cfgFile)
        {
            this.levels = levels;
            this.cfgFile = cfgFile;

            fs = FS.FileSystem;
        }


        public void Export()
        {
            using (var dir = fs.ReadDirectory(levels))
            using (var writer = new StreamWriter(cfgFile))
            {
                for (int f = 0; f < dir.Count; f++)
                {
                    if (dir.GetName(f).EndsWith(".lv", StringComparison.OrdinalIgnoreCase))
                    {
                        String path = Path.Combine(levels, dir.GetName(f), "!level");
                        using (var regFile = new FSRegisterFile(path, "KRKAL LEVEL"))
                        {
                            var li = new LevelInfo(regFile.Reg, Path.GetFileNameWithoutExtension(dir.GetName(f)));
                            writer.WriteLine();
                            writer.WriteLine(dir.GetName(f));
                            li.Write(writer);
                        }
                    }
                }
            }

        }


        public void Import()
        {
            using (var reader = new StreamReader(cfgFile))
            {
                LevelInfo input;
                String dirName;
                while ((input = ReadFromCfgFile(reader, out dirName)) != null) 
                {
                    String path = Path.Combine(levels, dirName, "!level");
                    String pathInfo = Path.Combine(levels, dirName, "!level.info");
                    using (var regFile = new FSRegisterFile(path, "KRKAL LEVEL"))
                    using (var regFileInfo = new FSRegisterFile(pathInfo, "KRKAL LEVEL I", true))
                    {
                        var li = new LevelInfo(regFile.Reg, Path.GetFileNameWithoutExtension(dirName));
                        li.Adjust(input);

                        li.SaveTo(regFile.Reg);
                        li.SaveTo(regFileInfo.Reg);

                        regFile.WriteFile();
                        regFileInfo.WriteFile();
                    }

                }

            }

        }

        private LevelInfo ReadFromCfgFile(StreamReader reader, out String dirName)
        {
            dirName = null;
            String line;
            line = reader.ReadLine();
            if (line == null)
                return null;

            dirName = reader.ReadLine();
            if (String.IsNullOrEmpty(dirName))
                return null;
            return new LevelInfo(reader);
        }

    }
}
