using BSF.Aop.SystemRuntime.Tool;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime
{
    public class AopExeProvider
    {
        public string ExeFileName = "BSF.Aop.ILRun.exe";
        public void BuildExe(string dir)
        {
            string aopdllpath =this.GetType().Assembly.Location;
            string monocecilpath = typeof(Mono.Cecil.AssemblyDefinition).Assembly.Location;
            string monocecilpdbpath = typeof(Mono.Cecil.Pdb.PdbReader).Assembly.Location;
            string winfromdllpath = "System.Windows.Forms.dll";//System.Reflection.Assembly.Load("System.Windows.Forms").Location;

            string[] dllpaths = new[] { aopdllpath,monocecilpath, monocecilpdbpath };

            foreach (var dllpath in dllpaths)
            {
                //if (!System.IO.File.Exists(aopdllpath))
                //{
                string path = dir.TrimEnd('\\') + "\\" + System.IO.Path.GetFileName(dllpath);
                if (dllpath != path)
                {
                    IOHelper.CreateDirectory(path);
                    System.IO.File.Copy(dllpath, path, true);
                }

                //}
            }

            string aopexepath = dir.TrimEnd('\\') + "\\" + ExeFileName;
            //if (!System.IO.File.Exists(aopexepath))
            //{
            DynamicCompilerTool.CompilerExe(new string[] { this.GetType().Assembly.Location, winfromdllpath }, AopConfig.ExeCode, aopexepath);
            //}
        }

        public bool RegisterProjectPostBuildEvent()
        {
            string path = this.GetType().Assembly.Location;
            string projectfilepath = IOHelper.GetProjectFile(path);
            if (projectfilepath == null)
                projectfilepath = IOHelper.GetProjectFile(AppDomain.CurrentDomain.BaseDirectory);//兼容web情况
            if (projectfilepath != null)
            {
                string relateexedir = @"\packages.BSF.Aop\";
                //生成运行目录
                string aopdir = System.IO.Path.GetDirectoryName(projectfilepath).TrimEnd('\\')+ relateexedir;
                BuildExe(aopdir);

                string postbulideventcommand = "";
                if (CommonHelper.IsWeb())//兼容web情况
                {
                    postbulideventcommand =
@"xcopy $(SolutionDir)\{ProjectName}\bin\{BsfAopDll} $(SolutionDir)\{ProjectName}\{RelateExeDir} /Y
call ""$(SolutionDir)\{ProjectName}\{RelateExeDir}{ExeFileName}"" msgbox $(SolutionDir)\{ProjectName}\bin\"
.Replace("{BsfAopDll}", System.IO.Path.GetFileName(this.GetType().Assembly.Location))
.Replace("{RelateExeDir}", relateexedir.TrimStart('\\'))
.Replace("{ExeFileName}", ExeFileName)
.Replace("{ProjectName}",System.IO.Path.GetFileNameWithoutExtension(projectfilepath));
                }
                else
                {
                    postbulideventcommand =
@"xcopy $(OutDir){BsfAopDll} $(ProjectDir){RelateExeDir} /Y
call ""$(ProjectDir){RelateExeDir}{ExeFileName}"" msgbox $(TargetDir)"
.Replace("{BsfAopDll}", System.IO.Path.GetFileName(this.GetType().Assembly.Location))
.Replace("{RelateExeDir}", relateexedir.TrimStart('\\'))
.Replace("{ExeFileName}", ExeFileName);
                }

                //注入项目.csproj文件
                Project project = null;
                try { project = new Project(projectfilepath); } catch { }
                if (project == null)
                { project = new ProjectCollection().LoadProject(projectfilepath); }//兼容web情况
                //注入PostBuildEvent事件
                if (project.GetProperty("PostBuildEvent") != null)
                {
                    if (!project.GetProperty("PostBuildEvent").UnevaluatedValue.Contains(ExeFileName))
                    {
                        project.GetProperty("PostBuildEvent").UnevaluatedValue += postbulideventcommand + "\n";
                        project.Save();
                    }
                }
                else
                {
                    project.SetProperty("PostBuildEvent", postbulideventcommand + "\n");//兼容web情况
                    project.Save();
                }

                return true;
            }

            return false;
        }
    }
}
