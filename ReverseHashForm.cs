using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ReverseHash
{
    public partial class ReverseHashForm : Form, IAppendText
    {
        public ReverseHashForm()
        {
            InitializeComponent();
        }

        #region IAppendText Members

        public void AppendText(string s)
        {
            if (IsHandleCreated && InvokeRequired)
            {
                // Invoke an anonymous method on the thread of the form.
                this.Invoke((MethodInvoker)delegate
                {
                    AppendText(s);
                });
            }
            else
            {
                txtOutput.AppendText(s);
            }
        }

        private void SetGoButtonText(string s)
        {
            if (IsHandleCreated && InvokeRequired)
            {
                // Invoke an anonymous method on the thread of the form.
                this.Invoke((MethodInvoker)delegate {
                    SetGoButtonText(s);
                });
            }
            else
            {
                btnGo.Text = s;
            }
        }

        #endregion

        Thread workerThread = null;

        private void btnGo_Click(object sender, EventArgs e)
        {
            if (workerThread == null)
            {
                txtOutput.Clear();
                _prefix = txtPrefix.Text;
                string hashStr = txtHash.Text;

                if (hashStr.StartsWith("0x"))
                    _hashToLookUp = Convert.ToUInt32(hashStr.Substring(2), 16);
                else
                    _hashToLookUp = Convert.ToUInt32(hashStr);
                _maxLen = (int)spinLen.Value;
                _possibleChars = txtPossibleCharacters.Text;

                btnGo.Text = "Stop";
                workerThread = new Thread(new ThreadStart(DoWork));
                workerThread.Start();
            }
            else
            {
                workerThread.Abort();
                workerThread = null;
                SetGoButtonText("&Go");
                AppendText("Worker Thread killed");
            }
        }

        private string _prefix = "";
        private string _possibleChars = "";
        private UInt32 _hashToLookUp = 0;
        int _maxLen = 0;

        private void DoWork()
        {
            //HashReverse hr = new HashReverse();
            ParallelReverseHash hr = new ParallelReverseHash();
            hr.AppendObj = this;
            AppendText( string.Format("Running command:\n  ReverseHash 0x{0} -p:{1} -c:{2} -l:{3}\n", 
                _hashToLookUp, _prefix, _possibleChars, _maxLen));
            hr.PrintMatches(_hashToLookUp, _possibleChars,
               _prefix.Length + 1, _prefix.Length + _maxLen, _prefix);
            SetGoButtonText("&Go");
        }

        private void bthShowCmdLine_Click(object sender, EventArgs e)
        {
            _prefix = txtPrefix.Text;
            string hashStr = txtHash.Text;

            if (hashStr.StartsWith("0x"))
                _hashToLookUp = Convert.ToUInt32(hashStr.Substring(2), 16);
            else
                _hashToLookUp = Convert.ToUInt32(hashStr);
            _maxLen = (int)spinLen.Value;
            _possibleChars = txtPossibleCharacters.Text;

            AppendText(string.Format("  ReverseHash.exe 0x{0} -p:{1} -c:{2} -l:{3}\n",
                    _hashToLookUp, _prefix, _possibleChars, _maxLen));
        }
    }
}
