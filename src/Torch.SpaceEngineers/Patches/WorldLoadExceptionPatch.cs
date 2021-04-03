using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Sandbox;

namespace Torch.SpaceEngineers.Patches
{
    [HarmonyPatch(typeof(MySandboxGame), "InitQuickLaunch")]
    internal class WorldLoadExceptionPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var msil = instructions.ToList();
            for (var i = 0; i < msil.Count; i++)
            {
                if (msil[i].blocks.All(x => x.blockType != ExceptionBlockType.BeginCatchBlock))
                    continue;

                for (; i < msil.Count; i++)
                {
                    if (msil[i].opcode != OpCodes.Leave)
                        continue;

                    msil[i].opcode = OpCodes.Rethrow;
                    break;
                }
            }

            return msil;
        }
    }
}