using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime
{
    /// <summary>
    /// Aop配置信息
    /// </summary>
    public static class AopConfig
    {
        /// <summary>
        /// 排除程序集（名称以XXX开头）
        /// </summary>
        public static List<string> NoIncludeAssemblyStartWith = new List<string> { "System.", "Microsoft.","Mono." };
        /// <summary>
        /// 包含程序集（文件名称以XXX开头）
        /// </summary>
        public static List<string> IncludeAssemblyStartWith = new List<string> { };
        /// <summary>
        /// 排除程序集（文件属性中版权信息包含XXX）
        /// </summary>
        public static List<string> NoIncludeAssemblyCopyright = new List<string> { "Microsoft" };
        /// <summary>
        /// Aop程序集扫描并行度，用于提高扫描速度
        /// 备注"Mono.Cecil 内部似乎不支持并行，否则会报错;故无法使用并行方式,故此处必须为1
        /// </summary>
        public static int ScanAssemblyParallelNumber = 1;
        /// <summary>
        /// ILRun.EXE 代码
        /// </summary>
        public static string ExeCode = @"
using System;
static class Program
    {
        static void Main(string[] args)
        {
            string logfile = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + ""\\"" + ""aop_log.txt"";
            string logtype = ""log"";//日志类型:log,msgbox,error
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                logtype = args[0];
                System.IO.File.AppendAllText(logfile, ""日志类型: "" + logtype + ""\r\n\r\n"");
            }
            if (args != null && args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
            {
                dir = args[1];
                System.IO.File.AppendAllText(logfile, ""生成目录: "" + dir + ""\r\n\r\n"");
            }
            try
            {
                BSF.Aop.AopStartLoader.Start(new string[] { dir });
                System.IO.File.AppendAllText(logfile, ""生成成功, 时间: "" + DateTime.Now.ToString(""yyyyMMddHHmmss"") + ""\r\n\r\n"");
            }
            catch (Exception exp)
            {
                System.IO.File.AppendAllText(logfile, ""生成失败: "" + exp.Message + ""\r\n\r\n"" + exp.StackTrace + ""\r\n\r\n"");
                if (logtype == ""msgbox"")
                {System.Windows.Forms.MessageBox.Show(""Aop注入失败[详情看aop_log.txt]:"" + exp.Message);}
                if (logtype == ""error"")
                {throw exp;}
            }

        }
    }
";
    }
}
