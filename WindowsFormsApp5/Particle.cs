using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; 
using System.Drawing.Imaging;
namespace WindowsFormsApp5
{
    public class Particle
    {
        public int Radius; // радуис частицы
        public float X; // X координата положения частицы в пространстве
        public float Y; // Y координата положения частицы в пространстве

        public float Direction; // направление движения
        public float Speed; // скорость перемещения

        public float Life; // запас здоровья частицы
       
        // добавили генератор случайных чисел
        public static Random rand = new Random();

        // метод генерации частицы
        public static Particle Generate()
        {
            return new Particle
            {
                Direction = rand.Next(360),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public virtual void Draw(Graphics g)
        {
            // рассчитываем коэффициент прозрачности по шкале от 0 до 1.0
            float k = Math.Min(1f, Life / 100);

            // рассчитываем значение альфа канала в шкале от 0 до 255
            int alpha = (int)(k * 255);

            // создаем цвет из уже существующего, но привязываем к нему еще и значение альфа канала
            var color = Color.FromArgb(alpha, Color.Black);
            var b = new SolidBrush(color);

            // нарисовали залитый кружок радиусом Radius с центром в X, Y
            g.FillEllipse(b, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            // удалили кисть из памяти
            b.Dispose();
        }
    }

    // новый класс для цветных частиц
    public class ParticleColorful : Particle
    {
        // два новых поля под цвет начальный и конечный
        public Color FromColor;
        public Color ToColor;

        // для смеси цветов
        public static Color MixColor(Color color1, Color color2, float k)
        {
            return Color.FromArgb(
                (int)(color2.A * k + color1.A * (1 - k)),
                (int)(color2.R * k + color1.R * (1 - k)),
                (int)(color2.G * k + color1.G * (1 - k)),
                (int)(color2.B * k + color1.B * (1 - k))
            );
        }

        // новйы метод генерации цветных частиц
        public new static ParticleColorful Generate()
        {
            return new ParticleColorful
            {
                Direction = -90 + rand.Next(10),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public override void Draw(Graphics g)
        {
            float k = Math.Min(1f, Life / 100);

            // так как k уменшается от 1 до 0, то порядок цветов обратный
            var color = MixColor(ToColor, FromColor, k);
            var b = new SolidBrush(color);

            g.FillEllipse(b, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            b.Dispose();
        }
    }

    public class ParticleImage : Particle
    {
        public Image image;
        public Color FromColor;
        public Color ToColor;

        public new static ParticleImage Generate()
        {
            return new ParticleImage
            {
                Direction = rand.Next(360),
                Speed = 1 + rand.Next(10),
                Radius = 2 + rand.Next(10),
                Life = 20 + rand.Next(100),
            };
        }

        public override void Draw(Graphics g)
        {
            float k = Math.Min(1f, Life / 100);

            var color = ParticleColorful.MixColor(ToColor, FromColor, k);

            // матрица преобразования цвета
            ColorMatrix matrix = new ColorMatrix(new float[][]{
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий красный цвет на 0
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий зеленый цвет на 0
            new float[] {0, 0, 0, 0, 0}, // умножаем текущий синий цвет на 0
            new float[] {0, 0, 0, k, 0}, // тут подставляем k который прозрачность задает
            new float[] {(float)color.R / 255, (float)color.G / 255, (float)color.B/255, 0, 1F}});

            // устанавливает матрицу в качестве атрибута
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(matrix);

            g.DrawImage(image,
                // куда рисовать
                new Rectangle((int)(X - Radius), (int)(Y - Radius), Radius * 2, Radius * 2),
                // и какую часть исходного изображения брать
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel, // надо передать
                imageAttributes // наши атрибуты с матрицей преобразования
               );
        }
    }

    //
    //
    public abstract class EmiterBase
    {
        List<Particle> particles = new List<Particle>();

        // количество частиц эмитера храним в переменной
        int particleCount = 0;

        public int ParticlesCount
        {
            get
            {
                return particleCount;
            }
            set
            {
                // при изменении этого значения
                particleCount = value;
                // удаляем лишние частицы
                if (value < particles.Count)
                {
                    particles.RemoveRange(value, particles.Count - value);
                }
            }
        }

        // три абстрактных метода
        public abstract void ResetParticle(Particle particle);
        public abstract void UpdateParticle(Particle particle);
        public abstract Particle CreateParticle();

        // тут общая логика обновления состояния эмитера
        public void UpdateState()
        {
            foreach (var particle in particles)
            {
                particle.Life -= 1;
                if (particle.Life < 0)
                {
                    ResetParticle(particle);
                }
                else
                {
                    UpdateParticle(particle);
                }
            }

            for (var i = 0; i < 10; ++i)
            {
                if (particles.Count < 50)
                {
                    particles.Add(CreateParticle());
                }
                else
                {
                    break;
                }
            }
        }

        public void Render(Graphics g)
        {
            foreach (var particle in particles)
            {
                particle.Draw(g);
            }
        }
    }

    //
    //
    public class PointEmiter : EmiterBase
    {
        public Point Position;

        public override Particle CreateParticle()
        {
            var particle = ParticleImage.Generate();
            particle.image = Properties.Resources.w1;

            particle.FromColor = Color.White;
            particle.ToColor = Color.FromArgb(0, Color.Blue);
            particle.X = Position.X;
            particle.Y = Position.Y;
            return particle;
        }

        public override void ResetParticle(Particle particle)
        {
            particle.Life = 20 + Particle.rand.Next(100);
            particle.Speed = 1 + Particle.rand.Next(10);
            particle.Direction = Particle.rand.Next(360);
            particle.Radius = 2 + Particle.rand.Next(10);
            particle.X = Position.X;
            particle.Y = Position.Y;
        }

        public override void UpdateParticle(Particle particle)
        {
            var directionInRadians = particle.Direction / 180 * Math.PI;
            particle.X += (float)(particle.Speed * Math.Cos(directionInRadians));
            particle.Y -= (float)(particle.Speed * Math.Sin(directionInRadians));
        }
    }

    //
    //
    public class DirectionColorfulEmiter : PointEmiter
    {
        public int Direction = -90; // направление частиц
        public int Spread = 10; // разброс частиц
        public int Speed = 1; // скорость частиц
        public int Radius = 2; // радиус частиц
        public int Life = 20; // жизнь частиц
        public Color FromColor = Color.White; // исходный цвет
        public Color ToColor = Color.FromArgb(0, Color.Blue); // конечный цвет

        public override Particle CreateParticle()
        {
            var particle = ParticleImage.Generate();
            particle.image = Properties.Resources.w1;

            particle.FromColor = this.FromColor;
            particle.ToColor = Color.FromArgb(0, this.ToColor);
            particle.Direction = this.Direction + Particle.rand.Next(-Spread / 2, Spread / 2);
            particle.Speed = this.Speed + Particle.rand.Next(10);
            particle.Radius = this.Radius + Particle.rand.Next(5);
            particle.Life = this.Life + Particle.rand.Next(100);

            particle.X = Position.X;
            particle.Y = Position.Y;
            return particle;
        }

        public override void ResetParticle(Particle particle)
        {
            var particleColorful = particle as ParticleImage;
            if (particleColorful != null)
            {
                particleColorful.Life = this.Life + Particle.rand.Next(100);
                particleColorful.Speed = this.Speed + Particle.rand.Next(10);
                particleColorful.Radius = this.Radius + Particle.rand.Next(10);

                particleColorful.FromColor = this.FromColor;
                particleColorful.ToColor = Color.FromArgb(0, this.ToColor);
                particleColorful.Direction = this.Direction + Particle.rand.Next(-Spread / 2, Spread / 2);

                particleColorful.X = Position.X;
                particleColorful.Y = Position.Y;
            }
        }
    }

}
