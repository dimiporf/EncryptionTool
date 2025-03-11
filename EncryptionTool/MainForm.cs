#nullable enable
using System.Diagnostics;
using System.Security.Cryptography;

namespace EncryptionTool
{
    public partial class MainForm : Form
    {
        #region Μεταβλητές κλάσης
        // Manages the cancellation of an encryption/decryption process
        private CancellationTokenSource? _cts = null;
        private Stopwatch? _stopwatch = null;
        private bool _isRunning = false; // If the process is running (true) or inactive (false)
        private long _totalBytes = 0; // The total size of the file being processed

        #endregion

        #region Constructors και Events
        public MainForm()
        {
            InitializeComponent();

            // Adds event handlers for form loading and resizing
            this.Load += MainForm_Load;
            this.Resize += MainForm_Resize;
            this.MinimumSize = new Size(660, 220);
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            SetIdleState(); // Sets the application to an idle state
            rdoEncrypt.Checked = true;
            txtKey.UseSystemPasswordChar = true;
            btnStartCancel.Text = "Start";
            CenterPanel();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            CenterPanel();  // Keeps the elements centered when resizing
        }

        private void SetIdleState()
        {
            progressBar1.IsIdle = true;
            progressBar1.BarColor = Color.FromArgb(192, 255, 192); // Resets the progress bar to the default green color
            progressBar1.Value = 0;
            progressBar1.ProgressText = "Waiting for user instructions";
            lblStatus.Text = "Idle";
        }

        #endregion

        #region Διεργασίες κουμπιών

