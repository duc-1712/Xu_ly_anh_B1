using System;
using System.Drawing;
using System.Windows.Forms;

namespace xulyAnh
{
    public partial class CropForm : Form
    {
        private readonly Bitmap originalImage;
        private Rectangle cropRect;
        private bool resizing = false;
        private bool moving = false;
        private Point startPoint;
        private const int handleSize = 10;
        private int resizeDir = -1; // 0:left, 1:top, 2:right, 3:bottom

        public Bitmap CroppedImage { get; private set; } = null!;

        public CropForm(Bitmap image)
        {
            InitializeComponent();
            originalImage = image;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // Khung crop mặc định giữa form
            int initWidth = this.ClientSize.Width / 3;
            int initHeight = this.ClientSize.Height / 3;
            cropRect = new Rectangle(
                (this.ClientSize.Width - initWidth) / 2,
                (this.ClientSize.Height - initHeight) / 2,
                initWidth,
                initHeight);

            // Gắn sự kiện
            this.MouseDown += CropForm_MouseDown;
            this.MouseMove += CropForm_MouseMove;
            this.MouseUp += CropForm_MouseUp;
            this.Paint += CropForm_Paint;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) this.DialogResult = DialogResult.Cancel;
            };
        }

        private void CropForm_Paint(object sender, PaintEventArgs e)
        {
            // Vẽ ảnh gốc
            e.Graphics.DrawImage(originalImage, this.ClientRectangle);

            // Overlay tối
            using (Brush darkBrush = new SolidBrush(Color.FromArgb(150, Color.Black)))
            {
                Region region = new Region(this.ClientRectangle);
                region.Exclude(cropRect);
                e.Graphics.FillRegion(darkBrush, region);
            }

            // Vẽ khung crop
            using (Pen whitePen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(whitePen, cropRect);
            }

            // Vẽ handle
            foreach (Rectangle h in GetAllHandles())
            {
                e.Graphics.FillRectangle(Brushes.White, h);
            }
        }

        private void CropForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y <= topPanel.Height) return; // tránh vùng toolbar

            for (int i = 0; i < 4; i++)
            {
                if (GetHandle(i).Contains(e.Location))
                {
                    resizing = true;
                    resizeDir = i;
                    startPoint = e.Location;
                    return;
                }
            }

            if (cropRect.Contains(e.Location))
            {
                moving = true;
                startPoint = e.Location;
            }
        }

        private void CropForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                cropRect.X += e.X - startPoint.X;
                cropRect.Y += e.Y - startPoint.Y;
                startPoint = e.Location;
                Invalidate();
            }
            else if (resizing)
            {
                switch (resizeDir)
                {
                    case 0: cropRect.X += e.X - startPoint.X; cropRect.Width -= e.X - startPoint.X; break;
                    case 1: cropRect.Y += e.Y - startPoint.Y; cropRect.Height -= e.Y - startPoint.Y; break;
                    case 2: cropRect.Width += e.X - startPoint.X; break;
                    case 3: cropRect.Height += e.Y - startPoint.Y; break;
                }
                startPoint = e.Location;
                Invalidate();
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (GetHandle(i).Contains(e.Location))
                    {
                        this.Cursor = (i == 0 || i == 2) ? Cursors.SizeWE : Cursors.SizeNS;
                        return;
                    }
                }
                this.Cursor = Cursors.Default;
            }
        }

        private void CropForm_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            resizing = false;
            resizeDir = -1;
        }

        private Rectangle GetHandle(int pos)
        {
            switch (pos)
            {
                case 0: return new Rectangle(cropRect.Left - handleSize, cropRect.Top + cropRect.Height / 2 - handleSize / 2, handleSize, handleSize);
                case 1: return new Rectangle(cropRect.Left + cropRect.Width / 2 - handleSize / 2, cropRect.Top - handleSize, handleSize, handleSize);
                case 2: return new Rectangle(cropRect.Right, cropRect.Top + cropRect.Height / 2 - handleSize / 2, handleSize, handleSize);
                case 3: return new Rectangle(cropRect.Left + cropRect.Width / 2 - handleSize / 2, cropRect.Bottom, handleSize, handleSize);
                default: return Rectangle.Empty;
            }
        }

        private Rectangle[] GetAllHandles()
        {
            return new Rectangle[] { GetHandle(0), GetHandle(1), GetHandle(2), GetHandle(3) };
        }

        private void ApplyBtn_Click(object sender, EventArgs e)
        {
            if (cropRect.Width <= 0 || cropRect.Height <= 0)
            {
                MessageBox.Show("Vùng crop không hợp lệ!");
                return;
            }

            float scaleX = (float)originalImage.Width / this.ClientSize.Width;
            float scaleY = (float)originalImage.Height / this.ClientSize.Height;

            Rectangle imgCrop = new Rectangle(
                (int)(cropRect.X * scaleX),
                (int)(cropRect.Y * scaleY),
                (int)(cropRect.Width * scaleX),
                (int)(cropRect.Height * scaleY)
            );

            CroppedImage = new Bitmap(imgCrop.Width, imgCrop.Height);
            using (Graphics g = Graphics.FromImage(CroppedImage))
            {
                g.DrawImage(originalImage, new Rectangle(0, 0, imgCrop.Width, imgCrop.Height), imgCrop, GraphicsUnit.Pixel);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
