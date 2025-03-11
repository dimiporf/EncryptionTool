#nullable enable

namespace EncryptionTool
{
    public class CustomProgressBar : ProgressBar
    {
        // Indicates whether the progress bar is in idle state.
        private bool _isIdle;

        // The text displayed on the progress bar.
        private string _progressText = "";

        private static readonly Color WhiteColor = Color.White;        
        private static readonly Color LightGreenColor = Color.FromArgb(192, 255, 192);       
        private Color _barColor = LightGreenColor;

        // Determines if the progress bar is idle (i.e., no process is running).
        public bool IsIdle
        {
            get => _isIdle;
            set
            {
                _isIdle = value;
                Invalidate(); // Refreshes the control to redraw it.
            }
        }
        
        public Color BarColor
        {
            get => _barColor;
            set
            {
                _barColor = value;
                Invalidate();
            }
        }

        // The text that appears over the progress bar.
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                Invalidate();
            }
        }

        public CustomProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;
            _isIdle = true;
        }

        /* Handles the custom rendering of the progress bar.
           This method is automatically called whenever the control needs to be repainted.
           It fills the background, calculates the progress percentage, and draws the filled portion.
           Although it shows 0 (zero) references, it is used to dynamically create and edit our progress bar */
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = e.ClipRectangle;

            if (_isIdle)
            {
                using (var whiteBrush = new SolidBrush(WhiteColor))
                {
                    e.Graphics.FillRectangle(whiteBrush, rect);
                }
                DrawText(e);
                return;
            }

            // If not idle, calculate the progress percentage.
            float fraction = (float)Value / Maximum;

            using (var whiteBrush = new SolidBrush(WhiteColor))
            {
                e.Graphics.FillRectangle(whiteBrush, rect);
            }

            // Draw the filled portion of the progress bar with the specified color (default: green, error: yellow).
            int fillWidth = (int)(rect.Width * fraction);
            if (fillWidth > 0)
            {
                var fillRect = new Rectangle(rect.X, rect.Y, fillWidth, rect.Height);
                using (var barBrush = new SolidBrush(_barColor))
                {
                    e.Graphics.FillRectangle(barBrush, fillRect);
                }
            }

            // Display the progress text inside the bar.
            DrawText(e);
        }

        private void DrawText(PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(_progressText))
                return;

            using (Font f = new Font("Segoe UI", 10))
            {
                // Calculate the position to center the text in the bar.
                SizeF size = e.Graphics.MeasureString(_progressText, f);
                float x = (Width - size.Width) / 2;
                float y = (Height - size.Height) / 2;
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    e.Graphics.DrawString(_progressText, f, textBrush, x, y);
                }
            }
        }
    }
}
