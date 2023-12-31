// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ABIStress
{
    class Callee
    {
        private static readonly MethodInfo s_memoryMarshalCreateReadOnlySpanMethod =
            typeof(MemoryMarshal).GetMethod("CreateReadOnlySpan").MakeGenericMethod(typeof(byte));
        private static readonly MethodInfo s_hashCodeAddBytesMethod =
            typeof(HashCode).GetMethod("AddBytes");
        private static readonly MethodInfo s_hashCodeToHashCodeMethod =
            typeof(HashCode).GetMethod("ToHashCode");

        public Callee(string name, List<TypeEx> parameters)
        {
            Name = name;
            Parameters = parameters;
            ArgStackSizeApprox = Program.Abi.ApproximateArgStackAreaSize(Parameters);
        }

        public string Name { get; }
        public List<TypeEx> Parameters { get; }
        public int ArgStackSizeApprox { get; }
        public DynamicMethod Method { get; private set; }
        public Dictionary<CallingConvention, Type> PInvokeDelegateTypes { get; private set; }

        public void Emit()
        {
            if (Method != null)
                return;

            Method = new DynamicMethod(
                Name, typeof(int), Parameters.Select(t => t.Type).ToArray(), typeof(Program));

            ILGenerator g = Method.GetILGenerator();
            LocalBuilder hashCode = g.DeclareLocal(typeof(HashCode));

            if (Config.Verbose)
                Program.EmitDumpValues("Callee's incoming args", g, Parameters.Select((t, i) => new ArgValue(t, i)));

            g.Emit(OpCodes.Ldloca, hashCode);
            g.Emit(OpCodes.Initobj, typeof(HashCode));

            for (int i = 0; i < Parameters.Count; i++)
            {
                TypeEx pm = Parameters[i];
                foreach ((int start, int end) in pm.DataSegments)
                {
                    g.Emit(OpCodes.Ldloca, hashCode);

                    g.Emit(OpCodes.Ldarga, checked((short)i));
                    if (start > 0)
                    {
                        g.Emit(OpCodes.Ldc_I4, start);
                        g.Emit(OpCodes.Conv_I);
                        g.Emit(OpCodes.Add);
                    }

                    g.Emit(OpCodes.Ldc_I4, end - start);
                    g.Emit(OpCodes.Call, s_memoryMarshalCreateReadOnlySpanMethod);

                    g.Emit(OpCodes.Call, s_hashCodeAddBytesMethod);
                }
            }

            g.Emit(OpCodes.Ldloca, hashCode);
            g.Emit(OpCodes.Call, s_hashCodeToHashCodeMethod);
            g.Emit(OpCodes.Ret);
        }

        private static ModuleBuilder s_delegateTypesModule;
        private static ConstructorInfo s_unmanagedFunctionPointerCtor =
            typeof(UnmanagedFunctionPointerAttribute).GetConstructor(new[] { typeof(CallingConvention) });

        public void EmitPInvokeDelegateTypes()
        {
            if (PInvokeDelegateTypes != null)
                return;

            PInvokeDelegateTypes = new Dictionary<CallingConvention, Type>();

            if (s_delegateTypesModule == null)
            {
                AssemblyBuilder delegates = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ABIStress_Delegates"), AssemblyBuilderAccess.RunAndCollect);
                s_delegateTypesModule = delegates.DefineDynamicModule("ABIStress_Delegates");
            }

            foreach (CallingConvention cc in Program.Abi.PInvokeConventions)
            {
                // This code is based on DelegateHelpers.cs in System.Linq.Expressions.Compiler
                TypeBuilder tb =
                    s_delegateTypesModule.DefineType(
                        $"{Name}_Delegate_{cc}",
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass,
                        typeof(MulticastDelegate));

                tb.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    new[] { typeof(object), typeof(IntPtr) })
                  .SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

                tb.DefineMethod(
                    "Invoke",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    typeof(int),
                    Parameters.Select(t => t.Type).ToArray())
                  .SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

                tb.SetCustomAttribute(new CustomAttributeBuilder(s_unmanagedFunctionPointerCtor, new object[] { cc }));
                PInvokeDelegateTypes.Add(cc, tb.CreateType());
            }
        }
    }
}
