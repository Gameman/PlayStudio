using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using System.Linq;
using Mono.Cecil.Cil;
using Play.Studio.Core.Services;
using Play.Studio.Core;
using Play.Studio.Core.Utility;
using System.Reflection;
using log4net.Repository.Hierarchy;
using log4net;

namespace Play.Studio.AOP
{
    /// <summary>
    /// AOP
    /// </summary>
    public static class AOP
    {
        private static string                                   AopFullName     = typeof(AopAttribute).AssemblyQualifiedName;
        private static Dictionary<string, AssemblyDefinition>   AsmDefines      = new Dictionary<string,AssemblyDefinition>();
        private static TypeDefinition                           EmptyTypeDefine = new TypeDefinition("e", "e", Mono.Cecil.TypeAttributes.Abstract, null);

        private static HashSet<string>                          Filter = new HashSet<string>()
        {
            "Play.Studio.AOP",
            "Mono.Cecil",
            "Play.Studio.AOP.vshost",
            "Play.Studio.Workbench.vshost"
        };

        /// <summary>
        /// 查询AOP实现
        /// </summary>
        public static void Resolve(string path)
        {
            // 复制路径
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoptemp");
            Directory.Delete(directoryPath, true);
            Directory.CreateDirectory(directoryPath);

            // 查找要实现aop的程序集
            foreach (var asmPath in FileService.SearchDirectory(path, "*.exe", false))
            {
                if (!Filter.Contains(Path.GetFileNameWithoutExtension(asmPath)))
                    ResolveAsm(asmPath);
            }

            foreach (var asmPath in FileService.SearchDirectory(path, "*.dll", false)) 
            {
                if (!Filter.Contains(Path.GetFileNameWithoutExtension(asmPath)))
                    ResolveAsm(asmPath);
            }
        }

