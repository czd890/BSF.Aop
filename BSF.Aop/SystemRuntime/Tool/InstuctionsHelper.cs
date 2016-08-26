using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.SystemRuntime.Tool
{
    public static class InstuctionsHelper
    {
        public static VariableDefinition GetVariableDefinition(System.Collections.Generic.ICollection<VariableDefinition> variableDefinitions,string key)
        {
            foreach (var v in variableDefinitions)
            {
                if (v.Name == key)
                    return v;
            }
             
            return null;
        }

        public static Object Import(AssemblyDefinition assembly, Object obj)
        {
            if (obj != null)
            {
                if (obj is FieldReference)
                {
                    obj = assembly.MainModule.Import(obj as FieldReference);
                }
                else if (obj is MethodReference)
                {
                    obj = assembly.MainModule.Import(obj as MethodReference);
                }
                else if (obj is TypeReference)
                {
                    obj = assembly.MainModule.Import(obj as TypeReference);
                }
            }
            return obj;
        }

        public static T Import<T>(AssemblyDefinition assembly, T obj) where T: MemberReference
        {
            return (T) Import(assembly, (Object)obj);
        }

        public static Type LoadType(TypeReference type)
        {
            //优先通过默认方式获取类型
            var findtype = Type.GetType(type.FullName);
            if (findtype != null)
                return findtype;
            Dictionary<string,string> locations = new Dictionary<string, string>();
            if (type.Module.Assembly.MainModule != null && !string.IsNullOrWhiteSpace(type.Module.Assembly.MainModule.FullyQualifiedName))
            {
                locations.Add(type.Module.Assembly.FullName,type.Module.Assembly.MainModule.FullyQualifiedName);
            }
            //if(type.Scope!=null &&(type.Scope is AssemblyDefinition))
            foreach (var location in locations)
            {
                //优先通过路径，读取文件方式加载内存，不会出现读锁
                if (!string.IsNullOrWhiteSpace(location.Value))
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(c => c.FullName == location.Key);
                    if (assembly == null)
                    {
                        assembly = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(location.Value));
                    }
                    findtype = assembly.GetType(type.FullName);
                    if (findtype != null)
                        return findtype;
                }
            }
            if (type.Scope != null && type.Scope is Mono.Cecil.AssemblyNameReference)
            {
                try
                {
                    System.Reflection.Assembly.Load(((Mono.Cecil.AssemblyNameReference)type.Scope).FullName).GetType(type.FullName);
                }
                catch { }
            }
           

                //通过load的方式加载，此时文件会被读锁，造成文件无法更新的情况。
            return System.Reflection.Assembly.Load(type.Module.Assembly.FullName).GetType(type.FullName);
            
        }
    }
}
