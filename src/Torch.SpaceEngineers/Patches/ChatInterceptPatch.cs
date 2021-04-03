using HarmonyLib;
using Sandbox.Engine.Multiplayer;
using Torch.SpaceEngineers.Chat;
using VRage.Network;

namespace Torch.SpaceEngineers.Patches
{
    internal delegate void ChatMessageDel(ref ChatMsg msg, ref bool cancel);

    [HarmonyPatch(typeof(MyMultiplayerBase))]
    [HarmonyPatch("OnChatMessageReceived_Server")]
    internal static class ChatInterceptPatch
    {
        internal static event ChatMessageDel ChatMessageReceived;

        public static bool Prefix(ref ChatMsg msg)
        {
            var cancel = false;
            ChatMessageReceived?.Invoke(ref msg, ref cancel);
            return !cancel;
        }
    }
}
