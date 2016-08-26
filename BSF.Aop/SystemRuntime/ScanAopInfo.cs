using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.SystemRuntime
{
    /// <summary>
    /// 扫描出来的 Aop 的信息
    /// </summary>
    public class ScanAopInfo
    {
        /// <summary>
        /// 涉及Aop的类型信息
        /// </summary>
        public List<TypeAopInfo> Types { get; set; } = new List<TypeAopInfo>();
        /// <summary>
        /// 涉及Aop的方法的信息
        /// </summary>
        public List<MethodAopInfo> Methods { get; set; } = new List<MethodAopInfo>();
    }

    public class TypeAopInfo
    {
        public TypeDefinition Type { get; set; }
        public AssemblyDefinition Assembly { get;set;}

        public string AssemblyPath { get; set; }
    }

    public class MethodAopInfo:TypeAopInfo
    {
        public MethodDefinition Method { get; set; }
    }
}
