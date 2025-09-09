using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSimulation.UI
{
    public class ThreadSafeTextBoxWriter : TextWriter
    {
        private ListBox listBox;
        public ThreadSafeTextBoxWriter(ListBox listBox)
        {
            this.listBox = listBox;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine()
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => {
                    listBox.Items.Add("");
                    AutoScrollToBottom();
                }));
            }
            else
            {
                listBox.Items.Add("");
                AutoScrollToBottom();
            }
        }

        public override void WriteLine(string? value)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => {
                    listBox.Items.Add(value ?? "");
                    AutoScrollToBottom();
                }));
            }
            else
            {
                listBox.Items.Add(value ?? "");
                AutoScrollToBottom();
            }
        }

        private void AutoScrollToBottom()
        {
            if (listBox.Items.Count > 0)
            {
                listBox.TopIndex = listBox.Items.Count - 1;
            }
        }
    }
}
