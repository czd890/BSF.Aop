using BSF.Aop.Attributes.Base;
using BSF.Aop.SystemRuntime;
using BSF.Aop.SystemRuntime.Tool;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace BSF.Aop.Attributes.Around
{
    public class AroundAopIL: BaseAopIL
    {
        public override void IL(ScanAopInfo info)
        {
            foreach (var method in info.Methods)
            {
                try
                {
                    var assembly = method.Assembly;
                    var attrs = CommonHelper.GetAttribute(method.Method.CustomAttributes, typeof(AroundAopAttribute).FullName);
                    foreach (var attr in attrs)
                    {
                        //必须实现默认构造函数
                        if (InstuctionsHelper.LoadType(attr.AttributeType).GetConstructor(Type.EmptyTypes) == null)
                        {
                            throw new AopException(string.Format("Type:{0} 必须实现默认无参构造函数", attr.AttributeType.FullName));
                        }

                        var ilProcessor = method.Method.Body.GetILProcessor();
                        ilProcessor.Body.InitLocals = true;

                        assembly.MainModule.Import(attr.AttributeType.Resolve());

                        BeforeIL(method, attr);
                        AfterIL(method, attr);

                        //移除属性，下次不再注入
                        method.Method.CustomAttributes.Remove(attr);
                    }
                }
                catch (Exception exp)
                {
                    throw new AopException("AroundAopIL出错,方法:"+method.Method.FullName, exp);
                }
            }
        }

        private void BeforeIL(MethodAopInfo info, CustomAttribute attribute)
        {
            var assembly = info.Assembly;var method = info.Method;
            var ilProcessor = method.Body.GetILProcessor();
            //L_0001: newobj instance void[mscorlib] System.Collections.Generic.Dictionary`2 < string, object>::.ctor()

            var firstInstruction = ilProcessor.Body.Instructions.First();
            List<Instruction> beforeInstructions = new List<Instruction>();

            //Dictionary<string, object> _bsf_dic = new Dictionary<string, object>();
            var dictype = assembly.MainModule.Import(typeof(System.Collections.Generic.Dictionary<string, object>));
            ilProcessor.Body.Variables.Add(new VariableDefinition("bsf_Dictionary", dictype));
            beforeInstructions.Add(ilProcessor.Create(OpCodes.Nop));
            beforeInstructions.Add(ilProcessor.Create(OpCodes.Newobj, assembly.MainModule.Import(typeof(System.Collections.Generic.Dictionary<string, object>).GetConstructor(Type.EmptyTypes))));
            beforeInstructions.Add(Instruction.Create(OpCodes.Stloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_Dictionary")));

            /* CommonHelper.AddOrEditDictionary<string>(dic, "a", a);
            ldloc.0 
            L_0008: ldstr "a"
            L_000d: ldarg.1 
            L_000e: call void [BSF.Aop]BSF.Aop.SystemRuntime.Tool.CommonHelper::AddOrEditDictionary<string>(class [mscorlib]System.Collections.Generic.Dictionary`2<string, object>, string, !!0)
            L_0013: nop 
             */

            byte index = 0;

            foreach (var t in method.Parameters)
            {
                if (t.IsReturnValue == false&&t.ParameterType.IsByReference==false)//排除return ref,out
                {
                    index++;
                    beforeInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_Dictionary")));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Ldstr, t.Name));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Ldarg_S, t));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Call, assembly.MainModule.Import(typeof(BSF.Aop.SystemRuntime.Tool.CommonHelper).GetMethod("AddOrEditDictionary").MakeGenericMethod(InstuctionsHelper.LoadType(t.ParameterType)))));
                    beforeInstructions.Add(ilProcessor.Create(OpCodes.Nop));
                }
            }

            /*
             *  var _bsf_around = new BSF.Aop.Attributes.AroundInfo(System.Reflection.MethodBase.GetCurrentMethod(), _bsf_dic, this);
    L_002e: call class [mscorlib]System.Reflection.MethodBase [mscorlib]System.Reflection.MethodBase::GetCurrentMethod()
    L_0033: ldloc.0 
    L_0034: ldarg.0 
    L_0035: newobj instance void [BSF.Aop]BSF.Aop.Attributes.AroundInfo::.ctor(class [mscorlib]System.Reflection.MethodBase, class [mscorlib]System.Collections.Generic.Dictionary`2<string, object>, object)
    L_003a: stloc.1 
             */
            var dictype2 = assembly.MainModule.Import(typeof(BSF.Aop.Attributes.Around.AroundInfo));

            ilProcessor.Body.Variables.Add(new VariableDefinition("bsf_AroundInfo", dictype2));
            beforeInstructions.Add(Instruction.Create(OpCodes.Call, assembly.MainModule.Import(typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))));
            beforeInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_Dictionary")));
            if (method.IsStatic == false)
            {
                beforeInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                beforeInstructions.Add(Instruction.Create(OpCodes.Newobj, assembly.MainModule.Import(typeof(BSF.Aop.Attributes.Around.AroundInfo).GetConstructor(new Type[] { typeof(System.Reflection.MethodBase), typeof(System.Collections.Generic.Dictionary<string, object>), typeof(object) }))));
            }
            else
            {
                beforeInstructions.Add(Instruction.Create(OpCodes.Newobj, assembly.MainModule.Import(typeof(BSF.Aop.Attributes.Around.AroundInfo).GetConstructor(new Type[] { typeof(System.Reflection.MethodBase), typeof(System.Collections.Generic.Dictionary<string, object>) }))));
            }
            beforeInstructions.Add(Instruction.Create(OpCodes.Stloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_AroundInfo")));



            /*new BSF.Aop.Attributes.AroundAopAttribute().Before(_bsf_around);
    L_0096: newobj instance void [BSF.Aop]BSF.Aop.Attributes.AroundAopAttribute::.ctor()
    L_009b: ldloc.1 
    L_009c: callvirt instance void [BSF.Aop]BSF.Aop.Attributes.AroundAopAttribute::Before(class [BSF.Aop]BSF.Aop.Attributes.AroundInfo)

             */
            beforeInstructions.Add(Instruction.Create(OpCodes.Newobj, assembly.MainModule.Import(InstuctionsHelper.LoadType(attribute.AttributeType).GetConstructor(Type.EmptyTypes))));
            beforeInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_AroundInfo")));
            beforeInstructions.Add(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.Import(InstuctionsHelper.LoadType(attribute.AttributeType).GetMethod("Before"))));

            /*a = CommonHelper.Convert<string>(info.Params["a"]);
   L_003a: ldloc.1 
   L_003b: callvirt instance class [mscorlib]System.Collections.Generic.Dictionary`2<string, object> [BSF.Aop]BSF.Aop.Attributes.AroundInfo::get_Params()
   L_0040: ldstr "a"
   L_0045: callvirt instance !1 [mscorlib]System.Collections.Generic.Dictionary`2<string, object>::get_Item(!0)
   L_004a: call !!0 [BSF.Aop]BSF.Aop.SystemRuntime.Tool.CommonHelper::Convert<string>(object)
   L_004f: starg.s a

            */
            foreach (var t in method.Parameters)
            {
                if (t.IsReturnValue == false && t.ParameterType.IsByReference == false)
                {
                    beforeInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_AroundInfo")));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(BSF.Aop.Attributes.Around.AroundInfo).GetMethod("get_Params"))));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Ldstr, t.Name));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.Collections.Generic.Dictionary<string, object>).GetMethod("get_Item"))));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Call, assembly.MainModule.Import(typeof(BSF.Aop.SystemRuntime.Tool.CommonHelper).GetMethod("Convert").MakeGenericMethod(InstuctionsHelper.LoadType(t.ParameterType)))));
                    beforeInstructions.Add(Instruction.Create(OpCodes.Starg_S, t));
                }
            }
            //插入指令
            foreach (var i in beforeInstructions)
            {
                ilProcessor.InsertBefore(firstInstruction, i);
            }
        }

        public void AfterIL(MethodAopInfo info, CustomAttribute attribute)
        {
            var assembly = info.Assembly; var method = info.Method;
            var ilProcessor = method.Body.GetILProcessor();

            List<Instruction> lastInstructions = new List<Instruction>();

            //CommonHelper.AddOrEditDictionary<ScanAopInfo>(dic, "point", point);
            int index = 0;

            foreach (var t in method.Parameters)
            {
                if (t.IsReturnValue == false && t.ParameterType.IsByReference == false)
                {
                    index++;
                    lastInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_Dictionary")));
                    lastInstructions.Add(Instruction.Create(OpCodes.Ldstr, t.Name));
                    lastInstructions.Add(Instruction.Create(OpCodes.Ldarg_S, t));
                    lastInstructions.Add(Instruction.Create(OpCodes.Call, assembly.MainModule.Import(typeof(BSF.Aop.SystemRuntime.Tool.CommonHelper).GetMethod("AddOrEditDictionary").MakeGenericMethod(InstuctionsHelper.LoadType(t.ParameterType)))));
                    lastInstructions.Add(ilProcessor.Create(OpCodes.Nop));
                }
            }
            //new AroundAopAttribute().After(info2);
            lastInstructions.Add(Instruction.Create(OpCodes.Newobj, assembly.MainModule.Import(InstuctionsHelper.LoadType(attribute.AttributeType).GetConstructor(Type.EmptyTypes))));
            lastInstructions.Add(Instruction.Create(OpCodes.Ldloc_S, InstuctionsHelper.GetVariableDefinition(ilProcessor.Body.Variables, "bsf_AroundInfo")));
            lastInstructions.Add(Instruction.Create(OpCodes.Callvirt, assembly.MainModule.Import(InstuctionsHelper.LoadType(attribute.AttributeType).GetMethod("After"))));

            var lastInstruction = method.Body.Instructions[method.Body.Instructions.Count - 1];

            //插入指令
            foreach (var i in lastInstructions)
            {
                ilProcessor.InsertBefore(lastInstruction, i);
            }
        }
    }
}
