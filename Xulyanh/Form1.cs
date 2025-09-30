using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace xulyAnh
{
    public partial class Form1 : Form
    {
        private Image originalImage;
        private Image currentImage;

        private float zoomFactor = 1.0f;   // hệ số zoom
        private float panX = 0;           // dịch ngang
        private float panY = 0;           // dịch dọc

        private bool isPanning = false;
        private Point panStartPoint;

        public Form1()
        {
            InitializeComponent();

            openMenu.Click += openMenu_Click;
            saveMenu.Click += saveMenu_Click;
            exitMenu.Click += (s, e) => this.Close();
            resetMenu.Click += resetMenu_Click;
            cropMenu.Click += cropMenu_Click;

            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.Paint += PictureBox1_Paint;

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;

            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void openMenu_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalImage = Image.FromFile(ofd.FileName);
                currentImage = (Image)originalImage.Clone();
                zoomFactor = 1.0f;
                panX = panY = 0;
                pictureBox1.Invalidate();
            }
        }

        private void saveMenu_Click(object sender, EventArgs e)
        {
            if (currentImage == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JPEG|*.jpg|PNG|*.png|Bitmap|*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                currentImage.Save(sfd.FileName);
                MessageBox.Show("Ảnh đã được lưu!", "Thông báo");
            }
        }

        private void resetMenu_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                currentImage = (Image)originalImage.Clone();
                zoomFactor = 1.0f;
                panX = panY = 0;
                pictureBox1.Invalidate();
            }
        }

        private void cropMenu_Click(object sender, EventArgs e)
        {
            if (currentImage == null) return;

            using (CropForm cf = new CropForm(new Bitmap(currentImage)))
            {
                if (cf.ShowDialog() == DialogResult.OK)
                {
                    currentImage = cf.CroppedImage;
                    zoomFactor = 1.0f;
                    panX = panY = 0;
                    pictureBox1.Invalidate();
                }
            }
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentImage == null) return;

            float oldZoom = zoomFactor;
            if (e.Delta > 0) zoomFactor *= 1.1f;
            else zoomFactor *= 0.9f;

            if (zoomFactor < 0.1f) zoomFactor = 0.1f;
            if (zoomFactor > 10f) zoomFactor = 10f;

            // Giữ điểm zoom tại vị trí con trỏ
            float mouseX = e.X - pictureBox1.Width / 2f - panX;
            float mouseY = e.Y - pictureBox1.Height / 2f - panY;

            panX -= mouseX * (zoomFactor / oldZoom - 1);
            panY -= mouseY * (zoomFactor / oldZoom - 1);

            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // kéo bằng chuột phải
            {
                isPanning = true;
                panStartPoint = e.Location;
                pictureBox1.Cursor = Cursors.Hand;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                panX += e.X - panStartPoint.X;
                panY += e.Y - panStartPoint.Y;
                panStartPoint = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isPanning = false;
                pictureBox1.Cursor = Cursors.Default;
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (currentImage == null) return;

            int newWidth = (int)(currentImage.Width * zoomFactor);
            int newHeight = (int)(currentImage.Height * zoomFactor);

            int x = (pictureBox1.Width - newWidth) / 2 + (int)panX;
            int y = (pictureBox1.Height - newHeight) / 2 + (int)panY;

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(currentImage, new Rectangle(x, y, newWidth, newHeight));
        }
    }
}
