using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZXing;
using System.Diagnostics;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;

namespace PosSystem
{
    public partial class frmscanBarcode : Form
    {
        private readonly frmPOS _parentPOSForm;
        private readonly frmProduct _parentProductForm;
        private FilterInfoCollection _filterInfoCollection;
        private VideoCaptureDevice _videoCaptureDevice;
        private readonly BarcodeReader _barcodeReader = new BarcodeReader();
        private readonly Stopwatch _decodeTimer = new Stopwatch();

        private const int DecodeIntervalMs = 100; // Increased for stability
        private string _lastScannedText = string.Empty;
        private DateTime _lastScanTime = DateTime.MinValue;
        private readonly int _debounceMs = 1200;

        private volatile bool _isStopping = false;
        private readonly object _syncLock = new object(); // Prevents overlapping start/stop

        private static string _lastSelectedCamera = null;
        private static string _lastSelectedMP = "Auto-Safe";

        private readonly Pen _redLaserPen = new Pen(Color.Red, 2);
        private readonly Pen _guidePen = new Pen(Color.LimeGreen, 3);
        private VideoCaptureDevice videoSource = null;

        public frmscanBarcode(frmProduct frmProduct = null, frmPOS frmPOS = null)
        {
            InitializeComponent();
            _parentProductForm = frmProduct;
            _parentPOSForm = frmPOS; // Store the POS reference
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Paint += PictureBox_Paint;
            ConfigureScanner();
        }

