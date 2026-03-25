namespace WinFormsApp1
{
    using System;
    using System.Collections.Generic;
    using System.Net.Quic;
    using System.Windows.Forms;
    using RJCP.IO.Ports;

    public partial class Form1 : Form
    {
        private readonly Dictionary<string, SerialPortStream> m_OpenPorts = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void cmdListPorts_Click(object sender, EventArgs e)
        {
            using var serialPortStream2 = new RJCP.IO.Ports.SerialPortStream();
            lstOutput.Items.Add($"{DateTime.Now}: Ports Available");
            foreach (var portName in serialPortStream2.GetPortNames()) {
                WritePort(portName);
            }
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            string portName = lstOutput.SelectedItem.ToString();
            if (portName.StartsWith("Port: ")) {
                portName = portName[6..];
                if (!m_OpenPorts.ContainsKey(portName)) {
                    SerialPortStream serialPortStream = new(portName, 115200);
                    serialPortStream.Open();
                    m_OpenPorts.Add(portName, serialPortStream);
                    WriteEvent($"Port {portName} opened");
                } else {
                    WriteEvent($"Port {portName} already opened - not opened");
                }
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            string portName = lstOutput.SelectedItem.ToString();
            if (portName.StartsWith("Port: ")) {
                portName = portName[6..];
                if (m_OpenPorts.TryGetValue(portName, out SerialPortStream serialPortStream)) {
                    serialPortStream.Close();
                    m_OpenPorts.Remove(portName);
                    WriteEvent($"Port {portName} closed");
                } else {
                    WriteEvent($"Port {portName} not opened - cannot close");
                }
            }
        }

        private void WriteEvent(string message)
        {
            int index = lstOutput.Items.Add($"{DateTime.Now}: {message}");
            lstOutput.TopIndex = index;
        }

        private void WritePort(string portName)
        {
            int index = lstOutput.Items.Add($"Port: {portName}");
            lstOutput.TopIndex = index;
        }
    }
}
