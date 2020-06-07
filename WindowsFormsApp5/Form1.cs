using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp5.Particle;

namespace WindowsFormsApp5
{
    public partial class Form1 : Form
    {
        List<DirectionColorfulEmiter> emiters = new List<DirectionColorfulEmiter>();

        public Form1()
        {
            InitializeComponent();

            // привязали изображение
            picDisplay.Image = new Bitmap(picDisplay.Width, picDisplay.Height);

            var rnd = new Random();
            for (var i = 0; i < 50; ++i)
            {
                emiters.Add(new DirectionColorfulEmiter
                {
                    ParticlesCount = 50,
                    Position = new Point(rnd.Next(picDisplay.Width), -20),
                    Radius = 2 + rnd.Next(5),
                });
            }
        }

        // добавил функцию обновления состояния системы
        private void UpdateState()
        {
            foreach (var emiter in emiters)
            {
                emiter.UpdateState();
            }
        }

        // функция рендеринга
        private void Render(Graphics g)
        {
            foreach (var emiter in emiters)
            {
                emiter.Render(g);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateState(); // каждый тик обновляем систему

            using (var g = Graphics.FromImage(picDisplay.Image))
            {
                g.Clear(Color.Black); // добавил очистку

                Render(g); // рендерим систему
            }

            // обновить picDisplay
            picDisplay.Invalidate();
        }

 

        private void TbDirection_Scroll_1(object sender, EventArgs e)
        {
            foreach (var emiter in emiters)
            {
                emiter.Direction = tbDirection.Value;
            }
        }

        private void TbSpread_Scroll_1(object sender, EventArgs e)
        {
            foreach (var emiter in emiters)
            {
                emiter.Spread = tbSpread.Value;
            }
        }

        private void TbSpeed_Scroll_1(object sender, EventArgs e)
        {
            foreach (var emiter in emiters)
            {
                emiter.Speed = tbSpeed.Value;
            }
        }
    }
}
