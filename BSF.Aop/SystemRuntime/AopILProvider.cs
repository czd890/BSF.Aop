
using BSF.Aop.Attributes.Around;
using BSF.Aop.Attributes.Base;
using BSF.Aop.Attributes.NotifyProperty;
using BSF.Aop.SystemRuntime.Tool;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;

namespace BSF.Aop.SystemRuntime
{
    /// <summary>
    /// Aop IL 静态织如提供者
    /// </summary>
    public class AopILProvider
    {

        List<BaseAopIL> ILs = new List<BaseAopIL>();

        public AopILProvider()
        {
            //子类优先织入，父类后续织入
            ILs.Add(new AroundAopIL());
            ILs.Add(new NotifyPropertyAopIL());
        }
        /// <summary>
        /// 获取扫描的程序集文件路径
        /// </summary>
        /// <returns></returns>
        public List<string> GetScanAssemblieFiles(List<string> dirs)
        {
            if (dirs == null) { dirs = new List<string>(); }
            List<string> filepaths = new List<string>();


            if (System.Web.Hosting.HostingEnvironment.IsHosted)
            {
                try
                {
                    //默认添加的程序集引用
                    foreach (var assembile in BuildManager.GetReferencedAssemblies().Cast<Assembly>())
                    {
                        if (!string.IsNullOrWhiteSpace(assembile.CodeBase) && !filepaths.Contains(assembile.CodeBase))
                        {
                            filepaths.Add(assembile.CodeBase);
                        }
                    }
                }
                catch (Exception exp) { }
            }

            List<string> defaultdir = new List<string>() {
                AppDomain.CurrentDomain.BaseDirectory,//当前文件夹的程序集引用
                AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + @"\bin\"//web下bin目录
            };

            foreach (var dir in defaultdir)
            {
                if (System.IO.Directory.Exists(dir))
                {
                    if (!dirs.Contains(dir))
                        dirs.Add(dir);
                }
            }

            foreach (var dir in dirs)
            {
                if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
                {
                    foreach (var filepath in System.IO.Directory.GetFiles(dir))
                    {
                        if ((filepath.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                            filepath.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)) && !filepaths.Contains(filepath))
                        {
                            filepaths.Add(filepath);
                        }
                    }
                }
            }

            //排除程序集
            List<string> result = new List<string>();
            foreach (var filepath in filepaths)
            {
                if (System.IO.File.Exists(filepath))
                {
                    //文件不包含XX开头
                    if (!CommonHelper.IsStartWith(System.IO.Path.GetFileName(filepath), AopConfig.IncludeAssemblyStartWith))
                    {
                        //排除文件以XX开头
                        if (CommonHelper.IsStartWith(System.IO.Path.GetFileName(filepath), AopConfig.NoIncludeAssemblyStartWith))
                            continue;
                        //排除特定版权
                        var fileinfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filepath);
                        if (fileinfo != null && fileinfo.LegalCopyright != null && CommonHelper.IsContains(fileinfo.LegalCopyright, AopConfig.NoIncludeAssemblyCopyright))
                            continue;
                    }
                    result.Add(filepath);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取需要注入的Types和Methods
        /// </summary>
        public ScanAopInfo GetAopInfo(List<string> assemblieFiles, bool isreadmemory)
        {
            ScanAopInfo info = new ScanAopInfo();
            foreach (var filepath in assemblieFiles)
            {
                //读取程序集
                AssemblyDefinition assembly = null;
                //if (isreadmemory)
                //{ assembly = AssemblyDefinition.ReadAssembly(new System.IO.MemoryStream(System.IO.File.ReadAllBytes(filepath))); }
                //else
                bool pdb = IOHelper.HasPdbFile(filepath);
                { assembly = AssemblyDefinition.ReadAssembly(filepath, new ReaderParameters { ReadSymbols = pdb }); }
                //获取BaseAopAttribute类型的类型 //可以采用并行forearch加快速度,注意加lock
                // (Mono.Cecil.dll 似乎不支持并行，会报错，故实际并行度为1，并不采用并行,代码保留)
                foreach (var type in assembly.MainModule.Types)
                {
                    if (CommonHelper.HasAttribute(type.CustomAttributes, typeof(BSF.Aop.Attributes.Base.BaseAopAttribute).FullName))
                    {

                        info.Types.Add(new TypeAopInfo() { Assembly = assembly, Type = type, AssemblyPath = filepath });

                    }
                }
                //可以采用并行forearch加快速度,注意加lock
                // (Mono.Cecil.dll 似乎不支持并行，会报错，故实际并行度为1，并不采用并行,代码保留)
                foreach (var type in assembly.MainModule.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        //获取BaseAopAttribute类型的方法
                        var find = CommonHelper.HasAttribute(method.CustomAttributes, typeof(BSF.Aop.Attributes.Base.BaseAopAttribute).FullName);
                        if (find == true)
                        {

                            info.Methods.Add(new MethodAopInfo { Assembly = assembly, Method = method, Type = type, AssemblyPath = filepath });

                        }
                    }
                }
            }

            return info;
        }

        public void IL(ScanAopInfo scanAopInfo)
        {
            foreach (var il in ILs)
            {
                il.IL(scanAopInfo);
            }


        }

        public void SaveFiles(ScanAopInfo scanAopInfo)
        {
            //回写程序集
            var assemblys = new Dictionary<string, AssemblyDefinition>();
            foreach (var m in scanAopInfo.Methods)
            {
                if (!assemblys.ContainsKey(m.AssemblyPath))
                {
                    assemblys.Add(m.AssemblyPath, m.Assembly);
                }
            }

            foreach (var m in scanAopInfo.Types)
            {
                if (!assemblys.ContainsKey(m.AssemblyPath))
                {
                    assemblys.Add(m.AssemblyPath, m.Assembly);
                }
            }

            foreach (var a in assemblys)
            {
                bool pdb = IOHelper.HasPdbFile(a.Key);
                a.Value.Write(a.Key, new WriterParameters { WriteSymbols = pdb });
            }
        }
    }
}
