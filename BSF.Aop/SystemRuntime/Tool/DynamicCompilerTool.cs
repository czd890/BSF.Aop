using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime.Tool
{
    public class DynamicCompilerTool
    {
        public static void CompilerExe(string[] dllfiles,string code,string exepath)
        {

            // 1.CodeDomProvider
            CodeDomProvider objICodeCompiler = CodeDomProvider.CreateProvider("CSharp");

            // 2.CompilerParameters
            CompilerParameters objCompilerParameters = new CompilerParameters();
            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            foreach (var r in dllfiles)
            {
                if (!objCompilerParameters.ReferencedAssemblies.Contains(r))
                    objCompilerParameters.ReferencedAssemblies.Add(r);
            }
            
            objCompilerParameters.GenerateExecutable = true;
            objCompilerParameters.GenerateInMemory = false;
            objCompilerParameters.OutputAssembly = exepath;


            //objCompilerParameters.GenerateInMemory = false;
            //objCompilerParameters.CompilerOptions = "/target:library /optimize";
            //objCompilerParameters.OutputAssembly = @"C:\Users\cjy\Desktop\test\" + DateTime.Now.ToString("yymmddHHmmss") + ".dll";


            // 3.CompilerResults
            CompilerResults cr = objICodeCompiler.CompileAssemblyFromSource(objCompilerParameters, new string[] { code });
            

            if (cr.Errors.HasErrors)
            {
                string errors = "";
                foreach (CompilerError err in cr.Errors)
                {
                    errors += string.Format("错误:{0},行号:{1},列号:{2},文件名:{3}\n", err.ErrorText, err.Line, err.Column, err.FileName);
                }
                if (!string.IsNullOrWhiteSpace(errors))
                {
                    throw new AopException("编译错误信息:" + errors);
                }
            }

        }
    }
}
