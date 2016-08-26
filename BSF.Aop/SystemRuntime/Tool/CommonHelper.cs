using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.SystemRuntime.Tool
{
    public class CommonHelper
    {
        public static bool IsType(string fullTypeName, TypeDefinition type)
        {
            var find = GetType(fullTypeName,type);
            if (find != null)
                return true;
            else
                return false;
        }

        public static TypeDefinition GetType(string fullTypeName, TypeDefinition type)
        {
            TypeDefinition basetype = type;
            while (basetype != null)
            {
                if (basetype.FullName == fullTypeName)
                {
                    return basetype;
                }
                try
                {
                    basetype = (basetype.BaseType == null ? null : basetype.BaseType.Resolve());
                }
                catch (Exception exp)
                {
                    TypeReference typeref = AssemblyDefinition.ReadAssembly(InstuctionsHelper.LoadType(basetype.BaseType).Assembly.Location).MainModule.GetType(basetype.BaseType.FullName,false);
                    if(typeref!=null)
                        basetype = typeref.Resolve();
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(exp.Message);
                        throw exp;
                    }
                       
                }
            }
            return null;
        }

        public static List<CustomAttribute> GetAttribute(Collection<CustomAttribute> customAttributes, string fullTypeName)
        {
            var find = customAttributes.Where(y => GetType(fullTypeName, y.AttributeType.Resolve()) != null).ToList();
            return find;
        }

        public static bool HasAttribute(Collection<CustomAttribute> customAttributes, string fullTypeName)
        {
                var find = GetAttribute(customAttributes, fullTypeName);
                if (find != null && find.Count > 0)
                    return true;
                else
                    return false;
           
        }

        public static bool IsStartWith(string str,List<string> startwiths)
        {
            foreach (var s in startwiths)
            {
                if (str.StartsWith(s, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsContains(string str, List<string> contains)
        {
            foreach (var s in contains)
            {
                if (str.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        public static T Convert<T>(Object obj)
        {
            return (T)obj;
        }

        public static void AddOrEditDictionary<T>(Dictionary<string,object> dic,string key,T value)
        {
            if (!dic.ContainsKey(key))
                dic.Add(key, value);
            else
                dic[key] = value;
        }

        public static bool IsWeb()
        {
            bool isWeb = false;
            if (AppDomain.CurrentDomain != null && AppDomain.CurrentDomain.SetupInformation != null)
            {
                string webconfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                if (webconfig != null && System.IO.Path.GetFileName(webconfig).ToLower() == "web.config")
                {
                    isWeb = true;
                }
            }
            if (System.Web.HttpContext.Current != null)
                isWeb = true;
            return isWeb;
        }
    }
}