        private void ConfigureScanner()
        {
            _barcodeReader.AutoRotate = false;
            _barcodeReader.Options = new ZXing.Common.DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.EAN_13, BarcodeFormat.UPC_A, BarcodeFormat.CODE_128 },
                TryHarder = false,
                PureBarcode = false
            };
        }

        private void frmscanBarcode_Load(object sender, EventArgs e)
        {
            // Setup Status Dot
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, pnlStatus.Width, pnlStatus.Height);
            pnlStatus.Region = new Region(gp);
            UpdateStatusLight(false);

            try
            {
                _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cbCamara.Items.Clear();
                foreach (FilterInfo device in _filterInfoCollection) cbCamara.Items.Add(device.Name);

                metroComboBoxMPlevel.Items.Clear();
                metroComboBoxMPlevel.Items.AddRange(new object[] { "Auto-Safe", "0.5 MP", "2 MP", "5 MP", "MAX" });

                if (!string.IsNullOrEmpty(_lastSelectedCamera) && cbCamara.Items.Contains(_lastSelectedCamera))
                    cbCamara.SelectedIndex = cbCamara.Items.IndexOf(_lastSelectedCamera);
                else if (cbCamara.Items.Count > 0) cbCamara.SelectedIndex = 0;

                metroComboBoxMPlevel.SelectedItem = _lastSelectedMP;
                _decodeTimer.Start();
            }
            catch { }
        }

        private void UpdateStatusLight(bool isReady)
        {
            if (pnlStatus.IsDisposed) return;
            if (pnlStatus.InvokeRequired) { pnlStatus.BeginInvoke(new Action(() => UpdateStatusLight(isReady))); return; }
            pnlStatus.BackColor = isReady ? Color.LimeGreen : Color.Red;
        }

        private void StartCamera()
        {
            lock (_syncLock)
            {
                if (cbCamara.SelectedIndex < 0) return;

                try
                {
                    StopCamera(); // Ensure full cleanup first
                    Thread.Sleep(150); // Pause for driver reset

                    _isStopping = false;
                    _videoCaptureDevice = new VideoCaptureDevice(_filterInfoCollection[cbCamara.SelectedIndex].MonikerString);

                    // Hardware Safety Check
                    var capabilities = _videoCaptureDevice.VideoCapabilities;
                    if (capabilities.Length == 0) throw new Exception("Camera reports no capabilities.");

                    _videoCaptureDevice.VideoResolution = SelectResolution(_videoCaptureDevice);
                    _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                    _videoCaptureDevice.Start();

                    btnStart.Enabled = false;
                    UpdateStatusLight(true);
                }
                catch (Exception ex)
                {
                    UpdateStatusLight(false);
                    MessageBox.Show($"Hardware Error: {ex.Message}");
                }
            }
        }

        private VideoCapabilities SelectResolution(VideoCaptureDevice device)
        {
            var caps = device.VideoCapabilities;
            long targetPixels;
            switch (_lastSelectedMP)
            {
                case "0.5 MP": targetPixels = 500000; break;
                case "2 MP": targetPixels = 2000000; break;
                case "5 MP": targetPixels = 5000000; break;
                case "MAX": return caps.OrderByDescending(c => c.FrameSize.Width * c.FrameSize.Height).First();
                default: targetPixels = 921600; break;
            }
            return caps.OrderBy(c => Math.Abs(((long)c.FrameSize.Width * c.FrameSize.Height) - targetPixels)).First();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (_isStopping || IsDisposed) return;

            try
            {
                // CRITICAL: Clone frame to 24bpp to disconnect from hardware memory
                using (Bitmap frame = (Bitmap)eventArgs.Frame.Clone())
                {
                    if (_decodeTimer.ElapsedMilliseconds >= DecodeIntervalMs)
                    {
                        _decodeTimer.Restart();
                        Bitmap processingCopy = (Bitmap)frame.Clone();
                        ThreadPool.QueueUserWorkItem(delegate { ProcessFrame(processingCopy); });
                    }
                    UpdatePreview(frame);
                }
            }
            catch { /* Hardware buffer collision ignored */ }
        }

        private void ProcessFrame(Bitmap frame)
        {
            using (frame)
            {
                try
                {
                    int cropW = (int)(frame.Width * 0.8);
                    int cropH = (int)(frame.Height * 0.4);
                    Rectangle zone = new Rectangle((frame.Width - cropW) / 2, (frame.Height - cropH) / 2, cropW, cropH);

                    using (Bitmap sub = frame.Clone(zone, PixelFormat.Format24bppRgb))
                    {
                        var result = _barcodeReader.Decode(sub);
                        if (result != null && !string.IsNullOrEmpty(result.Text))
                        {
                            this.BeginInvoke(new Action(() => FinalizeScan(result.Text)));
                        }
                    }
                }
                catch { }
            }
        }

        private void UpdatePreview(Bitmap bitmap)
        {
            if (pictureBox.IsDisposed) return;
            pictureBox.BeginInvoke(new MethodInvoker(() =>
            {
                try
                {
                    var oldImage = pictureBox.Image;
                    pictureBox.Image = (Bitmap)bitmap.Clone();
                    oldImage?.Dispose();
                }
                catch { }
            }));
        }

        private void FinalizeScan(string code)
        {
            // 1. Debounce check: Prevent scanning the same item 10 times in 1 second
            if (code == _lastScannedText && (DateTime.Now - _lastScanTime).TotalMilliseconds < _debounceMs) return;

            _lastScannedText = code;
            _lastScanTime = DateTime.Now;

            // 2. Audible feedback
            System.Media.SystemSounds.Beep.Play();

            // 3. Logic for adding to PRODUCT management (if opened from there)
            if (_parentProductForm != null)
            {
                _parentProductForm.txtBarcode.Text = code;
            }

            // 4. Logic for adding to CART (if opened from POS)
            if (_parentPOSForm != null)
            {
                // We use BeginInvoke to ensure the UI updates safely across threads
                _parentPOSForm.BeginInvoke(new Action(() => {
                    _parentPOSForm.AddToCart(code);
                }));
            }
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_videoCaptureDevice == null || _isStopping) return;
            Graphics g = e.Graphics;
            int w = pictureBox.Width, h = pictureBox.Height;

            // Draw the green guide box
            Rectangle targetRect = new Rectangle(w / 4, h / 3, w / 2, h / 3);
            g.DrawRectangle(_guidePen, targetRect);

            // Draw the moving red laser
            // Use Environment.TickCount for the animation instead of Invalidate inside the paint
            int laserY = (h / 2) + (int)(Math.Sin(Environment.TickCount * 0.005) * (h / 8));
            g.DrawLine(_redLaserPen, targetRect.Left + 5, laserY, targetRect.Right - 5, laserY);
        }

        private void StopCamera()
        {
            if (_isStopping) return; // Prevent double execution
            _isStopping = true;
            UpdateStatusLight(false);

            lock (_syncLock)
            {
                if (_videoCaptureDevice != null)
                {
                    // Detach the event handler immediately
                    _videoCaptureDevice.NewFrame -= VideoCaptureDevice_NewFrame;

                    if (_videoCaptureDevice.IsRunning)
                    {
                        _videoCaptureDevice.SignalToStop();
                        // Wait up to 1 second for the hardware to release
                        _videoCaptureDevice.WaitForStop();
                    }
                    _videoCaptureDevice = null;
                }
            }

            // Clear the picturebox to release the last frame's memory
            if (pictureBox.Image != null)
            {
                var img = pictureBox.Image;
                pictureBox.Image = null;
                img.Dispose();
            }

            // Force immediate cleanup of heavy Bitmap buffers
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void frmscanBarcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                StopCamera();

                if (videoSource != null)
                {
                    if (videoSource.IsRunning)
                    {
                        videoSource.SignalToStop();
                        videoSource.WaitForStop();
                    }
                    videoSource = null;
                }
            }
            catch
            {
            }
        }

        private void frmscanBarcode_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                StopCamera();
            }
            catch
            {
            }

            this.Dispose();
        }

        private void btnStart_Click(object sender, EventArgs e) => StartCamera();
        private void pictureBox2_Click(object sender, EventArgs e) => this.Close();

        private void metroComboBoxMPlevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lastSelectedMP = metroComboBoxMPlevel.SelectedItem.ToString();
            if (_videoCaptureDevice != null && _videoCaptureDevice.IsRunning) StartCamera();
        }
    }
}