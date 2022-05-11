using System;

namespace Network.Gen
{
    [Serializable]
    public class Net
    {
        public int[] layers = { 5, 3, 2 };
        public string[] layerActivation = {"tanh","tanh","tanh"};
    }
}