using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using System;
using System.Linq;

namespace Calli_To_Calls
{
    class Program
    {
        static void Main(string[] args)
        {
            ModuleDefMD module;
            string filepath;
            try
            {
                filepath = args[0];
                module = ModuleDefMD.Load(filepath);
            }
            catch
            {
                Console.WriteLine("Can't load module, maybe it's not .NET assembly..");
                Console.ReadLine();
                return;
            }
            int callsfixed = 0;
            Console.Title = "Calli fixer by ННчик";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Calli fixer by ННчик");
            foreach (var type in module.Types.ToArray())
            {
                foreach (var method in type.Methods.ToArray())
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        for (var i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            try
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Calli)
                                {
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i - 1].OpCode = OpCodes.Call;
                                    callsfixed++;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"[{callsfixed}] Fixed calli in " + method.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Can't fix - " + ex.Message);
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Fixed " + callsfixed + " calls");
            Console.WriteLine("Fix finished!Module writed in " + filepath.Split('.')[0] + "-callifixed.exe");
            var ModuleWriterOptions = new ModuleWriterOptions(module);
            ModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
            ModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            ModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            ModuleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
            ModuleWriterOptions.MetadataOptions.PreserveHeapOrder(module, true);

            module.Write(filepath.Split('.')[0] + "-callifixed.exe", ModuleWriterOptions);
            Console.ReadLine();
        }
    }
}
