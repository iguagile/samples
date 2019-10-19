using System;
using System.Collections.Generic;
using System.Text;

namespace IguagileChat
{
    public class RpcMessageReceiver
    {
        private readonly StringBuilder builder = new StringBuilder();

        public void WriteMessage(string message)
        {
            lock (builder)
            {
                builder.Append(message);
                builder.Append("\n");
                Console.Clear();
                Console.WriteLine(builder);
            }
        }
    }
}
