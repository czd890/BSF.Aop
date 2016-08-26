using BSF.Aop.SystemRuntime;
using BSF.Aop.SystemRuntime.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BSF.Aop
{

    /// <summary>
    /// Aop启动运行类
    /// 
    /// </summary>
    public static class AopStartLoader
    {
        //start 方法仅运行一次锁
        private static object _lock = new object();
        //start 方法仅运行一次
        private static bool hasrun = false;

        /// <summary>
        /// 项目环境下：自动检测vs是否配置好Aop注入的环境，若未配置则尝试自动配置（默认的vs project结构有效），并Aop注入exe文件（BSF.Aop.ILRun.exe）,否则会报错；请手工在PostBuildEvent 里面配置脚本。
        /// 非项目环境下:则在当前运行目录下面生成（BSF.Aop.ILRun.exe）。
        /// web项目在Application_Start调用,winform环境中请在program的main函数中调用此函数（用于初始化环境检查及自动配置）
        /// </summary>
        /// <param name="dirs"></param>
        public static void Start()
        {
            Start(null);
        }

       
        public static void Start(string[] dirs)
        {
            try
            {
                if (hasrun == false)
                {
                    lock (_lock)
                    {
                        if (hasrun == false)
                        {
                            hasrun = true;
                            List<string> ldirs = new List<string>();
                            if (dirs != null)
                            {
                                foreach (var dir in dirs)
                                {
                                    if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
                                    {
                                        ldirs.Add(dir);
                                    }
                                }
                            }
                            //检查Mono.Cecil.dll和Mono.Cecil.Pdb.dll文件是否引用
                            var type1 = typeof(Mono.Cecil.AssemblyDefinition);
                            var type2 = typeof(Mono.Cecil.Pdb.PdbReader);

                            AopIL(ldirs);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("Aop 出错" + exp.Message);
                throw exp;
            }
        }

        private static void AopIL(List<string> dirs)
        {
            var exeprovider = new AopExeProvider();
            //若是BSF.Aop.ILRun.exe启动
            if (AppDomain.CurrentDomain != null && AppDomain.CurrentDomain.SetupInformation != null && AppDomain.CurrentDomain.SetupInformation.ApplicationName == exeprovider.ExeFileName)
            {
                //检查是否有需要IL注入的点,若有则进行注入
                AopILProvider ilProvider = new AopILProvider();
                var files = ilProvider.GetScanAssemblieFiles(dirs);
                var scaninfo = ilProvider.GetAopInfo(files,true);
                if (scaninfo.Methods.Count > 0 || scaninfo.Types.Count > 0)
                {
                    ilProvider.IL(scaninfo);
                    ilProvider.SaveFiles(scaninfo);
                }
            }
            else
            {
                //检查是否有需要IL注入的点,若有则生成相关文件，并提示当前Aop未进行IL注入
                AopILProvider ilProvider = new AopILProvider();
                var files = ilProvider.GetScanAssemblieFiles(dirs);
                var scaninfo = ilProvider.GetAopInfo(files,false);
                if (scaninfo.Methods.Count > 0 || scaninfo.Types.Count > 0)
                {
                    //检查是否是vs项目状态;vs项目状态下，则注入vs .project 文件的PostBulid事件
                    if (exeprovider.RegisterProjectPostBuildEvent() == false)
                    {
                        //非project,则在bin下生成BSF.Aop.ILRun.exe
                        exeprovider.BuildExe(AppDomain.CurrentDomain.BaseDirectory);
                    }
                    string message = "检测到当前程序未进行静态IL注入,请重启或运行" + exeprovider.ExeFileName + "后再重启";

                    //如果是web，则自动重启;否则抛出错误
                    if (CommonHelper.IsWeb())
                    {
                        System.Diagnostics.Debug.WriteLine("【即将自动重启web站点】Aop 出错:" + message);
                        File.SetLastWriteTime(System.Web.HttpContext.Current.Server.MapPath("~/web.config"), DateTime.Now);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Aop 出错:" + message);
                        throw new AopException(message);
                    }
                }
               
            }
        }

    }
}
