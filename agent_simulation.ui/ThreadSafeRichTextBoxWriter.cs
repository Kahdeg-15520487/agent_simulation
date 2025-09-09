using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentSimulation.UI
{
    public class ThreadSafeRichTextBoxWriter : TextWriter
    {
        private RichTextBox richTextBox;
        private Action<string, Color>? addLogMessageCallback;

        public ThreadSafeRichTextBoxWriter(RichTextBox richTextBox, Action<string, Color>? addLogMessageCallback = null)
        {
            this.richTextBox = richTextBox;
            this.addLogMessageCallback = addLogMessageCallback;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine()
        {
            WriteLine("");
        }

        public override void WriteLine(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Use the callback to add colored messages instead of direct text manipulation
            if (addLogMessageCallback != null)
            {
                // Determine color based on content patterns
                Color color = GetMessageColor(value);
                addLogMessageCallback(value, color);
            }
            else
            {
                // Fallback to direct manipulation
                if (richTextBox.InvokeRequired)
                {
                    richTextBox.Invoke(new Action(() => AppendColoredText(value, Color.White)));
                }
                else
                {
                    AppendColoredText(value, Color.White);
                }
            }
        }

        private Color GetMessageColor(string message)
        {
            // Determine color based on message content
            if (message.Contains("thinks:") || message.Contains("💭") || message.Contains("Analyzing"))
                return Color.Cyan;
            if (message.Contains("🤖") || message.Contains("performs:") || message.Contains("works on"))
                return Color.Yellow;
            if (message.Contains("✅") || message.Contains("completed") || message.Contains("Mission accomplished"))
                return Color.LightGreen;
            if (message.Contains("❌") || message.Contains("failed") || message.Contains("💀"))
                return Color.Red;
            if (message.Contains("⚠️") || message.Contains("warning") || message.Contains("Life Support:"))
                return Color.Orange;
            if (message.Contains("🎮") || message.Contains("Human") || message.Contains("chose:"))
                return Color.Magenta;
            if (message.Contains("Step") || message.Contains("==="))
                return Color.LightBlue;
            
            return Color.White; // Default color
        }

        private void AppendColoredText(string text, Color color)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text + Environment.NewLine);
            richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to default
            
            // Auto-scroll to bottom
            richTextBox.SelectionStart = richTextBox.Text.Length;
            richTextBox.ScrollToCaret();
        }
    }
}
