using System.Runtime.InteropServices;
using System.Text;

namespace AutoClicker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //this.BackColor = Color.White;
            //this.TransparencyKey = Color.White;
            this.TopMost = true;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            timer1.Interval = 50;
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            //pictureBox1.Image = new Bitmap("1.png");
            SetCursePos();
        }
        enum MatchState
        {
            None = 0,
            Pick1 = 1, //��ѡA
            Pick2 = 2, //��ѡB

        };

        MatchState mMatchState = MatchState.None;
        MatchGrid? mLastMatchGrid = null;
        int mShowValue = 1;
        private void Timer1_Tick(object? sender, EventArgs e)
        {

            if (mMatchState == MatchState.Pick1)
            {
                foreach (var grid in AllMatchGrids)
                {
                    if (grid.Valid && !grid.Picked && grid.ImageType == MatchGridType.A)
                    {
                        grid.Picked = true;
                        OnClick(grid.X + MatchGridSize / 2, grid.Y + MatchGridSize / 2, mShowValue);

                        mLastMatchGrid = grid;
                        break;
                    }
                }
                if (mLastMatchGrid == null)
                {
                    SetCursePos();
                    mMatchState = MatchState.None;
                }
                else
                {
                    mMatchState = MatchState.Pick2;
                }
            }
            else if (mMatchState == MatchState.Pick2)
            {
                var grid = mLastMatchGrid.MatchedGrid;
                grid.Picked = true;
                OnClick(grid.X + MatchGridSize / 2, grid.Y + MatchGridSize / 2, mShowValue);
                mLastMatchGrid = null;
                mShowValue++;
                mMatchState = MatchState.Pick1;
            }

        }
        const int AoffsetX = 1522;
        const int AoffsetY = 358;
        const int MatchGridSize = 84; //ͼƬ��С84x84
        const int MatchGridCol = 3;
        const int MatchGridRow = 4; //4��3��
        const int ATop = 3;
        const int ALeft = 2;
        int[] MatchGridTop = new int[] { 4, 97, 190, 283 };//��93
        int[] MatchGridLeft = new int[] { 2, 96, 190 }; //��94
        class MatchGrid
        {
            public bool Valid = false;
            public bool Picked = false;
            public int X;
            public int Y;
            public Bitmap Picture = new Bitmap(MatchGridSize, MatchGridSize);
            public Bitmap DiffPic;
            public int DebugDiffPicValue;
            public int DiffPicValue;
            public MatchGrid MatchedGrid;
            public MatchGridType ImageType;
            public List<Tuple<int, MatchGrid>> ToMatchGrids = new List<Tuple<int, MatchGrid>>();
        }

        enum MatchGridType
        {
            A, B
        }

        MatchGrid[,] AllMatchGrids = new MatchGrid[3, 4];
        IEnumerator<MatchGrid> mEnumGrid;
        Color ColorB = Color.FromArgb(225, 189, 136);

        //ץ��
        private void button3_Click(object sender, EventArgs e)
        {
            //pictureBox1.Image = CatchScreen();
            Bitmap bmp = new Bitmap("2.png");
            pictureBox1.Image = bmp;
        }

        //��ͼ

        private void button1_Click(object sender, EventArgs e)
        {
            CutPicture(pictureBox1.Image as Bitmap);
        }
        //��
        private void button6_Click(object sender, EventArgs e)
        {
            foreach (var grid in AllMatchGrids)
            {
                grid.Picture = Ruihua(grid.Picture);
            }

            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Graphics g2 = Graphics.FromImage(pictureBox2.Image);
            for (int col = 0; col < MatchGridCol; col++)
            {
                for (int row = 0; row < MatchGridRow; row++)
                {
                    MatchGrid grid = AllMatchGrids[col, row];
                    if (grid.Valid)
                    {
                        g2.DrawImage(grid.Picture, new Rectangle(grid.X, grid.Y, MatchGridSize, MatchGridSize));
                    }
                }
            }
            g2.Dispose();
            mEnumGrid = MatchResult();
        }

        //ƥ��
        private void button4_Click(object sender, EventArgs e)
        {
            MatchPicture();
        }

        //����
        private void button2_Click(object sender, EventArgs e)
        {
            if (mEnumGrid.MoveNext())
            {
                MatchGrid grid = mEnumGrid.Current;
                Graphics g2 = pictureBox2.CreateGraphics();
                g2.DrawImage(grid.DiffPic, new Rectangle(grid.X, grid.Y, MatchGridSize, MatchGridSize));
                g2.DrawString(grid.DebugDiffPicValue.ToString(), SystemFonts.DefaultFont, Brushes.Yellow, grid.X + 4, grid.Y + 4);
                g2.Dispose();
            }

        }


        //һ��ƥ��
        private void button5_Click(object sender, EventArgs e)
        {
            if (mMatchState == MatchState.None)
            {
                Bitmap imgCatched = CatchScreen();
                pictureBox1.Image = imgCatched;
                CutPicture(imgCatched);
                MatchPicture();
            }
        }



        //��ͼ
        void CutPicture(Bitmap bm)
        {
            Graphics graphics = Graphics.FromImage(bm);
            for (int col = 0; col < MatchGridCol; col++)
            {
                for (int row = 0; row < MatchGridRow; row++)
                {
                    AllMatchGrids[col, row] = new MatchGrid();
                    MatchGrid grid = AllMatchGrids[col, row];
                    grid.X = ALeft + col * 94;// MatchGridLeft[col];
                    grid.Y = ATop + row * 93;// MatchGridTop[row];
                    if (HasPicture(grid, bm))
                    {
                        grid.Valid = true;
                        if (IsImageB(grid, bm))
                        {
                            grid.ImageType = MatchGridType.B;
                        }
                        else
                        {
                            grid.ImageType = MatchGridType.A;
                        }
                        for (int iy = 0; iy < MatchGridSize; iy++)
                        {
                            int y = grid.Y + iy;
                            for (int ix = 0; ix < MatchGridSize; ix++)
                            {
                                int x = grid.X + ix;
                                Color pixel = bm.GetPixel(x, y);
                                grid.Picture.SetPixel(ix, iy, pixel);
                            }
                        }
                    }
                }
            }

            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Graphics g2 = Graphics.FromImage(pictureBox2.Image);
            g2.Clear(Color.White);
            for (int col = 0; col < MatchGridCol; col++)
            {
                for (int row = 0; row < MatchGridRow; row++)
                {
                    MatchGrid grid = AllMatchGrids[col, row];
                    if (grid.Valid)
                    {
                        g2.DrawImage(grid.Picture, new Rectangle(grid.X, grid.Y, MatchGridSize, MatchGridSize));
                    }
                }
            }
            g2.Dispose();


            mEnumGrid = MatchResult();
        }
        IEnumerator<MatchGrid> MatchResult()
        {
            foreach (var grid in AllMatchGrids)
            {
                if (grid.Valid && grid.ImageType == MatchGridType.A)
                {
                    foreach (MatchGrid grid2 in AllMatchGrids)
                    {
                        if (grid2.Valid && grid2.ImageType == MatchGridType.B)
                        {
                            PicDiff(grid, grid2);
                            yield return grid2;
                        }
                    }
                }
            }

        }

        void MatchPicture()
        {
            foreach (var grid in AllMatchGrids)
            {
                if (grid.Valid && grid.ImageType == MatchGridType.A && grid.MatchedGrid == null)
                {
                    int minv = int.MaxValue;
                    MatchGrid ming = null;
                    foreach (MatchGrid grid2 in AllMatchGrids)
                    {
                        if (grid2.Valid && grid2.ImageType == MatchGridType.B )
                        {
                            int v = PicDiffValue(grid.Picture, grid2.Picture);
                            if (v < minv)
                            {
                                minv = v;
                                ming = grid2;
                            }
                        }
                    }
                    grid.MatchedGrid = ming;
                    //if(ming.MatchedGrid != null)
                    //{
                    //    MessageBox.Show("double choose me!");
                    //}
                    //ming.MatchedGrid = grid;
                }
            }

            mShowValue = 1;
            mMatchState = MatchState.Pick1;

        }

        //ʶ�𴰿�
        void GetWindow()
        {
            IntPtr MatchWindowHandler;
            MatchWindowHandler = FakeInput.FindWindow(null, "����Эͬ"); //nullΪ������������Spy++�õ���Ҳ����Ϊ��
            FakeInput.ShowWindow(MatchWindowHandler, FakeInput.SW_RESTORE); //�����ڻ�ԭ
            FakeInput.SetForegroundWindow(MatchWindowHandler); //���û��ShowWindow���˷�������������С���Ĵ���
            FakeInput.RECT rect = new FakeInput.RECT();
            FakeInput.GetWindowRect(MatchWindowHandler, ref rect);
            ClientSize = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top - 28);
            Location = new Point(rect.Left - 8, rect.Top);

        }
        //ץȡ��Ļ
        Bitmap CatchScreen()
        {
            Bitmap imgScreen = new Bitmap(280, 372);
            Graphics gg = Graphics.FromImage(imgScreen);
            gg.CopyFromScreen(new Point(AoffsetX, AoffsetY), new Point(0, 0), new Size(280, 372));
            gg.Dispose();
            //imgScreen.Save($"{DateTime.Now.Ticks}.png");
            return imgScreen;
        }

        bool HasPicture(MatchGrid grid, Bitmap bm)
        {
            Color bgColor = Color.FromArgb(242, 195, 130);
            for (int i = 0; i < MatchGridSize; i++)
            {
                Color c = bm.GetPixel(grid.X + i, grid.Y + i);
                if (DiffColor(bgColor, c, 20))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsImageB(MatchGrid grid, Bitmap bm)
        {
            int n = 0;
            for (int i = 0; i < MatchGridSize; i++)
            {
                for (int j = 0; j < MatchGridSize; j++)
                {
                    Color c = bm.GetPixel(grid.X + i, grid.Y + j);
                    if (!DiffColor(c, ColorB, 5))
                    {
                        n++;
                    }
                }
            }
            if (n > 200) { return true; }
            return false;
        }

        bool DiffColor(Color a, Color b, int v)
        {
            if (Math.Abs(a.R - b.R) < v && Math.Abs(a.G - b.G) < v && Math.Abs(a.B - b.B) < v)
            {
                return false;
            }
            return true;
        }
        void OnClick(int x, int y, int v)
        {
            //����
            Graphics g = pictureBox1.CreateGraphics();
            g.DrawString(v.ToString(), SystemFonts.DefaultFont, Brushes.Red, x, y);
            g.Dispose();
            x += AoffsetX;
            y += AoffsetY;
            //ģ����
            FakeInput.mouse_event(FakeInput.MOUSEEVENTF_LEFTDOWN | FakeInput.MOUSEEVENTF_LEFTUP | FakeInput.MOUSEEVENTF_ABSOLUTE | FakeInput.MOUSEEVENTF_MOVE, x * 65536 / 1920, y * 65536 / 1080, 0, 0);
        }

        void SetCursePos()
        {
            Point pos = button5.PointToScreen(new Point(30, 15));
            int x = pos.X;
            int y = pos.Y;
            //ģ����
            FakeInput.mouse_event(FakeInput.MOUSEEVENTF_ABSOLUTE | FakeInput.MOUSEEVENTF_MOVE, x * 65536 / 1920, y * 65536 / 1080, 0, 0);
        }

        void PicDiff(MatchGrid ga, MatchGrid gb)
        {
            Bitmap a = ga.Picture;
            Bitmap b = gb.Picture;
            Bitmap c = new Bitmap(a.Width, a.Height);
            int ret = 0;
            for (int i = 0; i < MatchGridSize; i++)
            {
                 
                for (int j = 0; j < MatchGridSize; j++)
                {
                    Color ca = a.GetPixel(i, j);
                    Color cb = b.GetPixel(i, j);
                    if (!DiffColor(ca, cb, 10))
                    {
                        ret += 0;
                        c.SetPixel(i, j, Color.Black);
                    
                    }
                    else if(!DiffColor(cb, ColorB, 10))
                    {
                        Color bg = a.GetPixel(4, j);//ref bg
                        if (!DiffColor(ca, bg, 10))
                        {
                            ret += 1;
                            c.SetPixel(i, j, Color.White);
                        }
                        else
                        {
                            c.SetPixel(i, j, Color.Black);
                        }
                    }
                    else
                    {
                        ret += 1;
                        c.SetPixel(i, j, Color.White);
                    }
                }
            }
            gb.DebugDiffPicValue = ret;
            gb.DiffPic = c;
        }

        int PicDiffValue(Bitmap a, Bitmap b)
        {
            int ret = 0;
            for (int i = 0; i < MatchGridSize; i++)
            {
                for (int j = 0; j < MatchGridSize; j++)
                {
                    Color ca = a.GetPixel(i, j);
                    Color cb = b.GetPixel(i, j);
                    if (!DiffColor(ca, cb, 10))
                    {
                        ret += 0;

                    }
                    else if (!DiffColor(cb, ColorB, 10))
                    {
                        Color bg = a.GetPixel(4, j);//ref bg
                        if (!DiffColor(ca, bg, 10))
                        {
                            ret += 1;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        ret += 1;
                    }
                }
            }
            return ret;
        }

        Bitmap Ruihua(Bitmap oldBitmap)
        {
            int Height = oldBitmap.Height;
            int Width = oldBitmap.Width;
            Bitmap newBitmap = new Bitmap(Width, Height);
            Color pixel;
            //������˹ģ��
            int[] Laplacian = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    int r = 0, g = 0, b = 0;
                    int Index = 0;
                    for (int col = -1; col <= 1; col++)
                        for (int row = -1; row <= 1; row++)
                        {
                            if (x + row < 0 || x + row >= Width || y + col < 0 || y + col >= Height)
                            {
                                pixel = Color.Black;
                            }
                            else
                            {
                                pixel = oldBitmap.GetPixel(x + row, y + col);
                            }

                            r += pixel.R * Laplacian[Index];
                            g += pixel.G * Laplacian[Index];
                            b += pixel.B * Laplacian[Index];
                            Index++;
                        }
                    //������ɫֵ���
                    r = r > 255 ? 255 : r;
                    r = r < 0 ? 0 : r;
                    g = g > 255 ? 255 : g;
                    g = g < 0 ? 0 : g;
                    b = b > 255 ? 255 : b;
                    b = b < 0 ? 0 : b;
                    newBitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }

            //this.pictureBox2.Image = newBitmap;
            return newBitmap;
        }

    }


    class FakeInput
    {

        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndlnsertAfter, int X, int Y, int cx, int cy, uint Flags);
        /// <summary>
        /// ��ȡָ�����ڵ��豸����
        /// </summary>
        /// <param name="hwnd">����ȡ���豸�����Ĵ��ڵľ������Ϊ0����Ҫ��ȡ������Ļ��DC</param>
        /// <returns>ָ�����ڵ��豸���������������Ϊ0</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        /// <summary>
        /// �ͷ��ɵ���GetDC������ȡ��ָ���豸����
        /// </summary>
        /// <param name="hwnd">Ҫ�ͷŵ��豸������صĴ��ھ��</param>
        /// <param name="hdc">Ҫ�ͷŵ��豸�������</param>
        /// <returns>ִ�гɹ�Ϊ1������Ϊ0</returns>
        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        /// <summary>
        /// ��ָ�����豸������ȡ��һ�����ص�RGBֵ
        /// </summary>
        /// <param name="hdc">һ���豸�����ľ��</param>
        /// <param name="nXPos">�߼�������Ҫ���ĺ�����</param>
        /// <param name="nYPos">�߼�������Ҫ����������</param>
        /// <returns>ָ�������ɫ</returns>
        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;                             //��������
            public int Top;                             //��������
            public int Right;                           //��������
            public int Bottom;                        //��������
        }
        //ShowWindow����
        public const int SW_SHOWNORMAL = 1;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWNOACTIVATE = 4;
        //SendMessage����
        public const int WM_KEYDOWN = 0X100;
        public const int WM_KEYUP = 0X101;
        public const int WM_SYSCHAR = 0X106;
        public const int WM_SYSKEYUP = 0X105;
        public const int WM_SYSKEYDOWN = 0X104;
        public const int WM_CHAR = 0X102;
        //�ƶ���� 
        public const int MOUSEEVENTF_MOVE = 0x0001;
        //ģ������������ 
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //ģ��������̧�� 
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        //ģ������Ҽ����� 
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //ģ������Ҽ�̧�� 
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //ģ������м����� 
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //ģ������м�̧�� 
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //��ʾ�Ƿ���þ������� 
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public Color GetColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        /// <summary>
        ��/// ����һ���ַ���
        ��/// </summary>
        ��/// <param name="myIntPtr">���ھ��</param>
        ��/// <param name="Input">�ַ���</param>
        public static void InputStr(IntPtr myIntPtr, string Input)
        {
            byte[] ch = (ASCIIEncoding.ASCII.GetBytes(Input));
            for (int i = 0; i < ch.Length; i++)
            {
                FakeInput.SendMessage(myIntPtr, WM_CHAR, ch[i], 0);
            }
        }
    }

}