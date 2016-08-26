using BSF.Aop.Attributes.Base;
using BSF.Aop.SystemRuntime;
using BSF.Aop.SystemRuntime.Tool;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.Attributes.NotifyProperty
{
    public class NotifyPropertyAopIL : BaseAopIL
    {
        public override void IL(ScanAopInfo info)
        {
            foreach (var aoptype in info.Types)
            {
                try
                {
                    var assembly = aoptype.Assembly; var type = aoptype.Type;
                    var find = CommonHelper.HasAttribute(type.CustomAttributes, typeof(NotifyPropertyChangedAopAttribute).FullName);
                    if (find)
                    {
                        PropertysIL(assembly, type);

                        //移除属性，包括重复定义
                        var attrs = CommonHelper.GetAttribute(type.CustomAttributes, typeof(NotifyPropertyChangedAopAttribute).FullName);
                        foreach (var attr in attrs)
                        {
                            type.CustomAttributes.Remove(attr);
                        }
                    }
                }
                catch (Exception exp)
                {
                    throw new AopException("NotifyPropertyAopIL出错,Type:"+aoptype.Type.FullName, exp);
                }
            }
        }

        private void PropertysIL(AssemblyDefinition assembly,TypeDefinition type)
        {
            //模板
            var notifyPropertyTemplateType = assembly.MainModule.Import(typeof(NotifyPropertyTemplate)).Resolve();

            //PropertyChangedEventHandler PropertyChanged = null;
            foreach (var f in notifyPropertyTemplateType.Fields)
            {
                var f2 = new FieldDefinition(f.Name, f.Attributes, f.FieldType);
                MapperHelper.CopyProperty(f, f2);
                f2.DeclaringType = type;
                f2.FieldType = InstuctionsHelper.Import(assembly, f.FieldType);
                type.Fields.Add(f2);
            }
            //public void add_PropertyChanged(PropertyChangedEventHandler value)
            //public void remove_PropertyChanged(PropertyChangedEventHandler value)
            //void NotifyPropertyChanged(string info)
            foreach (var m in notifyPropertyTemplateType.Methods)
            {
                if (m.IsConstructor)
                    continue;
                var m2 = new MethodDefinition(m.Name, m.Attributes, m.ReturnType);
                MapperHelper.CopyProperty(m, m2);
                m2.DeclaringType = type;
                foreach (var p in m.Parameters)
                {
                    m2.Parameters.Add(p);
                }
                foreach (var p in m2.Parameters)
                {
                    p.ParameterType = InstuctionsHelper.Import(assembly, p.ParameterType);
                }
                foreach (var v in m2.Body.Variables)
                {
                    v.VariableType = InstuctionsHelper.Import(assembly, v.VariableType);
                }

                foreach (var i in m2.Body.Instructions)
                {
                    if (i.Operand != null)
                    {
                        if (i.Operand is FieldDefinition && type.Fields.FirstOrDefault(c => c.Name == (i.Operand as FieldDefinition).Name) != null)
                        {
                            i.Operand = type.Fields.FirstOrDefault(c => c.Name == (i.Operand as FieldDefinition).Name);
                        }
                        else
                        { i.Operand = InstuctionsHelper.Import(assembly, i.Operand); }
                    }

                }

                type.Methods.Add(m2);
            }

            //public event PropertyChangedEventHandler PropertyChanged;
            foreach (var e in notifyPropertyTemplateType.Events)
            {
                var e2 = new EventDefinition(e.Name, e.Attributes, e.EventType);
                MapperHelper.CopyProperty(e, e2);
                e2.DeclaringType = type;
                e2.AddMethod = type.Methods.FirstOrDefault(c => c.Name == e2.AddMethod.Name);
                e2.RemoveMethod = type.Methods.FirstOrDefault(c => c.Name == e2.RemoveMethod.Name);
                type.Events.Add(e2);
            }

            //属性注入
            foreach (var p in type.Properties)
            {
                if (CommonHelper.HasAttribute(type.CustomAttributes, typeof(NoAopAttribute).FullName) == true)
                    continue;
                if (p.SetMethod != null)
                {
                    var method = p.SetMethod;
                    var ilProcessor = method.Body.GetILProcessor();
                    var lastInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];

                    /*
ldarg.0 
L_0009: ldstr "CustomerName"
L_000e: call instance void BSF.Aop.Test.NotifyTest::NotifyPropertyChanged(string)
L_0013: nop 

        */
                    List<Instruction> lastInstructions = new List<Instruction>();
                    lastInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    lastInstructions.Add(Instruction.Create(OpCodes.Ldstr, p.Name));
                    lastInstructions.Add(Instruction.Create(OpCodes.Call, type.Methods.FirstOrDefault(c => c.Name == "NotifyPropertyChanged")));
                    lastInstructions.Add(Instruction.Create(OpCodes.Nop));

                    //插入指令
                    foreach (var i in lastInstructions)
                    {
                        ilProcessor.InsertBefore(lastInstruction, i);
                    }
                }
            }
        }
    }
}