        private void btnBrowse_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (rdoDecrypt.Checked)
                {
                    ofd.Filter = "Encrypted files (*.crpt)|*.crpt";
                    ofd.Title = "Select an encrypted file (.crpt)";
                }
                else
                {
                    ofd.Filter = "All Files (*.*)|*.*";
                    ofd.Title = "Select a file to encrypt";
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                }
            }
        }

        private void btnStartCancel_Click(object? sender, EventArgs e)
        {
            if (!_isRunning)
            {
                StartOperation();
            }
            else
            {
                CancelOperation();
            }
        }
        private void chkShowKey_CheckedChanged(object? sender, EventArgs e)
        {
            txtKey.UseSystemPasswordChar = !chkShowKey.Checked;
        }
        #endregion

        #region Εκτέλεση κρυπτογράφησης/αποκρυπτογράφησης
        private async void StartOperation()
        {
            // 1) Check if a valid file is selected
            string filePath = txtFilePath.Text;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Please select a valid file.");
                return;
            }

            // 2) Check if a key is provided
            string key = txtKey.Text;
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Please enter a key.");
                return;
            }

            // 3) Retrieve the file name and its directory
            bool encryptMode = rdoEncrypt.Checked; // true for Encrypt , false for Decrypt
            string fileName = Path.GetFileName(filePath);
            string? directory = Path.GetDirectoryName(filePath); 

            // If the directory is null, set it to an empty string
            directory ??= string.Empty;

            // 4) Prevent re-encrypting a file that is already .crpt
            if (encryptMode && fileName.EndsWith(".crpt", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This file is already *.crpt. Cannot encrypt again.",
                                "Already Encrypted",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            // 5) Generate the output file name
            string outputFile;
            if (encryptMode)
            {
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                outputFile = Path.Combine(directory, baseName + ".crpt");
            }
            else
            {
                // remove .crpt
                if (fileName.EndsWith(".crpt", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = fileName.Substring(0, fileName.Length - ".crpt".Length);
                }
                else
                {
                    fileName += "_decrypted";
                }
                outputFile = Path.Combine(directory, fileName);
            }

            // 6) Ensure the output file does not already exist
            if (File.Exists(outputFile))
            {
                string unique = GetUniqueFilePath(outputFile);
                MessageBox.Show($"\"{outputFile}\" already exists.\nUsing \"{unique}\" instead.",
                                "File Exists",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                outputFile = unique;
            }

            // 7) Prepare for encryption/decryption
            _cts = new CancellationTokenSource();
            _isRunning = true;
            btnStartCancel.Text = "Cancel";
            lblStatus.Text = encryptMode ? "Encrypting..." : "Decrypting...";
            progressBar1.IsIdle = false;
            progressBar1.BarColor = Color.FromArgb(192, 255, 192); // set bar back to green
            progressBar1.Value = 0;
            progressBar1.ProgressText = "0%";

            // 8) Start the stopwatch and store the file's total size
            _stopwatch = Stopwatch.StartNew();
            _totalBytes = new FileInfo(filePath).Length;

            try
            {
                // 9) Run the process on a separate thread to keep the UI responsive
                await System.Threading.Tasks.Task.Run(() =>
                {
                    if (encryptMode)
                    {
                        FileEncryptor.EncryptFile(
                            filePath,
                            outputFile,
                            key,
                            progressCallback: p => UpdateProgress(p), // Updates the progress bar
                            cancelRequested: () => _cts.IsCancellationRequested, // Allows cancellation
                            renameNotification: msg => MessageBox.Show(msg, "Rename Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        );
                    }
                    else
                    {
                        FileEncryptor.DecryptFile(
                            filePath,
                            outputFile,
                            key,
                            progressCallback: p => UpdateProgress(p),
                            cancelRequested: () => _cts.IsCancellationRequested,
                            renameNotification: msg => MessageBox.Show(msg, "Rename Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        );
                    }
                }, _cts.Token);

                // 10) Once complete, update the UI
                lblStatus.Invoke((Action)(() =>
                {
                    lblStatus.Text = "Done.";
                    progressBar1.Value = 100;
                    progressBar1.ProgressText = "100%";
                    ShellHelper.RefreshExplorer(outputFile);
                }));
            }
            catch (OperationCanceledException)
            {
                // If the process is canceled, delete the partial file
                if (File.Exists(outputFile))
                {
                    try { File.Delete(outputFile); } catch { }
                }
                lblStatus.Invoke((Action)(() => lblStatus.Text = "Operation canceled."));
            }
            catch (CryptographicException ce)
            {
                // 12) If the decryption key is wrong, show an error
                if (ce.Message.Contains("bad key?"))
                {
                    progressBar1.Invoke((Action)(() =>
                    {
                        progressBar1.BarColor = Color.Yellow;
                        progressBar1.ProgressText = "Wrong Key!";
                    }));
                }

                MessageBox.Show(ce.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Invoke((Action)(() => lblStatus.Text = "Error."));
            }
            catch (Exception ex)
            {
                // 13) Handle any other errors 
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Invoke((Action)(() => lblStatus.Text = "Error."));
            }
            finally
            {
                // 14) Reset UI to idle state after completion
                _isRunning = false;
                _cts?.Dispose();
                btnStartCancel.Invoke((Action)(() => btnStartCancel.Text = "Start"));
                // We can call this to reset in initial state
                // SetIdleState();
            }
        }
        #endregion

        #region Ενημέρωση Progress bar
        private void UpdateProgress(double fraction)
        {
            int percent = (int)(fraction * 100);

            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke((Action)(() =>
                {
                    progressBar1.Value = Math.Min(100, percent);
                }));
            }
            else
            {
                progressBar1.Value = Math.Min(100, percent);
            }

            // ‘Estimation of time
            string estString = "";
            if (_stopwatch != null && _stopwatch.Elapsed.TotalSeconds > 1 && fraction > 0)
            {
                double elapsedSec = _stopwatch.Elapsed.TotalSeconds;
                double bytesSoFar = fraction * _totalBytes;
                double speed = bytesSoFar / elapsedSec;
                double bytesLeft = _totalBytes - bytesSoFar;
                double secLeft = (speed > 0) ? (bytesLeft / speed) : 0;
                var ts = TimeSpan.FromSeconds(secLeft);
                estString = $"Est. time left: {ts:hh\\:mm\\:ss}";
            }

            string text = $"{percent}%";
            if (!string.IsNullOrEmpty(estString))
                text += $"  {estString}";

            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke((Action)(() =>
                {
                    progressBar1.ProgressText = text;
                }));
            }
            else
            {
                progressBar1.ProgressText = text;
            }
        }

        private void CancelOperation()
        {
            _cts?.Cancel();
            lblStatus.Text = "Canceling...";
        }
        #endregion

        #region Βοηθητικές Μέθοδοι
        private void CenterPanel()
        {
            pnlContainer.Left = (this.ClientSize.Width - pnlContainer.Width) / 2;
            pnlContainer.Top = (this.ClientSize.Height - pnlContainer.Height) / 2;
        }

        // Finds a unique filename if a duplicate exists
        private string GetUniqueFilePath(string originalPath)
        {
            if (!File.Exists(originalPath))
                return originalPath;

            string? directory = Path.GetDirectoryName(originalPath);
            directory ??= string.Empty; // if null, use empty
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);

            int counter = 1;
            string newPath;
            do
            {
                string tempFileName = $"{filenameWithoutExt}({counter}){extension}";
                newPath = Path.Combine(directory, tempFileName);
                counter++;
            }
            while (File.Exists(newPath));

            return newPath;
        }
        #endregion
    }
}
