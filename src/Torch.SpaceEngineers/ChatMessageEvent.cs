namespace Torch.SpaceEngineers
{
    public struct ChatMessageEvent
    {
        public readonly ulong SenderId;
        public readonly string SenderName;
        public string Content;
        private bool _isCancelled;

        public bool IsCancelled => _isCancelled;

        public void Cancel() => _isCancelled = true;

        internal ChatMessageEvent(ulong senderId, string senderName, string content)
        {
            SenderId = senderId;
            SenderName = senderName;
            Content = content;
            _isCancelled = false;
        }
    }
}