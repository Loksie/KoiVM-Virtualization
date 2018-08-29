#region

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using dnlib.DotNet;

#endregion

namespace KoiVM.Confuser.Processor
{
    public class Renamer
    {
        private readonly Dictionary<string, string> nameMap = new Dictionary<string, string>();
        private readonly char prefix;
        private readonly Random rand;
        private readonly bool renAll;
        private int seed;

        public string Name
        {
            get
            {
                string a = "_";
                for (int x = 0; x < rand.Next(1, 250); x++)
                {
                    a += "_";
                }

                return a;
            }
        }
        public Renamer(int seed, bool renAll, char prefix)
        {
            this.seed = seed;
            rand = new Random(seed);
            this.renAll = renAll;
            this.prefix = prefix;
        }

        private string NextName()
        {
           // return string.Format("{0}{1:x}", prefix, nameMap.Count);
            return Name;
        }

        private string NewName(string name)
        {
            //string newName;
            //if(!nameMap.TryGetValue(name, out newName)) nameMap[name] = newName = NextName();
            //return newName;
            return Name;
        }

        public string GetNewName(string name)
        {
            //string newName;
            //if(nameMap.TryGetValue(name, out newName))
            //    return newName;
            //return null;
            return Name;
        }

        public void Process(ModuleDef module)
        {
            var list = new List<TypeDef>(module.GetTypes());
            rand.Shuffle(list);
            foreach(var type in list)
            {
                if(!type.IsGlobalModuleType && renAll || type.IsEnum)
                {
                    type.Name = NewName(type.FullName);
                    type.Namespace = "";
                }

                foreach(var genParam in type.GenericParameters)
                    genParam.Name = genParam.Number.ToString();

                var isDelegate = type.BaseType != null &&
                                 (type.BaseType.FullName == "System.Delegate" ||
                                  type.BaseType.FullName == "System.MulticastDelegate");

                foreach(var method in type.Methods)
                {
                    if(method.HasBody && renAll)
                        foreach(var instr in method.Body.Instructions)
                        {
                            var memberRef = instr.Operand as MemberRef;
                            if(memberRef != null)
                            {
                                var typeDef = memberRef.DeclaringType.ResolveTypeDef();

                                if(memberRef.IsMethodRef && typeDef != null)
                                {
                                    var target = typeDef.ResolveMethod(memberRef);
                                    if(target != null && target.IsRuntimeSpecialName)
                                        typeDef = null;
                                }

                                if(typeDef != null && typeDef.Module == module)
                                    memberRef.Name = NewName(memberRef.Name);
                            }
                        }

                    foreach(var arg in method.Parameters)
                        arg.Name = "";
                    if(method.IsRuntimeSpecialName || isDelegate || !renAll)
                        continue;
                    method.Name = NewName(method.Name);
                }
                for(var i = 0; i < type.Fields.Count; i++)
                {
                    var field = type.Fields[i];
                    if(field.IsRuntimeSpecialName)
                        continue;
                    if(renAll || field.IsLiteral)
                        field.Name = NewName(field.Name);
                }
                if(renAll)
                {
                    type.Properties.Clear();
                    type.Events.Clear();
                    type.CustomAttributes.Clear();
                }
            }
        }
    }
}