using System;
using System.Collections.Generic;
using System.IO;

namespace Federtest
{
    class Program
    {
        const double Schrittlänge = 1;

        List<double> einer = new List<double>();
        List<double> zehntel = new List<double>();
        List<double> hunderstel = new List<double>();

        static void Main(string[] args)
        {
            Federsystem f;
            //f = new Federsystem(new Feder(10), new Gewicht(1));
            //f = new Federsystem(new GeneigteFeder(10, 0, 1000), new Gewicht(1));
            f = new Federsystem(new GeneigteFederTetraeder(550, 0.589), new Gewicht(5));

            //Console.SetWindowSize(274, 72);

            f.Vergleich(@"D:\Studium\Master\HCI\Federtest\Federtest\Daten\daten.csv", new[] { 0.2, 0.1, 0.05, 0.01 }, 3);

            Console.WriteLine("Fertig");
            Console.ReadLine();
        }
    }

    class Federsystem
    {
        public Gewicht Gewicht;
        public Feder Feder;

        public Federsystem(Feder feder, Gewicht gewicht)
        {
            Gewicht = gewicht;
            Feder = feder;
        }

        public List<double> Run(double schrittlänge, double obergrenze = 1)
        {
            List<double> ausgabe = new List<double>();

            for (double i = schrittlänge; Math.Round(i, 2) <= obergrenze; i += schrittlänge)
            {
                double a = Physik.A((Gewicht.F - Feder.F), Gewicht.M);

                //Feder.Dl += Physik.S(a, Gewicht.V, schrittlänge);
                Feder.ChangeDl(Physik.S(a, Gewicht.V, schrittlänge));
                Gewicht.V += Physik.V(a, schrittlänge);

                ausgabe.Add(-Feder.Dl);
            }

            return ausgabe;
        }

        public void Vergleich(string pfad, double[] schrittlängen, double obergrenze = 1)
        {
            Array.Sort(schrittlängen);

            List<double>[] werte = new List<double>[schrittlängen.Length];
            for (int i = 0; i < schrittlängen.Length; i++)
            {
                werte[i] = Run(schrittlängen[i], obergrenze);
            }

            using (StreamWriter writer = new StreamWriter(pfad))
            {
                int spalten = schrittlängen.Length;

                for (int j = 0; j < spalten; j++)
                {
                    writer.Write("0;0;;");
                }
                writer.WriteLine();

                for (int i = 0; i < werte[0].Count; i++)
                {
                    for (int j = 0; j < spalten; j++)
                    {
                        writer.Write(((i + 1) * schrittlängen[j]) + ";" + werte[j][i] + ";;");
                        if (werte[j].Count - 1 == i) spalten--;
                    }
                    writer.WriteLine();
                }
            }
        }
    }

    class Feder
    {
        public double Dl;
        public double K;

        public Feder(double k, double dl = 0)
        {
            Dl = dl;
            K = k;
        }

        public virtual double F
        {
            get
            {
                return K * Dl;
            }
        }

        public virtual void ChangeDl(double delta)
        {
            Dl += delta;
        }
    }

    class GeneigteFeder : Feder
    {
        public double Basis;
        public double L;
        public int Anzahl;

        public double H;

        public GeneigteFeder(double k, double basis, double l, double dl = 0, int anz = 1) : base(k, dl)
        {
            Basis = basis;
            L = l;
            Anzahl = anz;
            H = Math.Sqrt(Math.Pow(L, 2) - Math.Pow(Basis, 2));
        }

        public double Alpha
        {
            get
            {
                return Math.Acos(Basis / (L - Dl));
            }
        }

        public override double F
        {
            get
            {
                return K * Dl * Math.Sin(Alpha) * Anzahl;
            }
        }

        public override void ChangeDl(double delta)
        {
            H -= delta;
            Dl = L - Math.Sqrt(Math.Pow(Basis, 2) + Math.Pow(H, 2));
        }
    }

    class GeneigteFederTetraeder : GeneigteFeder
    {
        public GeneigteFederTetraeder(double k, double l, double dl = 0) : base(k, GetBasis(l), l, dl, 3)
        {

        }

        static double GetBasis(double seite)
        {
            return (Math.Sin(GradZuBogenmaß(30)) / Math.Sin(GradZuBogenmaß(120))) * seite;
        }

        static double GradZuBogenmaß(double grad)
        {
            return Math.PI * (grad / 180);
        }
    }

    class Gewicht
    {
        public double M;
        public double V;

        public Gewicht(double m, double v = 0)
        {
            M = m;
            V = v;
        }

        public double F
        {
            get
            {
                return M * 9.81;
            }
        }
    }

    class Physik
    {
        public static double S(double a, double v, double t)
        {
            return a * Math.Pow(t, 2) + v * t;
        }

        public static double V(double a, double t)
        {
            return a * t;
        }

        public static double A(double F, double m)
        {
            return F / m;
        }
    }
}
