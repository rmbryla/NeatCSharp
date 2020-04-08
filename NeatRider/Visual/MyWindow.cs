using System;
using System.Drawing;
using System.Windows.Forms;
using NeatRider.Model;

namespace NeatRider.Visual
{
    public partial class MyWindow : Form
    {
        private Neat neat;
        public Genome genome;
        public MyWindow()
        {
            InitializeComponent();
        }

        private void MyWindow_Load(object sender, EventArgs e)
        {
            drawGenome();
        }

        private void MyWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("key pressed");
        }

        public void setNeat(Neat neat)
        {
            this.neat = neat;
            this.genome = neat.emptyGenome();
            drawGenome();
        }
        

        private void MutateLink_Click(object sender, EventArgs e)
        {
            genome.mutateLink();
            drawGenome();
        }

        private void MutateNode_Click(object sender, EventArgs e)
        {
            genome.mutateNode();
            drawGenome();
        }
        
        private void Calculate_Click(object sender, EventArgs e)
        {
            neat.redrawGenome = true;
            
        }

        public void drawGenome()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.White);
                pen.Width = 3;
                var w = pictureBox1.Width;
                var h = pictureBox1.Height;
                foreach (Node node in genome.Nodes.Values)
                {
                    
                    g.DrawEllipse(pen, (int) (node.X*w), (int) (node.Y*h), 10, 10);
                }
                
                foreach (var con in genome.Connections)
                {
                    var p = new PointF((float) con.from.X * w + 10 , (float) con.from.Y * h + 5);
                    var p2 = new PointF((float) con.to.X * w , (float) con.to.Y * h + 5);
                    if (con.enabled) pen.Color=Color.Green;
                    else pen.Color = Color.Red;
                    g.DrawLine(pen, p, p2);
                    g.DrawString(String.Format("{0:0.000}", con.weight), new Font("Tahoma", 8), Brushes.LimeGreen, (p.X+p2.X)/2, (p.Y+p2.Y)/2+5);
                }
            }

            pictureBox1.Image = bmp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}