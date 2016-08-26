using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime.Tool
{
    public class IOHelper
    {
        public static bool HasPdbFile(string filepath)
        {
            string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
            string dir = System.IO.Path.GetDirectoryName(filepath);
            string filepdb = dir.TrimEnd('\\') + "\\" + filename + ".pdb";
            if (System.IO.File.Exists(filepdb))
                return true;
            else
                return false;
        }
        public static string GetProjectFile(string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dir)||!System.IO.Directory.Exists(dir))
                return null;
            dir = dir.TrimEnd('\\') + "\\";
            string[] endwith = new[] { @"\bin\debug\", @"\bin\release\" };
            foreach (var e in endwith)
            {
                if (dir.EndsWith(e, StringComparison.CurrentCultureIgnoreCase))
                   dir = dir.Remove(dir.Length - e.Length, e.Length);
            }
            if (string.IsNullOrWhiteSpace(dir) || !System.IO.Directory.Exists(dir))
                return null;
            var projectfile = System.IO.Directory.GetFiles(dir).Where(c => c.EndsWith(".csproj",StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            return projectfile;
        }

        /// <summary>
        /// 根据文件路径，创建文件对应的文件夹，若已存在则跳过
        /// </summary>
        /// <param name="filepath"></param>
        public static void CreateDirectory(string filepath)
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(filepath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
            }
            catch (Exception exp)
            {
                throw new Exception("路径" + filepath, exp);
            }
        }
    }
}
