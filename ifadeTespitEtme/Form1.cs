using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DlibDotNet;
using Newtonsoft.Json;
using Rectangle = DlibDotNet.Rectangle;

namespace ifadeTespitEtme
{
    public partial class Form1 : Form
    {
        string imgYolu;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(open.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                imgYolu = open.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fd = Dlib.GetFrontalFaceDetector())
            using (var sp = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat"))
            {
                // Load image from file
                var img = Dlib.LoadImage<RgbPixel>(imgYolu);

                // Detect all faces
                var faces = fd.Operator(img);
                int yuzAdeti = 0;
                //Console.WriteLine("yuzler"+ faces.ToString());
                foreach (var face in faces)
                {
                    yuzAdeti++;
                    // Find the landmark points for this face
                    var shape = sp.Detect(img, face);
                    //Console.WriteLine("ilk yuz icin landmarklar"+ shape.ToString());
                    // Loop through detected landmarks
                    string path = @"d:\landmarks.txt";
                    data dataObjem = new data();
                    dataObjem.Values = new double[136];
                    dataObjem.Targets = new double[7];
                    //List<data> _data = new List<data>();
                    //_data.Add(new data()
                    //    {

                    //    });


                    for (int i = 0; i < shape.Parts; i++)
                    {
                        var point = shape.GetPart((uint)i);

                        dataObjem.Values[i] = point.X;

                        var rect = new Rectangle(point);
                        Dlib.DrawRectangle(img, rect, color: new RgbPixel(255, 0, 0), thickness: 8);

                        // Console.WriteLine(shape.NativePtr.ToString());
                    }


                    for (int i = 0; i < shape.Parts; i++)
                    {
                        //console.writeline(shape.tostring());
                        var point = shape.GetPart((uint)i);

                        dataObjem.Values[i + 68] = point.Y;
                        //write string to file

                    }
                    if (rdbtnAnger.Checked)
                    {
                        dataObjem.Targets[0] = 1.0;
                    }
                    else if (rdbtnFear.Checked)
                    {
                        dataObjem.Targets[1] = 1.0;
                    }
                    else if (rdbtnComtempt.Checked)
                    {
                        dataObjem.Targets[2] = 1.0;
                    }
                    else if (rdbtnHappy.Checked)
                    {
                        dataObjem.Targets[3] = 1.0;
                    }
                    else if (rdbtnSad.Checked)
                    {
                        dataObjem.Targets[4] = 1.0;
                    }
                    else if (rdbtnSurprise.Checked)
                    {
                        dataObjem.Targets[5] = 1.0;
                    }
                    else if (rdbtnDisgust.Checked)
                    {
                        dataObjem.Targets[6] = 1.0;
                    }
                    string json = JsonConvert.SerializeObject(dataObjem, Formatting.Indented);

                    using (StreamWriter w = File.AppendText(path))
                    {
                        w.WriteLine(json);
                        w.WriteLine(',');
                    }

                    //system.io.file.writealltext(path, json);

                    //using (streamwriter w = file.appendtext(path))
                    //{
                    //    w.writeline(json);
                    //}

                    //Console.Write(shape.Parts);
                }
                // Save the result
                string imagepath = imgYolu.ToString();
                imagepath = imagepath.Substring(imagepath.LastIndexOf("\\"));
                imagepath = imagepath.Remove(0, 1);
                imagepath.TrimEnd('g');
                imagepath.TrimEnd('e');
                imagepath.TrimEnd('p');
                imagepath.TrimEnd('j');
                imagepath.TrimEnd('.');
                string newName = imagepath + "Result.jpg";

                Dlib.SaveJpeg(img, newName);

                pictureBox1.Image = new Bitmap(newName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            }
        }
    }
}

   