        private static void ResolveAsm(string assemblyPath)
        {
            ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            log.Info(string.Format("load source {0}", assemblyPath));

            // 复制路径
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoptemp");
            var targetPath = Path.Combine(directoryPath, Path.GetFileName(assemblyPath));

            log.Info(string.Format("copy {0} to {1}", assemblyPath, targetPath));

            File.Copy(assemblyPath, targetPath, true);
            // copy pdb
            string oPath = Path.GetDirectoryName(assemblyPath);

            var sourceSymbol = oPath + @"\" + Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb";
            var symbolPath = directoryPath + @"\" + Path.GetFileNameWithoutExtension(targetPath) + ".pdb";
            try
            {
                File.Copy(sourceSymbol, symbolPath, true);
            }
            catch (Exception ex) 
            {
                log.Error(string.Format("copy {0} to {1}", assemblyPath, targetPath));
                return;
            }

            Stream stream = new FileStream(targetPath, FileMode.Open);
            var clrAsm = Assembly.Load(stream.ToBytes());
            if (clrAsm.GetCustomAttributes(typeof(AopAssembly), false).Length == 0)
                return;

            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(stream, new ReaderParameters() { ReadSymbols = true });

            MethodReference il2 = asm.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
            MethodReference il3 = asm.MainModule.Import(typeof(Type).GetMethod("GetMethod", new Type[1] { typeof(String) }));
            MethodReference il4 = asm.MainModule.Import(typeof(TypeService).GetMethod("GetAttribute", new Type[2] { typeof(Type), typeof(MemberInfo) }));
            MethodReference il5 = asm.MainModule.Import(typeof(AopAttribute).GetMethod("Invoke"));

            // 搜索并插入调用的il代码
            foreach (TypeDefinition type in asm.MainModule.Types)
            {
                if (!type.HasCustomAttributes) continue;

                foreach (CustomAttribute attribute in type.CustomAttributes)
                {
                    // 查找到实现aop标签的类
                    if (IsAssignableFromAopAttribute(attribute.Constructor.DeclaringType))
                    {
                        var clrType = clrAsm.GetType(type.FullName);
                        if (clrType == null) continue;

                        // 查找其实现基于aop标签的方法
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (!method.HasCustomAttributes) continue;

                            MethodInfo clrMethod = null;
                            try
                            {
                                clrMethod = clrType.GetMethod(method.Name, method.Parameters.Select(X => Type.GetType(X.ParameterType.GetAssemblyQualifiedName())).ToArray());
                            }
                            catch (Exception ex) { }
                            if (clrMethod == null) continue;

                            foreach (CustomAttribute methodAttribute in method.CustomAttributes)
                            {
                                if (IsAssignableFromAopAttribute(methodAttribute.Constructor.DeclaringType))
                                {
                                    var clrAttributeType = Type.GetType(methodAttribute.Constructor.DeclaringType.GetAssemblyQualifiedName());
                                    if (clrAttributeType == null) continue;

                                    var clrAttribute = clrMethod.GetCustomAttributes(clrAttributeType, true)[0] as AopAttribute;
                                    var aopTypes = clrAttribute.AopType;

                                    List<Instruction> fixedOffset = new List<Instruction>();

                                    if (aopTypes == AopTypes.Packed)
                                    {
                                        foreach (Instruction il in method.Body.Instructions)
                                        {
                                            if (il.OpCode == OpCodes.Ret || il.Offset == 0)
                                            {
                                                fixedOffset.Add(il);
                                            }
                                        }
                                    }
                                    else if (aopTypes == AopTypes.Prefixed)
                                    {
                                        foreach (Instruction il in method.Body.Instructions)
                                        {
                                            if (il.Offset == 1)
                                            {
                                                fixedOffset.Add(il);
                                            }
                                        }
                                    }
                                    else if (aopTypes == AopTypes.Suffixed)
                                    {
                                        foreach (Instruction il in method.Body.Instructions)
                                        {
                                            if (il.OpCode == OpCodes.Ret)
                                            {
                                                fixedOffset.Add(il);
                                            }
                                        }
                                    }

                                    var worker = method.Body.GetILProcessor();
                                    foreach (var offset in fixedOffset)
                                    {
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Ldtoken, methodAttribute.Constructor.DeclaringType));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Call, il2));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Ldtoken, type));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Call, il2));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Ldstr, method.Name));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Call, il3));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Call, il4));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Isinst, methodAttribute.Constructor.DeclaringType));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Callvirt, il5));
                                        worker.InsertBefore(offset, worker.Create(OpCodes.Nop));
                                    }

                                    if (aopTypes != AopTypes.None)
                                    {
                                        log.Info(string.Format("resolve aop method : {0} with {1}", clrMethod.ToString(), clrAttributeType.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }



            stream.Close();

            log.Info(string.Format("save {0} to {1}", asm.Name, assemblyPath));
            // 保存
            asm.Write(assemblyPath, new WriterParameters() { WriteSymbols = true });
            // 替换原文件
            //File.Copy(targetPath, assemblyPath, true);
            //File.Copy(symbolPath, sourceSymbol, true);
        }

        private static AssemblyDefinition GetAssemblyDefine(this TypeReference reference) 
        {
            string path;
            try
            {
                if (reference.Scope.Name.Contains(".dll") || reference.Scope.Name.Contains(".exe"))
                {
                    path = reference.Scope.Name;
                }
                else
                {
                    if (File.Exists(reference.Scope.Name + ".dll"))
                    {
                        path = reference.Scope.Name + ".dll";
                    }
                    else if (File.Exists(reference.Scope.Name + ".exe"))
                    {
                        path = reference.Scope.Name + ".exe";
                    }
                    else
                    {
                        return null;
                    }
                }

                if (!AsmDefines.ContainsKey(path))
                    AsmDefines[path] = AssemblyDefinition.ReadAssembly(path);

                return AsmDefines[path];
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        private static TypeDefinition GetTypeDefine(this TypeReference reference) 
        {
            if (reference == null)
                return EmptyTypeDefine;

            var asm = reference.GetAssemblyDefine();
            if (asm == null)
                return EmptyTypeDefine;
            else
                return reference.GetAssemblyDefine().MainModule.GetType(reference.FullName); //.Types[reference.FullName];
        }

        private static string GetAssemblyQualifiedName(this TypeReference reference) 
        {
            try
            {
                if (reference.Scope == null)
                {
                    return string.Empty;
                }
                else if (reference.Scope as ModuleDefinition == null)
                {
                    return reference.FullName + ", " + reference.Scope.ToString();
                }
                else
                {
                    return reference.FullName + ", " + (reference.Scope as ModuleDefinition).Assembly.Name;
                }       
            }
            catch (Exception ex) 
            {
                return string.Empty;
            }
        }

        private static TypeDefinition GetTypeDefine(AssemblyDefinition asm, string fullName) 
        {
            return asm.MainModule.GetType(fullName);
        }

        private static bool IsAssignableFromAopAttribute(TypeReference typeReference)
        {
            bool result = false;
            var define = typeReference.GetTypeDefine();
            while (define != null) 
            {
                if (define.GetAssemblyQualifiedName().Equals(AopFullName)) 
                {
                    result = true;
                    break;
                }

                if (define.BaseType == null)
                    return false;

                define = define.BaseType.GetTypeDefine();
            }

            return result;
        }
    }
}
